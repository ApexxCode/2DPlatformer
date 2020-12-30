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
    private bool facingRight = true, jumping, resetJump;
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
        Movement();
    }

    private void CheckUserInput()
    {
        //Store horizontal input from the player
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0 && !facingRight || horizontalInput < 0 && facingRight)
            Flip();

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            jumping = true;
        }

        //R Key was pressed - Reset Player position to home (0, 0, 0)
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = Vector3.zero;
        }
    }

    private void PlayFootsteps()
    {
        //Footstep particles when grounded and moving either right or left
        if (IsGrounded() && horizontalInput != 0)
        {
            _playFootsteps.PlaySound();
            _footEmission.rateOverDistance = particlesOverDistance;
        }
        else
        {
            _footEmission.rateOverDistance = 0;
        }
    }

    private void Movement()
    {
        PlayFootsteps();

        if (jumping)
        {
            _rigid.velocity = Vector2.up * jumpVelocity;
            jumping = false;
        }

        //Move the player
        _rigid.velocity = new Vector2(horizontalInput * moveVelocity, _rigid.velocity.y);

        _playerAnimation.Move(horizontalInput);
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.1f;
        Color rayColor;

        RaycastHit2D raycastHit = Physics2D.BoxCast(_boxCollider.bounds.center, _boxCollider.bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        
        //If hit something...
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else 
        {
            rayColor = Color.red;
        }
        
        Debug.DrawRay(_boxCollider.bounds.center + new Vector3(_boxCollider.bounds.extents.x, 0), Vector2.down * (_boxCollider.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(_boxCollider.bounds.center - new Vector3(_boxCollider.bounds.extents.x, 0), Vector2.down * (_boxCollider.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(_boxCollider.bounds.center - new Vector3(_boxCollider.bounds.extents.x, _boxCollider.bounds.extents.y), (Vector2.right * _boxCollider.bounds.extents.x) * 2, rayColor);
        //Debug.Log(raycastHit.collider);

        return raycastHit.collider != null;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }
}