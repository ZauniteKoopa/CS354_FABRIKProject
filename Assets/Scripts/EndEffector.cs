using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EndEffector : MonoBehaviour
{
    public UnityEvent onEndEffectorHit;
    public Transform endJoint;

    // Start is called before the first frame update
    void Awake()
    {
        onEndEffectorHit = new UnityEvent();
    }

    //On trigger, activate event
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Platform")
        {
            onEndEffectorHit.Invoke();
        }
    }

    public void refreshPosition()
    {
        transform.position = endJoint.position;
        transform.localPosition += 0.01f * Vector3.down;
    }
}
