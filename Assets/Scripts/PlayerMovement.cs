using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {
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
    public Transform respawnPoint;
    public float rotationSpeed = 10f;
    public TMP_Text deathText;
    public Button respawnButton;
    public Transform turnPivot;
    public bool disableRRespawn;
    
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isOnMud;
    private bool isOnIce;
    private Vector3 velocity;
    private int groundContactCount;
    private int deathCount;
    private float targetYRotation;
    private bool manualRotation;
    private bool isRespawning;
    
    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        targetYRotation = transform.eulerAngles.y;
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        if (deathText)
            deathText.text = ": " + deathCount;
        if (respawnButton)
            respawnButton.onClick.AddListener(TriggerRespawn);
    }
    
    void Update() {
        float moveX = 0f, moveZ = 0f;
        if (Input.GetKey(GetKeyCode(moveUpKey))) moveZ = 1f;
        if (Input.GetKey(GetKeyCode(moveDownKey))) moveZ = -1f;
        if (Input.GetKey(GetKeyCode(moveLeftKey))) moveX = -1f;
        if (Input.GetKey(GetKeyCode(moveRightKey))) moveX = 1f;
        Vector3 camForward = cameraTransform.forward, camRight = cameraTransform.right;
        camForward.y = camRight.y = 0f;
        if (camForward.magnitude < 0.01f) camForward = cameraTransform.up;
        camForward.Normalize(); camRight.Normalize();
        moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        if (Input.GetKeyDown(KeyCode.E)) { targetYRotation += 90f; manualRotation = true; }
        if (Input.GetKeyDown(KeyCode.Q)) { targetYRotation -= 90f; manualRotation = true; }
        if (moveDirection != Vector3.zero && !manualRotation)
            targetYRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        if (Input.GetKeyDown(KeyCode.R) && !disableRRespawn)
            TriggerRespawn();
    }
    
    void FixedUpdate() {
        bool grounded = groundContactCount > 0;
        if (isOnMud)
            velocity = moveDirection * (movementSpeed * mudSpeedMultiplier);
        else if (isOnIce) {
            if (moveDirection != Vector3.zero) {
                velocity += moveDirection * iceAcceleration;
                if (velocity.magnitude > maxIceSpeed)
                    velocity = velocity.normalized * maxIceSpeed;
            } else {
                velocity *= iceFriction;
            }
        } else {
            velocity = moveDirection * movementSpeed;
        }
        velocity.y = grounded ? 0 : rb.linearVelocity.y * verticalFriction;
        rb.linearVelocity = velocity;
        float currentAngle = transform.eulerAngles.y;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetYRotation, rotationSpeed * Time.fixedDeltaTime);
        Quaternion newRot = Quaternion.Euler(0, newAngle, 0);
        if (turnPivot != null) {
            float angleDelta = newAngle - currentAngle;
            Vector3 pivotOffset = transform.position - turnPivot.position;
            Vector3 rotatedOffset = Quaternion.Euler(0, angleDelta, 0) * pivotOffset;
            rb.MoveRotation(newRot);
            rb.MovePosition(turnPivot.position + rotatedOffset);
        } else {
            rb.MoveRotation(newRot);
        }
        if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetYRotation)) < 0.1f)
            manualRotation = false;
    }
    
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("bablockyes"))
            collision.gameObject.GetComponent<Renderer>().material.color = Color.green;
        else if (collision.gameObject.CompareTag("bablockno")) {
            collision.gameObject.GetComponent<Renderer>().material.color = Color.red;
            TriggerRespawn();
            return;
        }
        if (collision.collider.CompareTag("ground") || collision.collider.CompareTag("Mud") || collision.collider.CompareTag("Ice"))
            groundContactCount++;
        if (collision.collider.CompareTag("Mud"))
            isOnMud = true;
        if (collision.collider.CompareTag("Ice"))
            isOnIce = true;
        if (collision.collider.CompareTag("Checkpoint")) {
            Checkpoint cp = collision.collider.GetComponent<Checkpoint>();
            if (cp != null && cp.useCustomPoint && cp.customRespawnPoint != null)
                respawnPoint = cp.customRespawnPoint;
            else
                respawnPoint = collision.transform;
        }
        if (collision.collider.CompareTag("Finish"))
            LoadNextScene();
        if (collision.collider.CompareTag("death"))
            HandleDeath(collision);
    }
    
    void OnCollisionStay(Collision collision) {
        if ((collision.collider.CompareTag("bablockno") || collision.collider.CompareTag("death")))
            TriggerRespawn();
    }
    
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("bablockno") || other.CompareTag("death"))
            TriggerRespawn();
    }
    
    void OnCollisionExit(Collision collision) {
        if (collision.collider.CompareTag("ground") || collision.collider.CompareTag("Mud") || collision.collider.CompareTag("Ice"))
            groundContactCount--;
        if (collision.collider.CompareTag("Mud"))
            isOnMud = false;
        if (collision.collider.CompareTag("Ice"))
            isOnIce = false;
    }
    
    void HandleDeath(Collision collision) {
        if (isRespawning) return;
        TeleportToRespawn();
        if (collision != null && collision.collider.CompareTag("death") && enableDeathClones) {
            Vector3 pos = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            CreateDeathClone(pos);
        }
        isRespawning = false;
    }
    
    void CreateDeathClone(Vector3 pos) {
        GameObject clone = Instantiate(gameObject, pos, Quaternion.identity);
        Destroy(clone.GetComponent<PlayerMovement>());
        foreach (Collider c in clone.GetComponents<Collider>())
            Destroy(c);
        BoxCollider cloneCollider = clone.AddComponent<BoxCollider>();
        Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
        cloneRb.mass = rb.mass;
        Physics.IgnoreCollision(cloneCollider, GetComponent<Collider>());
        foreach (GameObject deathObj in GameObject.FindGameObjectsWithTag("death")) {
            Collider deathCollider = deathObj.GetComponent<Collider>();
            if (deathCollider != null)
                Physics.IgnoreCollision(cloneCollider, deathCollider);
        }
    }
    
    void TeleportToRespawn() {
        if (respawnPoint != null) {
            transform.position = respawnPoint.position;
            rb.position = respawnPoint.position;
        }
        rb.linearVelocity = Vector3.zero;
        deathCount++;
        if (deathText)
            deathText.text = $": {deathCount}";
    }
    
    void LoadNextScene() {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
    }
    
    KeyCode GetKeyCode(MovementKey key) {
        switch (key) {
            case MovementKey.W: return KeyCode.W;
            case MovementKey.A: return KeyCode.A;
            case MovementKey.S: return KeyCode.S;
            case MovementKey.D: return KeyCode.D;
            default: return KeyCode.None;
        }
    }
    
    public void TriggerRespawn() {
        TeleportToRespawn();
        isRespawning = false;
    }
}

public class Checkpoint : MonoBehaviour {
    public bool useCustomPoint;
    public Transform customRespawnPoint;
}

public enum MovementKey { W, A, S, D }
