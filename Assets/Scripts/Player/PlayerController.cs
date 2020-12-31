using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Physics")]
    //[SerializeField, Tooltip("How much time is allowed when the player is over a ledge to allow a jump.")] private float hangTime = 0.2f;
    //[SerializeField, Tooltip("How much time is allowed to enable jump, before hitting the ground.")] private float jumpBufferTime = 0.1f;
    [SerializeField, Tooltip("How much force to apply to the Player's vertical position when jumping.")] private float jumpVelocity = 12f;
    [SerializeField, Tooltip("How much force to apply the the Player's horizontal position.")] private float moveVelocity = 4f;
    [SerializeField, Tooltip("Select or Create a Layer designated for the ground.")] private LayerMask groundLayer;

    [Header("Player Effects")]
    [SerializeField, Tooltip("The ParticleSystem you would like for footsteps.")] private ParticleSystem _footstepParticles;
    [SerializeField, Tooltip("The amount of particles to emmit over distance."), Range(0.1f, 10.0f)] private float particlesOverDistance = 1f;

    private BoxCollider2D _boxCollider;
    private ParticleSystem.EmissionModule _footEmission;
    private PlayerAnimation _playerAnimation;
    private PlayFootstepsSounds _playFootsteps;
    private Rigidbody2D _rigid;
    private bool _facingRight = true, _grounded, _jumping, _attacking;
    private float hangCounter;
    private float horizontalInput;
    private float jumpBufferCount;

    private void Start()
    {
        _footstepParticles = GetComponentInChildren<ParticleSystem>();
        _rigid = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _footEmission = _footstepParticles.emission;
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playFootsteps = GetComponent<PlayFootstepsSounds>();

        _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        CheckUserInput();
        //PlayFootsteps();
    }

    private void FixedUpdate()
    {
        //All physics code should be handled inside FixedUpdate()

        if (_playerAnimation.animator.GetCurrentAnimatorStateInfo(0).IsTag("attack"))
        {
            _rigid.velocity = new Vector2(0, _rigid.velocity.y);
            return;
        }

        Movement();
    }

    private void CheckUserInput()
    {
        //Cache the attack animation state
        _attacking = _playerAnimation.animator.GetCurrentAnimatorStateInfo(0).IsTag("attack");
        
        //Cache horizontal input from the player
        horizontalInput = Input.GetAxisRaw("Horizontal");

        //Cache is grounded state
        _grounded = IsGrounded();

        //Lock and unlock the mouse cursor
        LockCursor.instance.UpdateCursorLock();

        //Left Mouse click + is grounded + not already attacking
        if (IsGrounded() && Input.GetMouseButtonDown(0) && !_attacking)
        {
            //Attack
            _playerAnimation.Attack();
        }   

        //If moving right and facing left OR moving left and facing right
        //flip the character facing the correct direction.
        if (horizontalInput > 0 && !_facingRight || horizontalInput < 0 && _facingRight)
        {
            //Make sure to disable flipping while the Player is in the Attack animation
            if (!_playerAnimation.animator.GetCurrentAnimatorStateInfo(0).IsTag("attack"))
                Flip();
        }

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            _playerAnimation.Jump(true);
            _jumping = true;
        }

        //R Key was pressed - Reset Player position to home (0, 0, 0)
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = Vector3.zero;
        }

        //Animate the movement
        _playerAnimation.Move(horizontalInput);

        PlayFootsteps();
    }

    private void PlayFootsteps()
    {
        if (_attacking)
            return;

        //Footstep particles when grounded and moving either right or left
        if (IsGrounded() && horizontalInput != 0)
        {
            //Play a random sound effect for steps taken
            _playFootsteps.PlaySound();

            //Modify the ParticleSystem emissions for the "rock debris"
            _footEmission.rateOverDistance = particlesOverDistance;
        }
        else
        {
            _footEmission.rateOverDistance = 0;
        }
    }

    private void Movement()
    {
        if (_jumping)
        {
            _rigid.velocity = Vector2.up * jumpVelocity;
            _playerAnimation.Jump(true);
            _jumping = false;
        }

        //Move the player
        _rigid.velocity = new Vector2(horizontalInput * moveVelocity, _rigid.velocity.y);
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.1f;
        Color rayColor;

        RaycastHit2D raycastHit = Physics2D.BoxCast(_boxCollider.bounds.center, _boxCollider.bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        
        //If hit something...
        if (raycastHit.collider != null)
        {
            _playerAnimation.Jump(false);

            //Hit the ground layer
            rayColor = Color.green;
        }
        else 
        {
            //Not touching the ground layer
            rayColor = Color.red;
        }
        
        //Draw a few rays around the player's lower half to show the collision
        Debug.DrawRay(_boxCollider.bounds.center + new Vector3(_boxCollider.bounds.extents.x, 0), Vector2.down * (_boxCollider.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(_boxCollider.bounds.center - new Vector3(_boxCollider.bounds.extents.x, 0), Vector2.down * (_boxCollider.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(_boxCollider.bounds.center - new Vector3(_boxCollider.bounds.extents.x, _boxCollider.bounds.extents.y), (Vector2.right * _boxCollider.bounds.extents.x) * 2, rayColor);
        //Debug.Log(raycastHit.collider);

        return raycastHit.collider != null;
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        transform.rotation = Quaternion.Euler(0, _facingRight ? 0 : 180, 0);
    }
}