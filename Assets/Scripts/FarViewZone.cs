using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Platformer.Mechanics;
using UnityEngine;

public class FarViewZone : MonoBehaviour
{
    [SerializeField] private float zoomValue;

    private CameraController vcam;

    private void Start()
    {
        vcam = FindObjectOfType<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 6) // 6 = player
        {
            vcam.zoomValue = zoomValue;
            vcam.StartZoom();
        }   
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        vcam.ResetZoom();
    }
}
