using UnityEngine;

public class InverseKinematics : MonoBehaviour
{
    [Header("IK Settings")]
    [Tooltip("Ordered list of joints from the base to the end effector.")]
    public Transform[] joints;
    [Tooltip("The target that the end effector will try to reach.")]
    public Transform target;
    [Tooltip("How close the end effector must get to the target (in world units).")]
    public float threshold = 0.1f;
    [Tooltip("Maximum iterations per frame.")]
    public int maxIterations = 10;
    [Tooltip("Speed factor for rotation adjustments.")]
    public float rotationSpeed = 1.0f;

    void Update()
    {
        SolveIK();
    }

    void SolveIK()
    {
        // Ensure at least one joint and a target is set
        if (joints == null || joints.Length == 0 || target == null)
            return;

        int iteration = 0;
        Transform endEffector = joints[joints.Length - 1];

        // Continue iterating until the end effector is close enough or it hits the max iteration count
        while (Vector3.Distance(endEffector.position, target.position) > threshold && iteration < maxIterations)
        {
            // Iterate backwards from the second-to-last joint down to the first
            for (int i = joints.Length - 2; i >= 0; i--)
            {
                Transform joint = joints[i];

                // Vector from joint to end effector and from joint to target
                Vector3 toEnd = endEffector.position - joint.position;
                Vector3 toTarget = target.position - joint.position;

                // Calculate the rotation needed to align these vectors
                // Using Quaternion.FromToRotation finds the shortest arc
                Quaternion rotationNeeded = Quaternion.FromToRotation(toEnd, toTarget);

                // limit the rotation speed by interpolating
                joint.rotation = Quaternion.Slerp(joint.rotation, rotationNeeded * joint.rotation, rotationSpeed * Time.deltaTime);
            }

            iteration++;
        }
    }
}
