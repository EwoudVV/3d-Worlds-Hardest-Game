using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Death Settings")]
    public bool enableDeathClones = false;
    public Vector3 respawnPosition = Vector3.zero;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

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

        if (moveDirection != Vector3.zero)
        {
            Vector3 horizontalDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection, Vector3.up);
            Vector3 targetEuler = targetRotation.eulerAngles;
            targetEuler.y = Mathf.Round(targetEuler.y / 90) * 90;
            targetRotation = Quaternion.Euler(0, targetEuler.y, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
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
        
        if (collision.collider.CompareTag("death"))
        {
            if (enableDeathClones)
            {
                Vector3 spawnPosition = collision.GetContact(0).point;
                CreateDeathClone(spawnPosition);
            }
            TeleportToRespawn();
        }
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

    void CreateDeathClone(Vector3 position)
    {
        GameObject clone = Instantiate(gameObject);
        clone.transform.position = position;
        Destroy(clone.GetComponent<PlayerMovement>());

        foreach (Collider col in clone.GetComponents<Collider>())
        {
            Destroy(col);
        }

        BoxCollider newCollider = clone.AddComponent<BoxCollider>();
        Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
        cloneRb.mass = rb.mass;
        cloneRb.linearDamping = rb.linearDamping;
        cloneRb.angularDamping = rb.angularDamping;

        foreach (GameObject deathObject in GameObject.FindGameObjectsWithTag("death"))
        {
            foreach (Collider deathCol in deathObject.GetComponents<Collider>())
            {
                Physics.IgnoreCollision(newCollider, deathCol);
            }
        }
    }

    void TeleportToRespawn()
    {
        rb.position = respawnPosition;
        rb.linearVelocity = Vector3.zero;
        velocity = Vector3.zero;
    }

    void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
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