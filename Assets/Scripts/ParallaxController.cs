using System;
using System.Collections;
using System.Collections.Generic;
using Platformer.Mechanics;
using UnityEngine;


// Credit to: https://www.youtube.com/watch?v=ZYZfKbLxoHI
public class ParallaxController : MonoBehaviour
{
    private Transform cam; // Main camera
    private Vector3 camStartPos;
    private float distance;

    private GameObject[] backgrounds;
    private Material[] mat;
    private float[] backSpeed;

    private float furthestBack;

    [Range(0.01f, 0.05f)] 
    public float parallaxSpeed;
    
    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;
        
        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            // store all backgrounds and respective materials in two arrays
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }
        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        // Store furthest background position
        for (int i = 0; i < backCount; i++)
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > furthestBack)
            {
                furthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }
        
        // set the speed of backgrounds
        for (int i = 0; i < backCount; i++)
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / furthestBack;
        }
    }

    // Fixed stutters
    // Regular stutters
    // Late Update stutters
    private void LateUpdate()
    {
        distance = cam.position.x - camStartPos.x;
        transform.position = new Vector3(cam.position.x, transform.position.y, 0);
        
        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}
