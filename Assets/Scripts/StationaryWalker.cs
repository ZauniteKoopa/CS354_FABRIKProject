using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryWalker : MonoBehaviour
{
    [SerializeField]
    private WalkAnimator walker = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        walker.updateIK(Vector3.forward, 2f);
    }
}
