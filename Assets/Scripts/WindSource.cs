using UnityEngine;

public class WindSource : MonoBehaviour
{
    public enum WindShape { Cone, Square }

    [Header("General Settings")]
    public WindShape shape = WindShape.Cone;
    [Tooltip("Maximum distance the wind can reach")]
    public float strength = 10f;
    [Tooltip("Maximum force applied at wind source")]
    public float force = 10f;

    [Header("Cone Settings")]
    [Tooltip("Diameter of the cone at maximum distance")]
    public float coneDiameter = 5f;

    [Header("Square Settings")]
    [Tooltip("Side length of the square area")]
    public float squareSideLength = 5f;

    [Header("Force Falloff")]
    [Tooltip("Smoothing for force transition")]
    [Range(0.1f, 5f)] public float falloffSmoothness = 2f;

    private void FixedUpdate()
    {
        Vector3 sourcePos = transform.position;
        Vector3 direction = transform.forward;

        Collider[] colliders;

        if (shape == WindShape.Cone)
        {
            colliders = Physics.OverlapSphere(sourcePos, strength);
        }
        else
        {
            Vector3 boxCenter = sourcePos + direction * strength * 0.5f;
            Vector3 boxSize = new Vector3(squareSideLength, squareSideLength, strength);
            colliders = Physics.OverlapBox(boxCenter, boxSize * 0.5f, transform.rotation);
        }

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Wind") && col.attachedRigidbody != null)
            {
                Rigidbody rb = col.attachedRigidbody;
                Vector3 toObject = rb.position - sourcePos;

                if (shape == WindShape.Cone)
                {
                    HandleConeForce(sourcePos, direction, rb, toObject);
                }
                else
                {
                    HandleSquareForce(sourcePos, direction, rb, toObject);
                }
            }
        }
    }

    private void HandleConeForce(Vector3 sourcePos, Vector3 direction, Rigidbody rb, Vector3 toObject)
    {
        float depth = Vector3.Dot(toObject, direction);
        if (depth < 0 || depth > strength) return;

        Vector3 radialPart = toObject - depth * direction;
        float radialDistance = radialPart.magnitude;
        float maxRadialAtDepth = (coneDiameter / 2) * (depth / strength);

        if (radialDistance > maxRadialAtDepth) return;

        float depthFactor = 1 - Mathf.Pow(depth / strength, falloffSmoothness);
        float radialFactor = 1 - Mathf.Pow(radialDistance / maxRadialAtDepth, falloffSmoothness);
        float totalForce = force * depthFactor * radialFactor;

        rb.AddForce(direction * totalForce);
    }

    private void HandleSquareForce(Vector3 sourcePos, Vector3 direction, Rigidbody rb, Vector3 toObject)
    {
        Vector3 localPos = transform.InverseTransformPoint(rb.position);
        float depth = localPos.z;
        if (depth < 0 || depth > strength) return;

        float xOffset = Mathf.Abs(localPos.x);
        float yOffset = Mathf.Abs(localPos.y);
        float maxXY = Mathf.Max(xOffset, yOffset);
        float squareHalf = squareSideLength / 2;

        if (maxXY > squareHalf) return;

        float depthFactor = 1 - Mathf.Pow(depth / strength, falloffSmoothness);
        float radialFactor = 1 - Mathf.Pow(maxXY / squareHalf, falloffSmoothness);
        float totalForce = force * depthFactor * radialFactor;

        rb.AddForce(direction * totalForce);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 sourcePos = transform.position;
        Vector3 direction = transform.forward;

        if (shape == WindShape.Cone)
        {
            Vector3 endCenter = sourcePos + direction * strength;
            Gizmos.DrawLine(sourcePos, endCenter);
            Gizmos.DrawWireSphere(endCenter, coneDiameter / 2f);

            //draw radial lines
            Vector3 endRight = endCenter + transform.right * coneDiameter / 2f;
            Vector3 endLeft = endCenter - transform.right * coneDiameter / 2f;
            Vector3 endUp = endCenter + transform.up * coneDiameter / 2f;
            Vector3 endDown = endCenter - transform.up * coneDiameter / 2f;
            Gizmos.DrawLine(sourcePos, endRight);
            Gizmos.DrawLine(sourcePos, endLeft);
            Gizmos.DrawLine(sourcePos, endUp);
            Gizmos.DrawLine(sourcePos, endDown);
        }
        else
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(
                sourcePos + direction * strength * 0.5f,
                transform.rotation,
                Vector3.one
            );
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(squareSideLength, squareSideLength, strength));
            Gizmos.matrix = oldMatrix;
        }
    }
}