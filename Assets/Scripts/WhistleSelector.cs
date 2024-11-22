using System;
using System.Collections;
using System.Collections.Generic;
using static Platformer.Core.Simulation;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;

public class WhistleSelector : MonoBehaviour
{
    private PlayerController player;
    private AudioSource audioSource;
    
    private GameObject down;
    private GameObject left;
    private GameObject up;
    private GameObject right;

    private GameObject[] whistles = new GameObject[4];
    
    private int selection = -1;

    public AudioClip downFlute;
    public AudioClip leftFlute;
    public AudioClip upFlute;
    public AudioClip rightFlute;
    
    // to disable movement while whistling
    [SerializeField] private float downFreezeTime;
    [SerializeField] private float leftFreezeTime;
    [SerializeField] private float upFreezeTime;
    [SerializeField] private float rightFreezeTime;
    private float disableMovementTimer;
    
    [SerializeField] private CircleCollider2D plantRadius;
    [SerializeField] private CircleCollider2D updraftRadius;
    [SerializeField] private CircleCollider2D revealRadius;
    [SerializeField] private WaypointMover pathGuide;

    [SerializeField] private ParticleSystem musicNotes_PS;
    
    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        
        down = transform.GetChild(0).gameObject;
        left = transform.GetChild(1).gameObject;
        up = transform.GetChild(2).gameObject;
        right = transform.GetChild(3).gameObject;

        for (int i = 0; i < whistles.Length; i++)
            whistles[i] = transform.GetChild(i).gameObject;
        
        musicNotes_PS.Stop();
        // Start with menu closed
        CloseMenu();
    }

    private void Update()
    {
        if (disableMovementTimer > 0)
        {
            musicNotes_PS.Play();
            player.singing = true;
            player.controlEnabled = false;
        }
        else
        {
            musicNotes_PS.Stop();
            player.singing = false;
            player.controlEnabled = true;
        }

        disableMovementTimer -= Time.deltaTime;
    }

    public void OpenMenu()
    {
        for (int i = 0; i <whistles.Length; i++)
            whistles[i].SetActive(true);
    }

    private void CloseMenu()
    {
        // set all to be inactive.
        for (int i = 0; i <whistles.Length; i++)
            whistles[i].SetActive(false);
    }
    
    public void ChangeSelection(int newSelection)
    {
        // we apply -1 as an offset
        selection = newSelection - 1;

        for (int i = 0; i < whistles.Length; i++)
        {
            // TODO: Not very pretty at the moment, but very functional!
            if (i == selection)
            {
                // high alpha
                Color oldColor = whistles[i].GetComponent<SpriteRenderer>().color;
                Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, 1);
                whistles[i].GetComponent<SpriteRenderer>().color = newColor;
            }
            else
            {
                // low alpha
                Color oldColor = whistles[i].GetComponent<SpriteRenderer>().color;
                Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, 0.2f);
                whistles[i].GetComponent<SpriteRenderer>().color = newColor;
            }
        }
    }

    public void ConfirmSelection()
    {
        switch (selection)
        {
            case (-1):
                // invalid selection. Do nothing.
                break;
            case 0:     // DOWN  - Activate Updraft
                if (downFlute != null)
                    audioSource.PlayOneShot(downFlute);
                disableMovementTimer = downFreezeTime;
                // TODO: COlor musicnotes right
                updraftRadius.gameObject.SetActive(true);
                break;
            case 1:     // LEFT  - Reveal Areas
                if (leftFlute != null)
                    audioSource.PlayOneShot(leftFlute);
                disableMovementTimer = leftFreezeTime;
                //revealRadius.gameObject.SetActive(true);
                Invoke("RevealArea", leftFreezeTime);
                break;
            case 2:     // UP   -  Grow plants
                if (upFlute != null)
                    audioSource.PlayOneShot(upFlute);
                disableMovementTimer = upFreezeTime;
                //plantRadius.gameObject.SetActive(true);
                break;
            case 3:     // RIGHT  -  Spawn Guide
                if (rightFlute != null)
                    audioSource.PlayOneShot(rightFlute);
                disableMovementTimer = rightFreezeTime;
                //Create instance of a guide.
                Instantiate(pathGuide, transform.position, Quaternion.identity);
                break;
        }
        CloseMenu();
    }

    private void RevealArea()
    {
        revealRadius.gameObject.SetActive(true);
    }
}
