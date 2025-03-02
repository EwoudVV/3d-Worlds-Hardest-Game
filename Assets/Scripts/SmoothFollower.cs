using UnityEngine;

public class SmoothFollower : MonoBehaviour
{
    public Transform target;
    public float positionSmoothTime = 0.5f;
    public float rotationSmoothTime = 0.7f;

    private Vector3 positionVelocity;
    private Quaternion rotationVelocity;

    void Update()
    {
        if (target == null) return;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.position,
            ref positionVelocity,
            positionSmoothTime
        );

        transform.rotation = SmoothDampQuaternion(
            transform.rotation,
            target.rotation,
            ref rotationVelocity,
            rotationSmoothTime
        );
    }

    private Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Quaternion velocity, float smoothTime)
    {
        if (Time.deltaTime < Mathf.Epsilon) return current;
        
        float dot = Quaternion.Dot(current, target);
        float sign = dot > 0f ? 1f : -1f;
        
        target.x *= sign;
        target.y *= sign;
        target.z *= sign;
        target.w *= sign;

        Vector4 smoothResult = new Vector4(
            Mathf.SmoothDamp(current.x, target.x, ref velocity.x, smoothTime),
            Mathf.SmoothDamp(current.y, target.y, ref velocity.y, smoothTime),
            Mathf.SmoothDamp(current.z, target.z, ref velocity.z, smoothTime),
            Mathf.SmoothDamp(current.w, target.w, ref velocity.w, smoothTime)
        ).normalized;

        Vector4 error = Vector4.Project(new Vector4(velocity.x, velocity.y, velocity.z, velocity.w), smoothResult);
        velocity.x -= error.x;
        velocity.y -= error.y;
        velocity.z -= error.z;
        velocity.w -= error.w;

        return new Quaternion(smoothResult.x, smoothResult.y, smoothResult.z, smoothResult.w);
    }
}