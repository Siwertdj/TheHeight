using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenPath : MonoBehaviour
{
    private AudioSource audioSource;
    
    [SerializeField] private float revealSpeed = 0.2f;
    private bool revealing;
    private bool revealSoundPlayed = false;
    
    private Tilemap tilemap;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        tilemap = GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (revealing)
        {
            Color oldColor = tilemap.color;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a - revealSpeed * Time.deltaTime);
            tilemap.color = newColor;
            
            // if it become invisible, AND we're no longer playing the sound, set the tilemap to inactive.
            if (tilemap.color.a <= 0 && !audioSource.isPlaying)
            {
                gameObject.SetActive(false);
            }
            else if (tilemap.color.a <= 0.2 && !revealSoundPlayed)
            {
                revealSoundPlayed = true;
                audioSource.PlayOneShot(audioSource.clip);
            }
        }
        
    }

    public void Reveal()
    {
        revealing = true;
    }
}
