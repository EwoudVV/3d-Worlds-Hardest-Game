using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // Existing movement variables
    public float movementSpeed = 5f;
    public float mudSpeedMultiplier = 0.5f;
    public float iceAcceleration = 0.1f;
    public float maxIceSpeed = 10f;
    public float iceFriction = 0.98f;
    public float verticalFriction = 0.9f;
    public MovementKey moveUpKey = MovementKey.W;
    public MovementKey moveDownKey = MovementKey.S;
    public MovementKey moveLeftKey = MovementKey.A;
    public MovementKey moveRightKey = MovementKey.D;
    public Transform cameraTransform;

    // Swing system variables
    [Header("Swing Settings")]
    public Transform swingCenter;
    public float swingRadius = 5f;
    public float angularSpeedMultiplier = 1f;
    public float lineThickness = 0.1f;
    public Button swingToggleButton;
    
    private LineRenderer swingLine;
    private GameObject swingSphere;
    private bool isSwinging = false;
    private bool swingEnabled = true;
    private Vector3 swingAxis;
    private float angularVelocity;
    private Vector3 storedVelocity;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isOnMud = false;
    private bool isOnIce = false;
    private Vector3 velocity;
    private int groundContactCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.linearDamping = 0;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        InitializeSwingComponents();
        
        if(swingToggleButton != null)
        {
            swingToggleButton.onClick.AddListener(ToggleSwing);
        }
    }

    void InitializeSwingComponents()
    {
        swingLine = gameObject.AddComponent<LineRenderer>();
        swingLine.material = new Material(Shader.Find("Standard"));
        swingLine.startColor = Color.white;
        swingLine.endColor = Color.white;
        swingLine.startWidth = lineThickness;
        swingLine.endWidth = lineThickness;
        swingLine.enabled = false;

        swingSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        swingSphere.transform.SetParent(swingCenter);
        swingSphere.transform.localPosition = Vector3.zero;
        swingSphere.transform.localScale = Vector3.one * 0.5f;
        swingSphere.GetComponent<Renderer>().material.color = Color.yellow;
        swingSphere.SetActive(false);
        Destroy(swingSphere.GetComponent<Collider>());
    }

    void Update()
    {
        HandleMovementInput();
        HandleSwingInput();
        UpdateSwingVisuals();
    }

    void HandleMovementInput()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(GetKeyCode(moveUpKey))) moveZ = 1f;
        if (Input.GetKey(GetKeyCode(moveDownKey))) moveZ = -1f;
        if (Input.GetKey(GetKeyCode(moveLeftKey))) moveX = -1f;
        if (Input.GetKey(GetKeyCode(moveRightKey))) moveX = 1f;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        if (cameraForward.magnitude < 0.01f)
        {
            cameraForward = cameraTransform.up;
            cameraForward.y = 0;
        }

        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = (cameraForward * moveZ + cameraRight * moveX).normalized;
    }

    void FixedUpdate()
    {
        if(isSwinging)
        {
            HandleSwingRotation();
        }
        else
        {
            HandleNormalMovement();
        }
    }

    void HandleNormalMovement()
    {
        bool isGrounded = groundContactCount > 0;

        if (isOnMud)
        {
            velocity = moveDirection * movementSpeed * mudSpeedMultiplier;
        }
        else if (isOnIce)
        {
            if (moveDirection != Vector3.zero)
            {
                velocity += moveDirection * iceAcceleration;
                if (velocity.magnitude > maxIceSpeed) velocity = velocity.normalized * maxIceSpeed;
            }
            else
            {
                velocity *= iceFriction;
            }
        }
        else
        {
            velocity = moveDirection * movementSpeed;
        }

        if (isGrounded)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y = rb.linearVelocity.y * verticalFriction;
        }

        rb.linearVelocity = velocity;
    }

    void HandleSwingRotation()
    {
        Vector3 toPlayer = transform.position - swingCenter.position;
        float currentRadius = toPlayer.magnitude;
        
        // Maintain fixed radius
        if(currentRadius != swingRadius)
        {
            transform.position = swingCenter.position + toPlayer.normalized * swingRadius;
        }

        // Calculate rotation axis based on initial velocity
        Vector3 rotationDirection = Vector3.Cross(toPlayer, storedVelocity).normalized;
        swingAxis = rotationDirection;

        // Apply continuous rotation
        float angularSpeed = angularVelocity * angularSpeedMultiplier * Time.fixedDeltaTime;
        transform.RotateAround(swingCenter.position, swingAxis, angularSpeed);
    }

    void HandleSwingInput()
    {
        if(!swingEnabled) return;

        if(Input.GetMouseButtonDown(0))
        {
            if(!isSwinging && Vector3.Distance(transform.position, swingCenter.position) <= swingRadius * 1.2f)
            {
                StartSwing();
            }
            else if(isSwinging)
            {
                ReleaseSwing();
            }
        }
    }

    void StartSwing()
    {
        isSwinging = true;
        storedVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Position player at proper radius
        Vector3 toCenter = swingCenter.position - transform.position;
        transform.position = swingCenter.position - toCenter.normalized * swingRadius;

        // Calculate initial angular velocity
        Vector3 radiusVector = transform.position - swingCenter.position;
        Vector3 tangentDirection = Vector3.Cross(radiusVector, Vector3.up).normalized;
        angularVelocity = storedVelocity.magnitude / swingRadius * Mathf.Rad2Deg;

        swingSphere.SetActive(true);
        swingLine.enabled = true;
    }

    void ReleaseSwing()
    {
        isSwinging = false;
        rb.isKinematic = false;

        // Convert angular velocity to linear velocity
        Vector3 radiusVector = transform.position - swingCenter.position;
        Vector3 tangentDirection = Vector3.Cross(radiusVector, swingAxis).normalized;
        rb.linearVelocity = tangentDirection * (angularVelocity * swingRadius * Mathf.Deg2Rad);

        swingSphere.SetActive(false);
        swingLine.enabled = false;
    }

    void ToggleSwing()
    {
        swingEnabled = !swingEnabled;
        if(!swingEnabled && isSwinging)
        {
            ReleaseSwing();
        }
    }

    void UpdateSwingVisuals()
    {
        if(isSwinging)
        {
            swingLine.SetPosition(0, transform.position);
            swingLine.SetPosition(1, swingCenter.position);
        }
    }

    // Existing collision methods
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("ground") || 
            collision.collider.CompareTag("Mud") || 
            collision.collider.CompareTag("Ice"))
        {
            groundContactCount++;
        }

        if (collision.collider.CompareTag("Mud")) isOnMud = true;
        if (collision.collider.CompareTag("Ice")) isOnIce = true;
        if (collision.collider.CompareTag("Finish")) LoadNextScene();
        if (collision.collider.CompareTag("death")) RestartScene();
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("ground") || 
            collision.collider.CompareTag("Mud") || 
            collision.collider.CompareTag("Ice"))
        {
            groundContactCount--;
        }

        if (collision.collider.CompareTag("Mud")) isOnMud = false;
        if (collision.collider.CompareTag("Ice")) isOnIce = false;
    }

    void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    KeyCode GetKeyCode(MovementKey key)
    {
        switch (key)
        {
            case MovementKey.W: return KeyCode.W;
            case MovementKey.A: return KeyCode.A;
            case MovementKey.S: return KeyCode.S;
            case MovementKey.D: return KeyCode.D;
            default: return KeyCode.None;
        }
    }
}

public enum MovementKey
{
    W,
    A,
    S,
    D
}