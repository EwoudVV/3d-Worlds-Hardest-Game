using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
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
    public bool enableDeathClones = false;
    public Vector3 respawnPosition = Vector3.zero;
    public float rotationSpeed = 10f;
    public TMP_Text deathText;
    public Button respawnButton;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isOnMud;
    private bool isOnIce;
    private Vector3 velocity;
    private int groundContactCount;
    private int deathCount;
    private Vector3 currentRespawn;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentRespawn = respawnPosition;
        if (!cameraTransform) cameraTransform = Camera.main.transform;
        if (deathText) deathText.text = ": 0";
        if (respawnButton) respawnButton.onClick.AddListener(TriggerRespawn);
    }

    void Update()
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

        if (cameraForward.magnitude < 0.01f) cameraForward = cameraTransform.up;
        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = (cameraForward * moveZ + cameraRight * moveX).normalized;

        if (Input.GetKeyDown(KeyCode.R))
        {
            TriggerRespawn();
        }
    }

    void FixedUpdate()
    {
        bool grounded = groundContactCount > 0;
        
        if (isOnMud) velocity = moveDirection * (movementSpeed * mudSpeedMultiplier);
        else if (isOnIce)
        {
            if (moveDirection != Vector3.zero)
            {
                velocity += moveDirection * iceAcceleration;
                if (velocity.magnitude > maxIceSpeed) velocity = velocity.normalized * maxIceSpeed;
            }
            else velocity *= iceFriction;
        }
        else velocity = moveDirection * movementSpeed;

        velocity.y = grounded ? 0 : rb.linearVelocity.y * verticalFriction;
        rb.linearVelocity = velocity;

        if (moveDirection != Vector3.zero)
        {
            Vector3 horizontalDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
            Quaternion targetRot = Quaternion.LookRotation(horizontalDirection);
            Vector3 targetEuler = targetRot.eulerAngles;
            targetEuler.y = Mathf.Round(targetEuler.y / 90) * 90;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.Euler(0, targetEuler.y, 0), rotationSpeed * Time.fixedDeltaTime));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("ground") || collision.collider.CompareTag("Mud") || collision.collider.CompareTag("Ice")) groundContactCount++;
        if (collision.collider.CompareTag("Mud")) isOnMud = true;
        if (collision.collider.CompareTag("Ice")) isOnIce = true;
        
        if (collision.collider.CompareTag("Checkpoint"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f)
                {
                    currentRespawn = contact.point + Vector3.up;
                    Checkpoint cp = collision.collider.GetComponent<Checkpoint>();
                    if (cp != null && cp.useCustomPosition) currentRespawn = cp.customRespawnPosition;
                    break;
                }
            }
        }

        if (collision.collider.CompareTag("Finish")) LoadNextScene();
        if (collision.collider.CompareTag("death")) HandleDeath(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("ground") || collision.collider.CompareTag("Mud") || collision.collider.CompareTag("Ice")) groundContactCount--;
        if (collision.collider.CompareTag("Mud")) isOnMud = false;
        if (collision.collider.CompareTag("Ice")) isOnIce = false;
    }

    void HandleDeath(Collision collision)
    {
        if (enableDeathClones && collision != null)
        {
            Vector3 pos = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            CreateDeathClone(pos);
        }
        TeleportToRespawn();
    }

    void CreateDeathClone(Vector3 pos)
    {
        GameObject clone = Instantiate(gameObject, pos, Quaternion.identity);
        Destroy(clone.GetComponent<PlayerMovement>());
        foreach (Collider c in clone.GetComponents<Collider>()) Destroy(c);
        BoxCollider cloneCollider = clone.AddComponent<BoxCollider>();
        Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
        cloneRb.mass = rb.mass;
        foreach (GameObject deathObj in GameObject.FindGameObjectsWithTag("death"))
        {
            Collider deathCollider = deathObj.GetComponent<Collider>();
            if (deathCollider != null)
            {
                Physics.IgnoreCollision(cloneCollider, deathCollider);
            }
        }
    }

    void TeleportToRespawn()
    {
        rb.position = currentRespawn;
        rb.linearVelocity = Vector3.zero;
        deathCount++;
        if (deathText) deathText.text = $": {deathCount}";
    }

    void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(nextSceneIndex);
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

    public void TriggerRespawn()
    {
        HandleDeath(null);
    }
}

public class Checkpoint : MonoBehaviour
{
    public bool useCustomPosition;
    public Vector3 customRespawnPosition;
}

public enum MovementKey
{
    W,
    A,
    S,
    D
}