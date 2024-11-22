using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.Serialization;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip flyAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        [SerializeField] ParticleSystem dust;
        [SerializeField] ParticleSystem feather;
        
        /// Max horizontal speed of the player.
        public float maxSpeed = 2;
        /// Initial jump velocity at the start of a jump.
        public float jumpTakeOffSpeed = 5.5f;

        private bool _slowingDown;
        private bool _buildingUp;
        [SerializeField] private float buildUpModifier;
        [SerializeField] private float slowDownModifier;
        [SerializeField] private float buildUpStartValue;
        private float _buildUpSpeed = 0;
        private float _slowDownSpeed = 0;

        private bool _coyoteTimeStarted;    // Did we we start counting? yes/no
        private bool _coyoteTimeActive;     // Do we still have the opportunity to jump? yes/no
        [SerializeField] float coyoteInterval;  // How long is the marginf or coyotetime?
        private float _coyoteTime;              // internal counter
        [SerializeField] private float rememberJumpAttemptTime;
        private float _lastJumpAttempt;

        [SerializeField] private float flappingInterval;
        private float _flappingTimer;


        [SerializeField] private float normalGravityModifier = 0.1f;
        [SerializeField] private float forceGravityModifier;
        public bool forceDescent;   // We are currently forcing our character down.
        public bool forceActive;    // We only apply our forced gravity once, remembered by this bool.
        
        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        public bool singing = false;
        
        public bool flyingEnabled = false;
        
        bool jump;
        Vector2 move;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] private Animator squishAnimator;
        [SerializeField] internal Animator spriteAnimator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        private bool menuOpen = false;
        private int recentSelection = 0;
        private bool holdingSelection = false;
        [SerializeField] float releaseMargin;
        private float releaseTimer;
        [SerializeField] private WhistleSelector whistleMenu;
        
        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                // Horizontal movement
                move.x = Input.GetAxis("Horizontal");
                
                // Jumping
                // Regardless of the input, check if there was an attempt to jump within a given timeframe.
                // If yes, then jump.
                // If the button is not held, its the largest possible jump. If it is held, the player can influence the length.
                if (_lastJumpAttempt <= rememberJumpAttemptTime)
                {
                    if (jumpState == JumpState.Grounded)
                        jumpState = JumpState.PrepareToJump;
                }
                
                if (Input.GetButtonDown("Jump"))
                {
                    // if on ground, or coyotetime is active (between excl. 0 and the incl. upper limit)
                    //if (jumpState == JumpState.Grounded)
                    if (IsGrounded || _coyoteTimeActive)
                    {
                        jumpState = JumpState.PrepareToJump;
                    }
                    else // if the player is attempting to jump but is currently not in the grounded state
                    {
                        // Remember jump input for a very short time.
                        _lastJumpAttempt = 0;
                    }
                    // Reverse the effects of forcing down when jumping
                    StopForcedDescent();
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
                
                // Force descent
                // when s or down is pressed, and we are descending
                if (Input.GetKeyDown(KeyCode.S) && Math.Abs(velocity.y) > 0.05f)
                {
                    stopJump = true;
                    forceDescent = true;
                    Schedule<PlayerStopJump>().player = this;
                    CreateFeathers();
                    CreateFeathers();
                    CreateFeathers();
                }
                
                // Whistle Selector
                WhistleSelect();
            }
            else // restrict movement when controls disabled
            {
                move.x = 0;
            }

            // if descending..
            if (velocity.y < 0)
            {
                // COYOTE TIME
                // coyoteTime starts, if not already active
                if (!_coyoteTimeStarted)            // after landing, this becomes true and remains as such, until we start falling.
                {
                    _coyoteTimeStarted = true;      // we define the start of coyotetime
                    _coyoteTimeActive = true;       // it is currently active, until the timer runs out
                    _coyoteTime = coyoteInterval;   // timer is reset
                }
                else // Coyotetime has started counting
                {
                    _coyoteTime -= Time.deltaTime;  // decrease timer by time
                    if (_coyoteTime < 0)
                        _coyoteTimeActive = false;  // If time runs out, coyotetime becomes inactive.
                }
                
                // create sound and make feathers every 'wingflap'
                if (_flappingTimer >= flappingInterval)
                {
                    if (!forceDescent)
                    {
                        audioSource.PlayOneShot(flyAudio);
                        _flappingTimer = 0;
                        CreateFeathers();
                    }
                }
            }
            
            
        
            
            _flappingTimer += Time.deltaTime;
            _lastJumpAttempt += Time.deltaTime;
            
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        // Create featherParticle effect on the player
                        CreateFeathers();
                        CreateFeathers();
                        // This queues a jumping-sound to play..
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    // As soon as the player lands after being 'InFlight', change state and queue sound
                    if (IsGrounded)
                    {
                        //stop flapping sound
                        audioSource.Stop();
                        // This queues a landing-sound to play
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    // Not sure why this is a separate state
                    CreateDust();
                    if (forceDescent)
                    {
                        CreateDust();
                        CreateDust();
                    }
                    jumpState = JumpState.Grounded;
                    _coyoteTimeStarted = false;
                    StopForcedDescent();
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            // If the player is performing a jump while grounded, move them up.
            if (jump && (IsGrounded || _coyoteTimeActive))
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
                _coyoteTimeActive = false;
            }
            // If the player let go off the spacebar, they should now start falling.
            else if (stopJump && !jump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y *= model.jumpDeceleration;
                }
            }
            
            if (forceDescent && !forceActive)
            {
                forceActive = true;
                gravityModifier = forceGravityModifier;
            }

            // Rotate sprite based on their movement
            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            squishAnimator.SetBool("grounded", IsGrounded);
            squishAnimator.SetFloat("velocityY", Mathf.Abs(velocity.y) / maxSpeed);
            spriteAnimator.SetBool("grounded", IsGrounded);
            spriteAnimator.SetBool("falling", forceDescent);
            spriteAnimator.SetBool("singing", singing);
            spriteAnimator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            if (Mathf.Abs(move.x) < 0.01)   // if not inputting movement
            {
                if (_buildUpSpeed > 0.9)     // if nearly built up completely
                    _slowingDown = true;
                else                        // else, should have been standing still before, so build up.
                    _buildingUp = true;
                
                if (_slowingDown)       // Keep moving until completely slown down.
                {
                    if (_slowDownSpeed > 0)
                    {
                        _slowDownSpeed -= slowDownModifier * Time.deltaTime;
                    }
                    else                // stop sliding when slowdownspeed reaches zero. 
                        _slowingDown = false;

                    // This creates a gradual slow-down.
                    _buildUpSpeed = buildUpStartValue;
                    targetVelocity = move * (maxSpeed * _slowDownSpeed);
                    //Debug.Log("Slow-down speed: " + _slowDownSpeed);
                }
            }
            else // if inputting movement
            {
                if (_buildingUp)
                {
                    if (_buildUpSpeed < 1)
                    {
                        _buildUpSpeed += buildUpModifier * Time.deltaTime;
                    }
                    else
                        _buildingUp = false;

                    _slowDownSpeed = 1;
                    targetVelocity = move * (maxSpeed * _buildUpSpeed);
                    //Debug.Log("Build-up speed: " + _buildUpSpeed);
                }
                else
                {
                    // should be on max speed here.
                    _slowDownSpeed = 0;
                    _buildUpSpeed = buildUpStartValue;
                    targetVelocity = move * maxSpeed;
                }
            }
            
            // if buildingUp, buildup speed.Otherwise, slowdown-speed
            spriteAnimator.SetFloat("walkSpeed", _buildingUp ? _buildUpSpeed : (_slowingDown ? _slowDownSpeed : 1));
            
        }

        private void WhistleSelect()
        {
            // Get key states
            bool downDown = Input.GetKeyDown("down");
            bool leftDown = Input.GetKeyDown("left");
            bool upDown = Input.GetKeyDown("up");
            bool rightDown = Input.GetKeyDown("right");
            bool down = Input.GetKey("down");
            bool left = Input.GetKey("left");
            bool up = Input.GetKey("up");
            bool right = Input.GetKey("right");
            bool anyReleases = Input.GetKeyUp("down") || Input.GetKeyUp("left") || Input.GetKeyUp("up") || Input.GetKeyUp("right"); 
            
            // if any are currently held, menu should be open. Selection is switched to the most recent 'Down'.
            if (down || left || up || right)
            {
                // Some take priority over others if pressed simultaneously
                if (downDown)
                    recentSelection = 1;
                if (leftDown)
                    recentSelection = 2;
                if (upDown)
                    recentSelection = 3;
                if (rightDown)
                    recentSelection = 4;

                // we give a new input, so we forget whatever we were holding
                holdingSelection = false;
                
                // change selection of menu to emphasize the chosen 'recentDown'
                // > IF NOT OPEN, OPEN MENU
                if (!menuOpen)
                {
                    menuOpen = true;
                    whistleMenu.OpenMenu();
                }
                // > CHANGE SELECTOR TO RECENTDOWN
                whistleMenu.ChangeSelection(recentSelection);
            }
            // if none are currently held, and there is a button release..
            else if (anyReleases)
            {
                // remember the selection. Wait for a short time - if no other presses, confirm. Otherwise, confirm.
                releaseTimer = 0;
                holdingSelection = true;
            }
            // else, no changes to the menu keys, so dont bother doing anything.

            // if we were holding back a selection, and the timer passes the given margin, confirm selection.
            if (releaseTimer > releaseMargin && holdingSelection)
            {
                // reset variables
                recentSelection = 0;
                menuOpen = false;
                holdingSelection = false;
                // call to confirm selection
                whistleMenu.ConfirmSelection();
            }
            
            // count, always
            releaseTimer += Time.deltaTime;
        }
        
        private void CreateDust()
        {
            dust.Play();
        }
        
        private void CreateFeathers()
        {
            feather.Play();
        }

        private void StopForcedDescent()
        {
            forceDescent = false;
            forceActive = false;
            gravityModifier = normalGravityModifier;
        }
        
        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}