using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveVelocity = 4f;
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ParticleSystem footstepParticles;
    [SerializeField] private float hangTime = 0.2f;
    [SerializeField] private float jumpBufferLength = 0.1f;
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
        theAnimator = GetComponentInChildren<Animator>();
        theRB = GetComponent<Rigidbody2D>();
        theBoxCollider = GetComponent<BoxCollider2D>();
        footEmission = footstepParticles.emission;
        playRandomSounds = GetComponent<PlayRandomSounds>();
    }

    private void Update()
    {
        //Update the Animator controller with the player's grounded state
        theAnimator.SetBool("grounded", IsGrounded());

        //Get horizontal input from the player
        inputDirection = Input.GetAxisRaw("Horizontal");
        CheckPlayerInput();

        //Footstep particles
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

        MovePlayer(inputDirection);

        //Move the player
        //rigidbody2d.velocity = new Vector2(move, rigidbody2d.velocity.y);
    }

    private void CheckPlayerInput()
    {
        //Manage jump buffer
        if (Input.GetButtonDown("Jump"))
        {
            //Jump button was pressed
            jumpBufferCount = jumpBufferLength;
        }
        else
        {
            jumpBufferCount -= Time.deltaTime;
        }

        //Check managed Hang-time and jump buffer before modifying the velocity
        if (jumpBufferCount >= 0 && hangCounter > 0)
        {
            theRB.velocity = Vector2.up * jumpVelocity;
            theAnimator.SetTrigger("jump");
            jumpBufferCount = 0;
            hangCounter = 0;
        }

        //Jump button was released and Player is moving up
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