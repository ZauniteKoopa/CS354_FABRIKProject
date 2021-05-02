using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailAnimator : MonoBehaviour
{

    Vector3 dir = Vector3.zero;
    float transitionTime = 0.3f;
    float timer = 0.6f;

    [SerializeField]
    private Transform endEffector = null;
    [SerializeField]
    private Vector3 normalPose = Vector3.zero;
    [SerializeField]
    private float deltaX = 0.9f;
    [SerializeField]
    private float activeY = 1.0f;
    [SerializeField]
    private float deltaZ = 0.4f;


    //Vectors for interpolation
    Vector3 beginVector;
    Vector3 endVector;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (timer <= transitionTime)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionTime;
            endEffector.localPosition = Vector3.Lerp(beginVector, endVector, progress);
        }
        
    }

    //Method to change direction
    public void changeDirection(Vector3 direction)
    {
        direction *= -1f;

        if (dir != direction)
        {
            dir = direction;

            beginVector = endEffector.localPosition;

            if (direction.z >= 0.1)
            {
                endVector.z = normalPose.z + (deltaZ * direction.z);
            }
            else
            {
                endVector = normalPose + (deltaX * direction.x * Vector3.right) + (deltaZ * direction.z * Vector3.forward);

                if (direction != Vector3.zero)
                    endVector.y = activeY;
            }

            timer = 0f;


        }
    }
}
