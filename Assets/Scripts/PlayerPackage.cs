using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPackage : MonoBehaviour
{
    [SerializeField]
    private float speed = 1.5f;
    [SerializeField]
    private float rotateSpeed = 5f;
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
    [SerializeField]
    private float laserDistance = 50f;
    [SerializeField]
    private Text pointText = null;
    private const float CAMERA_SCREEN_DIST = 150f;

    private const float LOOK_DISTANCE = 30f;

    private int numPoints = 0;


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
        Vector3 dir = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x <= CAMERA_SCREEN_DIST)
        {
            dir += Vector3.left;
        }
        else if (mousePos.x >= Screen.width - CAMERA_SCREEN_DIST)
        {
            dir += Vector3.right;
        }

        // //Edit the rotation
        if (dir != Vector3.zero)
        {
            Vector3 newEulers = transform.eulerAngles;
            newEulers.y += (rotateSpeed * Time.deltaTime * dir.x);
            transform.eulerAngles = newEulers;
        }
    }

    //Overall coroutine to fire laser
    private IEnumerator renderLaser()
    {
        //Set up line
        Vector3 gunDir = lookTgt.position - gun.position;
        gunDir.Normalize();
        gunDir *= laserDistance;
        Vector3 finalPos = lookTgt.position + gunDir;

        //Check if object hits a target
        gunDir.Normalize();
        RaycastHit hit;
        Ray laserRay = new Ray(gun.position, gunDir);

        //If hit a target, deactivate that target if possible and get a point
        if (Physics.Raycast(laserRay, out hit, laserDistance))
        {
            finalPos = hit.point;
            Target shotTgt = hit.transform.GetComponent<Target>();

            if (shotTgt != null)
            {
                bool gainPoint = shotTgt.hitTarget();

                if (gainPoint)
                {
                    numPoints++;
                    pointText.text = "Targets Hit:  " + numPoints;
                }
            }
        }

        //Render the line and play the audio
        laserRender.SetPosition(0, gun.position);
        laserRender.SetPosition(1, finalPos);
        laserRender.enabled = true;
        laserAudio.Play();

        yield return new WaitForSeconds(0.2f);

        laserRender.enabled = false;
    }
}
