using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAnimator : MonoBehaviour
{
    //Private varialbes (lowest ground and leg range are in local position)
    [Header("IK variables")]
    [SerializeField]
    private Transform leftLegEnd = null;
    [SerializeField]
    private Transform rightLegEnd = null;
    [SerializeField]
    private float lowestGround = 0.1f;
    [SerializeField]
    private float legRange = 0.6f;
    [SerializeField]
    private float legLift = 0.75f;
    [SerializeField]
    private float speed = 0.65f;

    [Header("Movement Variables")]
    [SerializeField]
    private bool automated = false;
    [SerializeField]
    private Vector3 startingVector = Vector3.zero;
    [SerializeField]
    private float endingX = 13.5f;


    //Tells you which leg is up
    private bool leftLegUp = true;
    private bool legLanding = true;
    float timer = 0.0f;

    //Left leg back / forward
    private float leftLegBaseY;
    private float defaultLeftX;
    private Vector3 leftLegTop;

    //Right leg vector
    private float rightLegBaseY;
    private float defaultRightX;
    private Vector3 rightLegTop;

    //Method on base ground
    private Vector3 originalGroundPos;
    private float curGroundDiff = 0f;
    private Vector3 lowerGroundPos = Vector3.zero;
    private bool hardFall = false;

    private Vector3 restingLegPos;

    //Variables to transition to idle
    private bool transitioningToIdle = false;
    private float transitionSpeed = 4f;

    //Additional bones
    [Header("Additional Bones")]
    [SerializeField]
    private Transform bustBone = null;
    [SerializeField]
    private float MAX_BUST_ROT = 15f;


    // Start is called before the first frame update
    void Start()
    {
        //Base Vector3s to calculate top
        Vector3 rightLegForward = new Vector3(defaultRightX, lowestGround, legRange);
        Vector3 rightLegBack = new Vector3(defaultRightX, lowestGround, -legRange);
        Vector3 leftLegForward = new Vector3(defaultLeftX, lowestGround, legRange);
        Vector3 leftLegBack = new Vector3(defaultLeftX, lowestGround, -legRange);

        //Left Leg
        defaultLeftX = leftLegEnd.localPosition.x;
        leftLegBaseY = lowestGround;
        leftLegTop = (leftLegForward + leftLegBack) / 2f;
        leftLegTop.y = legLift;

        leftLegEnd.GetComponent<EndEffector>().onEndEffectorHit.AddListener(onLeftFeetHitGround);

        //Right leg
        defaultRightX = rightLegEnd.localPosition.x;
        rightLegBaseY = lowestGround;
        rightLegTop = (rightLegForward + rightLegBack) / 2f;
        rightLegTop.y = legLift;
        restingLegPos = rightLegEnd.position;

        rightLegEnd.GetComponent<EndEffector>().onEndEffectorHit.AddListener(onRightFeetHitGround);

        //Calculate other variables
        originalGroundPos = transform.position;
        lowerGroundPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //Move model if automated, automatically move character
        if (automated)
        {
            float distance = Time.deltaTime * speed;
            transform.Translate(distance * Vector3.forward);

            if (transform.position.x >= endingX)
            {
                transform.position = startingVector;
                lowerGroundPos = startingVector;
                leftLegBaseY = lowestGround;
                rightLegBaseY = lowestGround;
            }

            updateIK(Vector3.forward, speed);
        }
    }


    //Method to update IK: 2 ways --> normal IK walk and falling
    public void updateIK(Vector3 dir, float speed)
    {
        timer += Time.deltaTime;
        float stepTime = (legRange * 2.0f) / speed;
        float progress = timer / stepTime;
        transitioningToIdle = false;

        if (progress <= 1.12f)
        {
            normal_IK_walk(progress, dir);
        }
        else
        {
            if (!hardFall)
                hardFall = true;

            //When falling, just move the entire body down until a leg hits
            //  The resting leg will maintain absolute position
            //  The active leg will maintain local position
            Vector3 restLegPos = (leftLegUp) ? rightLegEnd.position : leftLegEnd.position;
            float distFalled = speed * Time.deltaTime;
            transform.Translate(distFalled * Vector3.down);

            if (leftLegUp)
            {
                leftLegEnd.localPosition = new Vector3(defaultLeftX, lowestGround, 0f) + (legRange * dir);
                rightLegEnd.position = restLegPos;
            }
            else{
                rightLegEnd.localPosition = new Vector3(defaultRightX, lowestGround, 0f) + (legRange * dir);
                leftLegEnd.position = restLegPos;
            }
        }

        Vector3 bustEulers = new Vector3(dir.z, 0f, -dir.x);
        bustBone.localEulerAngles = MAX_BUST_ROT * bustEulers;

    }

    //Method to transition to idle: last for 0.1 seconds
    public IEnumerator transitionToIdle()
    {
        transitioningToIdle = true;
        timer = 0.0f;
        rightLegEnd.GetComponent<EndEffector>().refreshPosition();
        leftLegEnd.GetComponent<EndEffector>().refreshPosition();
        bool leftLegActive = leftLegUp;
        Vector3 restLegPos = (leftLegUp) ? rightLegEnd.position : leftLegEnd.position;

        //Check if grounded
        bool alreadyGrounded = false;
        int layerMask = 1 << 2;
        layerMask = ~layerMask;

        Vector3 activePos = (leftLegUp) ? leftLegEnd.position : rightLegEnd.position;
        alreadyGrounded = Physics.CheckSphere(activePos, 0.2f, layerMask);
        bustBone.localEulerAngles = Vector3.zero;


        while (transitioningToIdle && !alreadyGrounded)
        {
            float transitionDist = transitionSpeed * Time.deltaTime;

            //If active leg hasn't reached lowest ground yet, just move end effector. Else, move body without editing resting position
            if (leftLegActive) 
            {
                if (leftLegEnd.localPosition.y <= lowestGround)
                {
                    transform.Translate(transitionDist * Vector3.down);
                    rightLegEnd.position = restLegPos;
                }
                else
                {
                    leftLegEnd.position += transitionDist * Vector3.down;
                }

            }
            else
            {
                if (rightLegEnd.localPosition.y <= lowestGround)
                {
                    transform.Translate(transitionDist * Vector3.down);
                    leftLegEnd.position = restLegPos;
                }
                else
                {
                    rightLegEnd.position += transitionDist * Vector3.down;
                }
            }

            activePos = (leftLegUp) ? leftLegEnd.position : rightLegEnd.position;
            alreadyGrounded = Physics.CheckSphere(activePos, 0.2f, layerMask);

            yield return new WaitForEndOfFrame();
        }

        if (transitioningToIdle)
        {
            swapLegs();
        }
    }

    //IK method to be run if person is walking straight or upwards
    private void normal_IK_walk(float progress, Vector3 walkDir)
    {
        if (progress <= 0.5f)
            updateGround(progress);
        else
            legLanding = true;

        //Get four vectors
        walkDir.Normalize();

        Vector3 rightLegForward = (leftLegUp) ? restingLegPos : new Vector3(defaultRightX, lowestGround, 0f) + (legRange * walkDir);
        Vector3 rightLegBack = new Vector3(defaultRightX, rightLegBaseY, 0f) - (legRange * walkDir);
        Vector3 leftLegForward = (!leftLegUp) ? restingLegPos : new Vector3(defaultLeftX, lowestGround, 0f) + (legRange * walkDir);
        Vector3 leftLegBack = new Vector3(defaultLeftX, leftLegBaseY, 0f) - (legRange * walkDir);

        //Update end effectors
        if (leftLegUp)
        {
            //Update right leg (down)
            rightLegEnd.localPosition = Vector3.Lerp(rightLegForward, rightLegBack, progress);

            //Update left leg (up)
            Vector3 leftPos1 = (progress < 0.5f) ? leftLegBack : leftLegTop;
            Vector3 leftPos2 = (progress < 0.5f) ? leftLegTop : leftLegForward;
            progress = (progress < 0.5f) ? progress * 2.0f : (progress - 0.5f) * 2.0f;
            leftLegEnd.localPosition = Vector3.Slerp(leftPos1, leftPos2, progress);
        }
        else
        {
            //Update left leg (down)
            leftLegEnd.localPosition = Vector3.Lerp(leftLegForward, leftLegBack, progress);

            //Update right leg (up)
            Vector3 rightPos1 = (progress < 0.5f) ? rightLegBack : rightLegTop;
            Vector3 rightPos2 = (progress < 0.5f) ? rightLegTop : rightLegForward;
            progress = (progress < 0.5f) ? progress * 2.0f : (progress - 0.5f) * 2.0f;
            rightLegEnd.localPosition = Vector3.Slerp(rightPos1, rightPos2, progress);
        }
    }

    //Updater method when the left end effector hits the ground, change flag
    private void onLeftFeetHitGround()
    {
        if (leftLegUp && legLanding)
        {
            restingLegPos = leftLegEnd.localPosition;
            swapLegs();
        }
    }

    //Updater method when the right end effector hits the ground, change flag
    private void onRightFeetHitGround()
    {
        if (!leftLegUp && legLanding)
        {
            restingLegPos = rightLegEnd.localPosition;
            swapLegs();
        }
    }


    //Helper function to swap legs and update base variables
    private void swapLegs()
    {  

        //Set flags
        transitioningToIdle = false;
        leftLegUp = !leftLegUp;
        timer = 0.0f;
        legLanding = false;

        //Set base variables
        rightLegEnd.GetComponent<EndEffector>().refreshPosition();
        leftLegEnd.GetComponent<EndEffector>().refreshPosition();
        rightLegBaseY = rightLegEnd.localPosition.y;
        leftLegBaseY = leftLegEnd.localPosition.y;

        //Set up ground variables for potential interpolation only if body moves up!
        if (hardFall)
        {   
            curGroundDiff = 0.0f;
        }
        else
        {
            curGroundDiff = Mathf.Abs(leftLegEnd.position.y - rightLegEnd.position.y);
        }

        lowerGroundPos = transform.position;
        hardFall = false;
    }

    //Helper function to linearly interpolate moving up a step
    //  Progress must be between 0.0 and 0.5
    private void updateGround(float progress)
    {
        //Set up progress for linear interpolation: body will exert force on ground depending on which leg is on the ground
        progress *= 2.0f;
        Vector3 restLegPos = (leftLegUp) ? rightLegEnd.position : leftLegEnd.position;

        //Get vectors for linear interpolation
        Vector3 upperGroundPos = lowerGroundPos + (curGroundDiff * Vector3.up);
        float newHeight = Mathf.Lerp(lowerGroundPos.y, upperGroundPos.y, progress);
        transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);

        //Set resting leg's variables accordingly
        if (!leftLegUp)
        {
            leftLegEnd.position = restLegPos;
            leftLegBaseY = leftLegEnd.localPosition.y;
        }
        else
        {
            rightLegEnd.position = restLegPos;
            rightLegBaseY = rightLegEnd.localPosition.y;
        }
    }
}
