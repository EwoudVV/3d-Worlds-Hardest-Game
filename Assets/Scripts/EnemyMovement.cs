using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EnemyMovement : MonoBehaviour
{
    public enum MovementType { Linear, Rotational }
    public MovementType movementType;
    public enum DirectionLock { None, X, Y, Z }
    public DirectionLock lockAxis;
    public List<Transform> waypoints;
    public float moveSpeed = 5f;
    private int currentWaypointIndex = 0;
    private bool reverse = false;
    public Transform rotationCenter;
    public float rotationRadius = 3f;
    public float rotationSpeed = 5f;
    private float angle;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.isKinematic = true;
        }
        if (movementType == MovementType.Linear && waypoints.Count > 0)
        {
            transform.position = waypoints[0].position;
        }
    }
    void FixedUpdate()
    {
        if (movementType == MovementType.Linear)
        {
            MoveLinear();
        }
        else if (movementType == MovementType.Rotational)
        {
            RotateAround();
        }
    }
    void MoveLinear()
    {
        if (waypoints.Count < 2) return;
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 moveDirection = (targetWaypoint.position - transform.position).normalized;
        if (lockAxis == DirectionLock.X) moveDirection.x = 0;
        if (lockAxis == DirectionLock.Y) moveDirection.y = 0;
        if (lockAxis == DirectionLock.Z) moveDirection.z = 0;
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.fixedDeltaTime);
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.01f)
        {
            if (!reverse)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    currentWaypointIndex = waypoints.Count - 2;
                    reverse = true;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    reverse = false;
                }
            }
        }
    }
    void RotateAround()
    {
        if (rotationCenter == null) return;
        angle += rotationSpeed * Time.fixedDeltaTime;
        float x = rotationCenter.position.x + Mathf.Cos(angle) * rotationRadius;
        float z = rotationCenter.position.z + Mathf.Sin(angle) * rotationRadius;
        transform.position = new Vector3(x, transform.position.y, z);
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("bablockno") || collision.gameObject.CompareTag("death"))
        {
            collision.gameObject.GetComponent<PlayerMovement>()?.TeleportToRespawn();
        }
    }
}
