using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPackage : MonoBehaviour
{
    [SerializeField]
    private float speed = 1.5f;
    [SerializeField]
    private WalkAnimator walkAnim = null;
    private bool moving = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Get overall direction
        float hor = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        Vector3 dir = Vector3.zero;

        if (hor > 0.01 || hor < -0.01)
        {
            dir += hor * Vector3.right;
        }
        
        if (vert > 0.01 || vert < -0.01)
        {
            dir += vert * Vector3.forward;
        }

        //If moving, update movement
        if (dir != Vector3.zero)
        {
            moving = true;
            dir.Normalize();
            walkAnim.updateIK(dir, speed);
            transform.Translate(speed * Time.deltaTime * dir);
        }
        else if (moving)
        {
            moving = false;
            StartCoroutine(walkAnim.transitionToIdle());
        }
    }
}
