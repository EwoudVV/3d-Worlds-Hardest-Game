using UnityEngine;
using System.Collections.Generic;
public class Cable : MonoBehaviour {
    public GameObject pointA;
    public GameObject pointB;
    public int segmentCount = 20;
    public float segmentLength = 0.2f;
    public float stretchSpring = 1000f;
    public float stretchDamper = 100f;
    List<GameObject> segments = new List<GameObject>();
    void Start(){
        Vector3 startPos = pointA.transform.position;
        Vector3 endPos = pointB.transform.position;
        Vector3 dir = (endPos - startPos).normalized;
        for(int i = 0; i < segmentCount; i++){
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            seg.transform.position = startPos + dir * segmentLength * (i + 0.5f);
            seg.transform.rotation = Quaternion.LookRotation(dir);
            seg.transform.localScale = new Vector3(0.1f, segmentLength * 0.5f, 0.1f);
            seg.GetComponent<Renderer>().material.color = Color.black;
            Rigidbody rb = seg.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            segments.Add(seg);
        }
        if(pointA.GetComponent<Rigidbody>() == null){
            Rigidbody rb = pointA.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        if(pointB.GetComponent<Rigidbody>() == null){
            Rigidbody rb = pointB.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        ConfigurableJoint jointA = segments[0].AddComponent<ConfigurableJoint>();
        jointA.connectedBody = pointA.GetComponent<Rigidbody>();
        jointA.autoConfigureConnectedAnchor = false;
        jointA.anchor = new Vector3(0, -segmentLength * 0.5f, 0);
        jointA.connectedAnchor = Vector3.zero;
        JointDrive driveA = new JointDrive();
        driveA.positionSpring = stretchSpring;
        driveA.positionDamper = stretchDamper;
        driveA.maximumForce = Mathf.Infinity;
        jointA.xDrive = driveA;
        jointA.yDrive = driveA;
        jointA.zDrive = driveA;
        jointA.xMotion = ConfigurableJointMotion.Limited;
        jointA.yMotion = ConfigurableJointMotion.Limited;
        jointA.zMotion = ConfigurableJointMotion.Limited;
        SoftJointLimit limitA = new SoftJointLimit();
        limitA.limit = segmentLength;
        jointA.linearLimit = limitA;
        for(int i = 0; i < segments.Count - 1; i++){
            ConfigurableJoint cj = segments[i + 1].AddComponent<ConfigurableJoint>();
            cj.connectedBody = segments[i].GetComponent<Rigidbody>();
            cj.autoConfigureConnectedAnchor = false;
            cj.anchor = new Vector3(0, -segmentLength * 0.5f, 0);
            cj.connectedAnchor = new Vector3(0, segmentLength * 0.5f, 0);
            JointDrive drive = new JointDrive();
            drive.positionSpring = stretchSpring;
            drive.positionDamper = stretchDamper;
            drive.maximumForce = Mathf.Infinity;
            cj.xDrive = drive;
            cj.yDrive = drive;
            cj.zDrive = drive;
            cj.xMotion = ConfigurableJointMotion.Limited;
            cj.yMotion = ConfigurableJointMotion.Limited;
            cj.zMotion = ConfigurableJointMotion.Limited;
            SoftJointLimit limit = new SoftJointLimit();
            limit.limit = segmentLength;
            cj.linearLimit = limit;
        }
        ConfigurableJoint jointB = pointB.AddComponent<ConfigurableJoint>();
        jointB.connectedBody = segments[segments.Count - 1].GetComponent<Rigidbody>();
        jointB.autoConfigureConnectedAnchor = false;
        jointB.anchor = Vector3.zero;
        jointB.connectedAnchor = new Vector3(0, segmentLength * 0.5f, 0);
        JointDrive driveB = new JointDrive();
        driveB.positionSpring = stretchSpring;
        driveB.positionDamper = stretchDamper;
        driveB.maximumForce = Mathf.Infinity;
        jointB.xDrive = driveB;
        jointB.yDrive = driveB;
        jointB.zDrive = driveB;
        jointB.xMotion = ConfigurableJointMotion.Limited;
        jointB.yMotion = ConfigurableJointMotion.Limited;
        jointB.zMotion = ConfigurableJointMotion.Limited;
        SoftJointLimit limitB = new SoftJointLimit();
        limitB.limit = segmentLength;
        jointB.linearLimit = limitB;
    }
}
