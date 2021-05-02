using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

public class Bone : MonoBehaviour
{
    //Joints
    private Transform originJoint;
    [SerializeField]
    private Transform destJoint = null;

    //Method accessing bone length
    private float length;

    //Default tangent (tangent Direction if Rotation is all (0, 0, 0))
    //  IMPORTANT: ALL JOINTS MUST HAVE 0 ROTATION BEFORE RUNTIME
    private Vector3 zeroTangent = Vector3.up;
    private float defaultRoll = 0.0f;
    private Vector3 localOrigRot;
    Vector3 localNormal;


    // Start is called before the first frame update
    void Awake()
    {
        originJoint = transform;
        length = Vector3.Distance(originJoint.position, destJoint.position);
        zeroTangent = Vector3.Normalize(destJoint.position - originJoint.position);
        defaultRoll = originJoint.localEulerAngles.y;

        //Obtain the default roll
        // Quaternion zeroRoll = Quaternion.AngleAxis(0f, destJoint.localPosition);
        Vector3 normal = Vector3.Cross(zeroTangent, Vector3.up).normalized;
        localNormal = transform.InverseTransformVector(normal);
        localOrigRot = transform.InverseTransformVector(transform.rotation * normal);
        // Vector3 tester = transform.localRotation * perpendicular;
        // print(name + ": " + Vector3.SignedAngle(perpendicular, tester, destJoint.localPosition));
    }

    
    //Method to update bone with new dest joint position
    public void updateBone(Vector3 newDestJointPos)
    {   
        //Test if new dest joint is valid
        float testDist = Vector3.Distance(originJoint.position, newDestJointPos);

        //Assert.IsTrue(testDist <= length + 0.1f && testDist >= length - 0.1f);

        //Rotate towards that direction
        Vector3 tgtDir = Vector3.Normalize(newDestJointPos - originJoint.position);
        originJoint.eulerAngles = Vector3.zero;
        zeroTangent = Vector3.Normalize(destJoint.position - originJoint.position);
        originJoint.rotation *= Quaternion.FromToRotation(zeroTangent, tgtDir);
        //testDist = Vector3.Distance(newDestJointPos, destJoint.position);


        //Maintain roll of the parent 
        // float parentRoll = originJoint.parent.eulerAngles.y;       //Assumes that hip bone is straight up
        // Quaternion rollQuat = Quaternion.AngleAxis(parentRoll, Vector3.up);
        // originJoint.localRotation *= (rollQuat * Quaternion.AngleAxis(defaultRoll, Vector3.up));

        Vector3 testNormal = transform.rotation * transform.TransformVector(localNormal);
        testNormal = transform.InverseTransformVector(testNormal);
        float roll = Vector3.SignedAngle(testNormal, localOrigRot, zeroTangent);
        originJoint.rotation *= Quaternion.AngleAxis(roll, zeroTangent);
        //print(name + ": " + Vector3.SignedAngle(testNormal, origNormal, zeroTangent));
    }


    //Method to access dest joint position
    public Vector3 getDestJointPos()
    {
        return destJoint.position;
    }

    //Method to access originJoint position
    public Vector3 getOriginJointPos()
    {
        return originJoint.position;
    }

    //Method to access the length of this bone
    public float getLength()
    {
        return length;
    }

    //Method to get child bone
    //  If no child bone, return null
    //  ASSUMES THAT EACH BONE WILL ONLY HAVE ONE CHILD (only used for IK)
    public Bone getChildBone()
    {
        return destJoint.GetComponent<Bone>();
    }
}
