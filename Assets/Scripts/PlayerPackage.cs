using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPackage : MonoBehaviour
{
    [SerializeField]
    private float speed = 1.5f;
    // [SerializeField]
    // private float rotateSpeed = 2f;
    [SerializeField]
    private WalkAnimator walkAnim = null;
    private bool moving = false;
    [SerializeField]
    private Transform lookTgt = null;
    [SerializeField]
    private Transform head = null;
    [SerializeField]
    private Transform gun = null;
    [SerializeField]
    private LineRenderer laserRender = null;
    [SerializeField]
    private AudioSource laserAudio = null;

    private const float LOOK_DISTANCE = 10f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cameraMovement();
        movement();
        updateLookTarget();

        //If left click, shoot from gun
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(renderLaser());
        }

    }

    //Method for general movement
    private void movement()
    {
        //Get overall movementdirection
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

    //Method to update look target
    private void updateLookTarget()
    {
        //Get a ray at mouse position and then get a point
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = screenRay.GetPoint(LOOK_DISTANCE);

        //Update look target
        lookTgt.position = rayPoint;

        //Continously look at tgt
        head.LookAt(lookTgt);
    }

    //Method for general camera movement and rotation
    private void cameraMovement()
    {
        //Get movement direction
        float camHor = Input.GetAxis("CamHorizontal");
        Vector3 dir = Vector3.zero;

        if (camHor > 0.01 || camHor < -0.01)
        {
            dir += camHor * Vector3.right;
        }

        //If moving the camera, move the camera
        // if (dir != Vector3.zero)
        // {
        //     dir.Normalize();
        //     Transform camTransform = Camera.main.transform;
        //     camTransform.Translate(rotateSpeed * Time.deltaTime * dir);
        //     Vector3 camForward = transform.position - camTransform.position;
        //     camForward.y = camTransform.forward.y;

        //     camTransform.forward = camForward;
        // }

        //Edit the rotation
        if (dir != Vector3.zero)
        {
            Vector3 newEulers = transform.eulerAngles;
            newEulers.y += (camHor);
            transform.eulerAngles = newEulers;
        }
    }

    //Overall coroutine to fire laser
    private IEnumerator renderLaser()
    {
        //Set up line
        Vector3 gunDir = lookTgt.position - gun.position;
        gunDir.Normalize();
        gunDir *= 50f;
        Vector3 finalPos = lookTgt.position + gunDir;

        laserRender.SetPosition(0, gun.position);
        laserRender.SetPosition(1, finalPos);
        laserRender.enabled = true;
        laserAudio.Play();

        yield return new WaitForSeconds(0.2f);

        laserRender.enabled = false;
    }
}
