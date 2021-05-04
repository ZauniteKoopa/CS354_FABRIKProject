using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField]
    private Material activeMat = null;
    [SerializeField]
    private Material hitMat = null;
    private bool active;

    // Start is called before the first frame update
    void Start()
    {
        active = true;
        GetComponent<MeshRenderer>().material = activeMat;
    }

    //Method to hit the target: returns if it was a successful hit (tgt was active)
    public bool hitTarget()
    {
        if (active)
        {
            active = false;
            GetComponent<MeshRenderer>().material = hitMat;
            return true;
        }

        return false;
    }
}
