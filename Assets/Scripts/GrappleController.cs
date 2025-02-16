using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RopeSwing : MonoBehaviour
{
    public Transform anchorPoint;
    private ConfigurableJoint joint;
    private LineRenderer line;
    private Vector3 ropeDirection;

    private void Start()
    {
        CreateLineRenderer();
        GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void CreateLineRenderer()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Unlit/Color")) { color = Color.white };
        line.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) CreateRope();
        if (Input.GetMouseButtonUp(0)) ReleaseRope();
        if (joint) UpdateRopeVisual();
    }

    private void CreateRope()
    {
        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedAnchor = anchorPoint.position;
        joint.autoConfigureConnectedAnchor = false;
        
        ropeDirection = (anchorPoint.position - transform.position).normalized;
        joint.axis = ropeDirection;
        
        // Lock rope length
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        
        // Allow rotation around anchor
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        line.enabled = true;
    }

    private void ReleaseRope()
    {
        Destroy(joint);
        line.enabled = false;
    }

    private void UpdateRopeVisual()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, anchorPoint.position);
    }
}