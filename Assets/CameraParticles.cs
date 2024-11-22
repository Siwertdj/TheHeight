using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraParticles : MonoBehaviour
{
    private Camera cam;
    private ParticleSystem leaves;

    private float originalOrthoSize;
    
    void Start()
    {
        cam = transform.parent.GetComponent<Camera>();
        leaves = GetComponent<ParticleSystem>();
        
        originalOrthoSize = cam.orthographicSize;
    }
    
    void Update()
    {
        var emission = leaves.emission;
        emission.rateOverTime = cam.orthographicSize / originalOrthoSize;
    }
}
