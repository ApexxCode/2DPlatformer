using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveVelocity = 4f;
    public float jumpVelocity = 12f;
    
    //"Coyote Hang-time (Allows the player to jump even if off the ledges for a brief moment)
    public float hangTime = 0.2f;
    private float hangCounter;

    public float jumpBufferLength = 0.1f;
    private float jumpBufferCount;

    public ParticleSystem footstepParticles;
    public int particlesOverDistance;
    private ParticleSystem.EmissionModule footEmission;

    private Rigidbody2D theRB;
    private BoxCollider2D theBoxCollider;
    private float inputDirection;
    public LayerMask groundLayer;

    private void Awake()
    {
        theRB = GetComponent<Rigidbody2D>();
        theBoxCollider = GetComponent<BoxCollider2D>();
        footEmission = footstepParticles.emission;
    }

    private void Update()
    {
        //Get horizontal input from the player
        inputDirection = Input.GetAxisRaw("Horizontal");
        CheckPlayerInput();

        //Footstep particles
        if (IsGrounded() && Input.GetAxisRaw("Horizontal") != 0)
        {
            footEmission.rateOverDistance = particlesOverDistance;
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
            Debug.Log($"hangCounter={hangCounter}");
        }
        else
        {
            hangCounter -= Time.deltaTime;
            Debug.Log($"hangCounter={hangCounter}");
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
            jumpBufferCount = 0;
            hangCounter = 0;
        }

        //Jump button was released and Player is moving up
        if (Input.GetButtonUp("Jump") && theRB.velocity.y > 0) 
        {
            //Cut the player's y-velocity in half
            theRB.velocity = new Vector2(theRB.velocity.x, theRB.velocity.y * 0.5f);
        }

        if (Input.GetKey(KeyCode.A)) {
            MovePlayer(-inputDirection);
        }
        else
        {
            if (Input.GetKey(KeyCode.D))
            {
                MovePlayer(inputDirection);
            }
            else
            {
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
        //Cast a ray straight down
        RaycastHit2D raycastHit = Physics2D.BoxCast(theBoxCollider.bounds.center, theBoxCollider.bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        Color rayColor;

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
        theRB.velocity = new Vector2(inputDirection * moveVelocity, theRB.velocity.y);
    }
}