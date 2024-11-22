using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Platformer.Mechanics;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // This script will alter the virtual CM camera to..
    // - decrease dampening when falling
    // - zoom out when the player enters a specific area
    // - ..

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer vcamBody;
    private PlayerController player;        //.. to read player's vertical movement

    private float originalOrthoSize;
    private float targetOrthoSize;

    public float zoomValue = 1;
    [SerializeField] float zoomSpeed = 1;
    
    // Start is called before the first frame
    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        vcamBody = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        
        player = FindObjectOfType<PlayerController>();

        originalOrthoSize = vcam.m_Lens.OrthographicSize;
        targetOrthoSize = originalOrthoSize;
    }

    private void Update()
    {
        // follow camera down for landing
        if (player.velocity.y < -0.3f)
        {
            vcamBody.m_YDamping = 1f;
            vcamBody.m_ScreenY = 0.45f;
        } //follow up for flying
        else
        {
            vcamBody.m_YDamping = 3f;
            vcamBody.m_ScreenY = 0.6f;
        }
        
        // change lens-size to zoom-in/-out
        vcam.m_Lens.OrthographicSize =
            Mathf.Lerp(vcam.m_Lens.OrthographicSize, targetOrthoSize, zoomSpeed * Time.deltaTime);
    
    }

    public void StartZoom()
    {
        targetOrthoSize = vcam.m_Lens.OrthographicSize * zoomValue;
    }

    public void ResetZoom()
    {
        zoomValue = 1;
        targetOrthoSize = originalOrthoSize;
    }

    public void CompleteResetZoom()
    {
        ResetZoom();
        vcam.m_Lens.OrthographicSize = originalOrthoSize;
    }
}
