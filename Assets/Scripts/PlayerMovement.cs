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
    public float activationRadius = 5f;
    public float angularSpeedMultiplier = 1f;
    public float lineThickness = 0.1f;
    public Button swingToggleButton;

    [Header("Axis Constraints")]
    public bool allowXAxis = true;
    public bool allowYAxis = true;
    public bool allowZAxis = true;

    private LineRenderer swingLine;
    private GameObject swingSphere;
    private bool isSwinging = false;
    private bool swingEnabled = true;
    private Vector3 swingAxis;
    private float angularVelocity;
    private Vector3 storedVelocity;
    private float currentSwingRadius;
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

        if (swingToggleButton != null)
        {
            swingToggleButton.onClick.AddListener(ToggleSwing);
            UpdateButtonColor();
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
        if (isSwinging)
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
        transform.position = swingCenter.position + toPlayer.normalized * currentSwingRadius;

        // Apply axis constraints
        Vector3 constrainedAxis = ApplyAxisConstraints(swingAxis);
        
        // Apply continuous rotation
        float rotationAmount = angularVelocity * angularSpeedMultiplier * Time.fixedDeltaTime;
        transform.RotateAround(swingCenter.position, constrainedAxis, rotationAmount);
    }

    Vector3 ApplyAxisConstraints(Vector3 axis)
    {
        if (!allowXAxis) axis.x = 0;
        if (!allowYAxis) axis.y = 0;
        if (!allowZAxis) axis.z = 0;
        return axis.normalized;
    }

    void HandleSwingInput()
    {
        if (!swingEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!isSwinging && Vector3.Distance(transform.position, swingCenter.position) <= activationRadius)
            {
                StartSwing();
            }
            else if (isSwinging)
            {
                ReleaseSwing();
            }
        }
    }

    void StartSwing()
    {
        isSwinging = true;
        storedVelocity = rb.velocity;
        currentSwingRadius = Vector3.Distance(transform.position, swingCenter.position);
        
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        // Calculate initial rotation axis
        Vector3 toCenter = (swingCenter.position - transform.position).normalized;
        swingAxis = Vector3.Cross(toCenter, ApplyAxisConstraints(storedVelocity)).normalized;
        
        // Calculate angular velocity
        angularVelocity = storedVelocity.magnitude / currentSwingRadius * Mathf.Rad2Deg;

        swingSphere.SetActive(true);
        swingLine.enabled = true;
    }

    void ReleaseSwing()
    {
        isSwinging = false;
        rb.isKinematic = false;

        // Convert angular velocity to linear velocity
        Vector3 tangentDirection = Vector3.Cross(swingAxis, (transform.position - swingCenter.position)).normalized;
        rb.velocity = tangentDirection * (angularVelocity * currentSwingRadius * Mathf.Deg2Rad);

        swingSphere.SetActive(false);
        swingLine.enabled = false;
    }

    void ToggleSwing()
    {
        swingEnabled = !swingEnabled;
        UpdateButtonColor();
        if (!swingEnabled && isSwinging)
        {
            ReleaseSwing();
        }
    }

    void UpdateButtonColor()
    {
        if (swingToggleButton != null)
        {
            swingToggleButton.GetComponent<Image>().color = swingEnabled ? Color.green : Color.red;
        }
    }

    void UpdateSwingVisuals()
    {
        if (isSwinging)
        {
            swingLine.SetPosition(0, transform.position);
            swingLine.SetPosition(1, swingCenter.position);
        }
    }

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