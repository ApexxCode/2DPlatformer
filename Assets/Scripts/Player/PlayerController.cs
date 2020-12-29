using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Physics")]
    [SerializeField, Tooltip("How much time is allowed when the player is over a ledge to allow a jump.")] private float hangTime = 0.2f;
    [SerializeField, Tooltip("How much time is allowed to enable jump, before hitting the ground.")] private float jumpBufferTime = 0.1f;
    [SerializeField, Tooltip("How much force to apply to the Player's vertical position when jumping.")] private float jumpVelocity = 12f;
    [SerializeField, Tooltip("How much force to apply the the Player's horizontal position.")] private float moveVelocity = 4f;
    [SerializeField, Tooltip("Select or Create a Layer designated for the ground.")] private LayerMask groundLayer;

    [Header("Player Effects")]
    [SerializeField] private ParticleSystem footstepParticles;
    [SerializeField] private float particlesOverDistance;

    private Animator theAnimator;
    private BoxCollider2D theBoxCollider;
    private ParticleSystem.EmissionModule footEmission;
    private PlayRandomSounds playRandomSounds;
    private Rigidbody2D theRB;
    private float hangCounter;
    private float inputDirection;
    private float jumpBufferCount;

    private void Awake()
    {
        footstepParticles = GetComponentInChildren<ParticleSystem>();
        theAnimator = GetComponentInChildren<Animator>();
        theRB = GetComponent<Rigidbody2D>();
        theBoxCollider = GetComponent<BoxCollider2D>();
        footEmission = footstepParticles.emission;
        playRandomSounds = GetComponent<PlayRandomSounds>();

        #region Error Checks

        if (footstepParticles == null)
            Debug.LogError($"You are missing a ParticleSystem component on the Player object.");
        if (theAnimator == null)
            Debug.LogError($"You are missing an Animator component on the Player object.");
        if (theRB == null)
            Debug.LogError($"You are missing a Rigidbody2D component on the Player object.");
        if (theBoxCollider == null)
            Debug.LogError($"You are missing a BoxCollider2D component on the Player object.");
        if (theRB == null)
            Debug.LogError($"You are missing a Rigidbody2D component on the Player object.");
        #endregion
    }

    private void Update()
    {
        //Update the Animator controller with the player's grounded state
        theAnimator.SetBool("grounded", IsGrounded());

        //Get horizontal input from the player
        inputDirection = Input.GetAxisRaw("Horizontal");
        CheckPlayerInput();
        PlayFootsteps();
    }

    private void PlayFootsteps()
    {
        //Footstep particles when grounded and moving either right or left
        if (IsGrounded() && Input.GetAxisRaw("Horizontal") != 0)
        {
            footEmission.rateOverDistance = particlesOverDistance;
            playRandomSounds.PlaySound();
        }
        else
        {
            footEmission.rateOverDistance = 0;
        }
    }

    private void FixedUpdate()
    {
        //All physics code should be handled inside FixedUpdate()

        theRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        //Manage Hang-time
        if (IsGrounded() && theRB.velocity.y == 0)
        {
            hangCounter = hangTime;
            //Debug.Log($"hangCounter={hangCounter}");
        }
        else
        {
            hangCounter -= Time.deltaTime;
            //Debug.Log($"hangCounter={hangCounter}");
        }

        //Move the player
        MovePlayer(inputDirection);
        //rigidbody2d.velocity = new Vector2(move, rigidbody2d.velocity.y);
    }

    private void CheckPlayerInput()
    {
        //Manage jump buffer
        if (Input.GetButtonDown("Jump"))
        {
            //Jump button was pressed
            jumpBufferCount = jumpBufferTime;
        }
        else
        {
            jumpBufferCount -= Time.deltaTime;
        }

        //Check managed Hang-time and jump buffer before modifying the velocity for Jump
        if (jumpBufferCount >= 0 && hangCounter > 0)
        {
            theRB.velocity = Vector2.up * jumpVelocity;
            theAnimator.SetTrigger("jump");

            //Reset counters
            jumpBufferCount = 0;
            hangCounter = 0;
        }

        //If the Jump button was released and Player is moving up
        if (Input.GetButtonUp("Jump") && theRB.velocity.y > 0) 
        {
            //Cut the player's y-velocity in half
            theRB.velocity = new Vector2(theRB.velocity.x, theRB.velocity.y * 0.5f);
        }

        if (Input.GetKey(KeyCode.A))
        {
            //Move left
            MovePlayer(-inputDirection);
        }
        else
        {
            if (Input.GetKey(KeyCode.D))
            {
                //Move right
                MovePlayer(inputDirection);
            }
            else
            {
                //Stop all movement
                MovePlayer(0);
                theRB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
        }
        
        //R Key was pressed - Reset Player position to home (0, 0, 0)
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = Vector3.zero;
        }
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.1f;
        Color rayColor;

        RaycastHit2D raycastHit = Physics2D.BoxCast(theBoxCollider.bounds.center, theBoxCollider.bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        
        //If hit something...
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else 
        {
            rayColor = Color.red;
        }
        
        Debug.DrawRay(theBoxCollider.bounds.center + new Vector3(theBoxCollider.bounds.extents.x, 0), Vector2.down * (theBoxCollider.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(theBoxCollider.bounds.center - new Vector3(theBoxCollider.bounds.extents.x, 0), Vector2.down * (theBoxCollider.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(theBoxCollider.bounds.center - new Vector3(theBoxCollider.bounds.extents.x, theBoxCollider.bounds.extents.y), (Vector2.right * theBoxCollider.bounds.extents.x) * 2, rayColor);
        //Debug.Log(raycastHit.collider);

        return raycastHit.collider != null;
    }

    private void MovePlayer(float direction)
    {
        theAnimator.SetFloat("move", direction);
        theRB.velocity = new Vector2(inputDirection * moveVelocity, theRB.velocity.y);
    }
}