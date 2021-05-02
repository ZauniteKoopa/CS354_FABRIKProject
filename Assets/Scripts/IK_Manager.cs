using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

public class IK_Manager : MonoBehaviour
{
    //Bone classes that only indicate the first bone --> end bone
    [SerializeField]
    private Bone firstBone = null;
    [SerializeField]
    private Bone endBone = null;

    //Target end effector
    [SerializeField]
    private float THRESHOLD = 0.01f;
    [SerializeField]
    private int MAX_ITERATIONS = 10;
    [SerializeField]
    private Transform pole = null;
    [SerializeField]
    private Transform endEffector = null;

    List<Bone> bones = null;
    float totalLength = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        //Make sure IK manager is set
        Assert.IsTrue(firstBone != null && endBone != null);

        //Add all bones from first bone to last bone in the list
        bones = new List<Bone>();
        Bone curBone = firstBone;
        bool valid = false;

        while (curBone != null && curBone != endBone)
        {
            bones.Add(curBone);
            totalLength += curBone.getLength();

            curBone = curBone.getChildBone();
            if (curBone == endBone)
                valid = true;
        }

        //Assert that it's valid. If it still is, add the last bone
        Assert.IsTrue(valid);

        bones.Add(curBone);
        totalLength += curBone.getLength();
    }

    void Update()
    {
        executeIK(endEffector.position);
    }


    //Method to execute Inverse Kinematics
    public void executeIK(Vector3 tgtPos)
    {
        //Get important vectors and check the length, if its bigger than bone, just do a straight line. Else, do FABRIK
        Vector3 origin = firstBone.getOriginJointPos();
        List<Vector3> jointPositions = new List<Vector3>();

        if (Vector3.Distance(origin, tgtPos) > totalLength)
        {
            jointPositions.Add(firstBone.getOriginJointPos());
            Vector3 distDir = Vector3.Normalize(tgtPos - origin);

            for (int i = 0; i < bones.Count; i++)
            {
                float boneLength = bones[i].getLength();
                Vector3 curJoint = jointPositions[i] + (boneLength * distDir);
                jointPositions.Add(curJoint);
            }
        }
        else
        {
            //Get all joint positions
            foreach(Bone bone in bones)
            {
                jointPositions.Add(bone.getOriginJointPos());
            }

            jointPositions.Add(bones[bones.Count - 1].getDestJointPos());

            //Main loop
            bool converged = false;
            int curIteration = 0;

            while(!converged)
            {
                jointPositions = backwardKinematics(jointPositions, tgtPos);
                jointPositions = forwardKinematics(jointPositions, origin);

                float distToTgt = Vector3.Distance(jointPositions[jointPositions.Count - 1], tgtPos);
                curIteration++;

                converged = distToTgt <= THRESHOLD || curIteration >= MAX_ITERATIONS;
            }

            //Bend all joints between origin and end towards pole
            if (pole != null)
            {
                for (int i = 1; i < jointPositions.Count - 1; i++)
                {
                    //Preject pole and bone on a plane created by the 2 nearby joints
                    Plane plane = new Plane(jointPositions[i + 1] - jointPositions[i - 1], jointPositions[i - 1]);
                    Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
                    Vector3 projectedBone = plane.ClosestPointOnPlane(jointPositions[i]);

                    //Get the angle that will bend towards pole. Then apply it to position
                    float angle = Vector3.SignedAngle(projectedBone - jointPositions[i - 1], projectedPole - jointPositions[i - 1], plane.normal);
                    jointPositions[i] = Quaternion.AngleAxis(angle, plane.normal) * (jointPositions[i] - jointPositions[i - 1]) + jointPositions[i - 1];
                }
            }
        }

        //Update all bones tragectory and each bone's roll so they align with parent
        for (int i = 0; i < bones.Count; i++)
        {
            bones[i].updateBone(jointPositions[i + 1]);
        }
    }


    //Private helper method to get all the initial positions of the vectors
    //  Goes backwards, setting the last joint in the list to tgtPos
    private List<Vector3> backwardKinematics(List<Vector3> jointPositions, Vector3 tgt)
    {
        //Set up return list
        List<Vector3> newPositions = new List<Vector3>();
        foreach(Vector3 pos in jointPositions)
            newPositions.Add(pos);

        //Initially set the last joint position to tgt
        newPositions[newPositions.Count - 1] = tgt;
        int posIndex = newPositions.Count - 2;

        for (int i = bones.Count - 1; i >= 0; i--)
        {
            //Adjust joint length
            float boneLength = bones[i].getLength();
            Vector3 distDir = Vector3.Normalize(jointPositions[posIndex] - newPositions[posIndex + 1]);
            newPositions[posIndex] = newPositions[posIndex + 1] + (boneLength * distDir);

            //Update induction variables
            posIndex--;
        }

        return newPositions;
    }

    //Private helper method that updates joints to go forward
    //  Meant to keep model rooted in startPoint
    private List<Vector3> forwardKinematics(List<Vector3> jointPositions, Vector3 start)
    {
        //Set up return list
        List<Vector3> newPositions = new List<Vector3>();

        //Initially set up first joint to be set to start
        newPositions.Add(start);

        //Addjust all joint lengths according to this start
        for (int i = 0; i < bones.Count; i++)
        {
            float boneLength = bones[i].getLength();
            Vector3 distDir = Vector3.Normalize(jointPositions[i + 1] - newPositions[i]);
            Vector3 curJointPos = newPositions[i] + (boneLength * distDir);
            newPositions.Add(curJointPos);
        }

        return newPositions;
    }
}
