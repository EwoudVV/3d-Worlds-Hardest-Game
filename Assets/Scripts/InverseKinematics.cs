using UnityEngine;

[System.Serializable]
public class IKJoint
{
    public Transform jointTransform;
    public bool rotateX;
    public float minX = -180f;
    public float maxX = 180f;
    public bool rotateY;
    public float minY = -180f;
    public float maxY = 180f;
    public bool rotateZ;
    public float minZ = -180f;
    public float maxZ = 180f;
    public float weight = 0.3f;
}

public class InverseKinematics : MonoBehaviour
{
    public IKJoint[] joints;
    public Transform target;
    public int iterations = 40;
    public float tolerance = 0.2f;

    void Update()
    {
        SolveIK();
    }

    void SolveIK()
    {
        if (joints.Length == 0 || target == null) return;
        
        Transform endEffector = joints[joints.Length - 1].jointTransform;

        for (int i = 0; i < iterations; i++)
        {
            if (Vector3.Distance(endEffector.position, target.position) <= tolerance) break;

            for (int j = joints.Length - 1; j >= 0; j--)
            {
                IKJoint current = joints[j];
                Transform joint = current.jointTransform;
                Vector3 toEnd = endEffector.position - joint.position;
                Vector3 toTarget = target.position - joint.position;

                if (current.rotateX) ProcessAxis(joint, Vector3.right, toEnd, toTarget, current.minX, current.maxX, current.weight);
                if (current.rotateY) ProcessAxis(joint, Vector3.up, toEnd, toTarget, current.minY, current.maxY, current.weight);
                if (current.rotateZ) ProcessAxis(joint, Vector3.forward, toEnd, toTarget, current.minZ, current.maxZ, current.weight);
            }
        }
    }

    void ProcessAxis(Transform joint, Vector3 axis, Vector3 toEnd, Vector3 toTarget, float min, float max, float weight)
    {
        Vector3 localToEnd = joint.InverseTransformDirection(toEnd);
        Vector3 localToTarget = joint.InverseTransformDirection(toTarget);

        Vector3 projectedEnd = Vector3.ProjectOnPlane(localToEnd, axis);
        Vector3 projectedTarget = Vector3.ProjectOnPlane(localToTarget, axis);

        if (projectedEnd.magnitude < 0.001f || projectedTarget.magnitude < 0.001f) return;

        float angle = Vector3.SignedAngle(projectedEnd, projectedTarget, axis);
        float clampedAngle = Mathf.Clamp(angle * weight * Time.deltaTime, min, max);
        
        joint.Rotate(axis, clampedAngle, Space.Self);
    }
}