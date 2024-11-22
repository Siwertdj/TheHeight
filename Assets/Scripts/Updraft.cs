using System;
using System.Collections;
using System.Collections.Generic;
using Platformer.Mechanics;
using Unity.VisualScripting;
using UnityEngine;

public class Updraft : MonoBehaviour
{
    [SerializeField] private float updraftStrength;
    [SerializeField] private float strengthDecay;
    private float currentUpdraftStrength;
    private bool startedPush;

    private float originalGravity;

    [SerializeField] private float deactivateTime = 17f;
    float deactivateTimer;
    public bool active;
    private bool activating;
    private bool deactivating;

    [SerializeField] private ParticleSystem leavesPS;
    [SerializeField] private ParticleSystem windPS;
    [SerializeField] private float wind_inactiveEmission = 2;
    [SerializeField] private float wind_activeEmission = 10;
    [SerializeField] private float leaves_inactiveSpeed = 2;
    [SerializeField] private float leaves_activeSpeed = 5;
    [SerializeField] private float leaves_inactiveEmission = 2;
    [SerializeField] private float leaves_activeEmission = 25;
    [SerializeField] private float deactivateDelay = 1.0f;
    
    
    // TODO: CHeck if we implement this correctly
    private bool revertedPlayerChangeWhileInactive = true;
    //private AudioSource wind;

    
    private void Start()
    {
        startedPush = false;
        currentUpdraftStrength = updraftStrength;
        
        Deactivate();
        
        //wind = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (active)
        {
            if(deactivateTimer < 0 && !deactivating)
            {
                Debug.Log("deactivating..");
                Deactivate();
            }
            
            deactivateTimer -= Time.deltaTime;
        }
        
    }

    public void Activate()
    {
        if (!activating)
        {
            deactivateTimer = deactivateTime;
            activating = true;

            // Strengthen PS emission
            var leavesMain = leavesPS.main;
            leavesMain.startSpeed = leaves_activeSpeed;
            var leavesEmission = leavesPS.emission;
            leavesEmission.rateOverTimeMultiplier = leaves_activeEmission;

            var windEmission = windPS.emission;
            windEmission.rateOverTimeMultiplier = wind_activeEmission;

            Invoke("ActivateEffect", deactivateDelay);
        }
    }

    private void ActivateEffect()
    {
        // This exists because there is a small delay inbetween. May very per updraft.
        active = true;
        activating = false;
    }

    private void Deactivate()
    {
        if (!deactivating)
        {
            deactivating = true;

            // Weaken PS emission
            var leavesMain = leavesPS.main;
            leavesMain.startSpeed = leaves_inactiveSpeed;
            var leavesEmission = leavesPS.emission;
            leavesEmission.rateOverTimeMultiplier = leaves_inactiveEmission;

            var windEmission = windPS.emission;
            windEmission.rateOverTimeMultiplier = wind_inactiveEmission;

            Invoke("DeactivateEffect", deactivateDelay);
        }
    }

    private void DeactivateEffect()
    {
        // This exists because there is a small delay inbetween. May very per updraft.
        active = false;
        deactivating = false;
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == 6) // 6 = player
        {
            if (active)
            {
                // when the player enters the trigger while active..
                // we want its changes to be able to revert should it go inactive while the player is in there.
                revertedPlayerChangeWhileInactive = false;
                
                //wind.Play();
                KinematicObject player = other.GetComponent<KinematicObject>();

                if (!startedPush)
                {
                    originalGravity = player.gravityModifier;
                    startedPush = true;
                }
                else if (currentUpdraftStrength > 0)
                    currentUpdraftStrength -= strengthDecay * Time.deltaTime;
                else
                {
                    currentUpdraftStrength = 0;
                }

                // if the player lands while within the updraft, reset the updraft strength.
                if (player.IsGrounded)
                    currentUpdraftStrength = updraftStrength;

                // Allow the player to force-fall through an updraft
                if (!other.GetComponent<PlayerController>().forceActive)
                    player.gravityModifier = originalGravity - currentUpdraftStrength;
            }
            else if (!active && !revertedPlayerChangeWhileInactive)
            {
                revertedPlayerChangeWhileInactive = true;
                // revert everything, but only once.
                startedPush = false;
                currentUpdraftStrength = updraftStrength;
                //reset gravity
                KinematicObject player = other.GetComponent<KinematicObject>();
                player.gravityModifier = originalGravity;
                //wind.Stop();
            }
        }   
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (active)
        {
            startedPush = false;
            currentUpdraftStrength = updraftStrength;
            //reset gravity
            KinematicObject player = other.GetComponent<KinematicObject>();
            player.gravityModifier = originalGravity;
            //wind.Stop();
        }
    }
}
