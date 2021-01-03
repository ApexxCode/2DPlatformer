using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Physics")]
    //==========================================================================================
    [SerializeField, Tooltip("How much time to be able to jump, after leaving the \"ground\".")] private float hangTime = 0.2f;
    private float hangCounter;
    [SerializeField, Tooltip("How much time to be able to jump, before hitting the \"ground\".")] private float jumpBufferTime = 0.1f;
    private float jumpBufferCount;
    [SerializeField, Tooltip("How much force to apply to the Player's vertical position when jumping.")] private float jumpVelocity = 12f;
    [SerializeField, Tooltip("How much force to apply the the Player's horizontal position.")] private float moveVelocity = 4f;
    [SerializeField, Tooltip("Select or Create a Layer designated for the ground.")] private LayerMask groundLayer;

    [Header("Player Effects")]
    //==========================================================================================
    [SerializeField, Tooltip("The ParticleSystem you would like for footsteps.")] private ParticleSystem _footstepParticles;
    [SerializeField, Tooltip("The amount of particles to emmit over distance."), Range(0.1f, 100.0f)] private float particlesOverDistance = 1f;

    private AudioManager _audioManager;
    private BoxCollider2D _boxCollider;
    private ParticleSystem.EmissionModule _footEmission;
    private PlayerAnimation _playerAnimation;
    private PlayFootstepsSounds _playFootsteps;
    private Rigidbody2D _rigid;
    private bool _facingRight = true, _resetJump, _grounded = true, _attacking, _onPlatformLastFrame, _landing, _wasGrounded;
    private float horizontalInput;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playFootsteps = GetComponent<PlayFootstepsSounds>();
        _rigid = GetComponent<Rigidbody2D>();
        
        _footstepParticles = GetComponentInChildren<ParticleSystem>();
        _footEmission = _footstepParticles.emission;

        _audioManager = FindObjectOfType<AudioManager>();
        
        //Lock the rotation on Player's rigidbody
        _rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        #region Manage Jump Hangtime

        //Hangtime (A.k.a "Coyote Hangtime")
        //Used to allow the Player to jump for a short period after not being on the ground
        if (IsGrounded() && !_attacking)
        {
            //Always keep the hangCounter = hangTime while touching the ground
            hangCounter = hangTime;
        }
        else
        {
            //Not on the ground, start reducing the hangCounter
            hangCounter -= Time.deltaTime;
        }
        #endregion

        #region Manage Jump Buffer Time

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCount = jumpBufferTime;
        }
        else
        {
            jumpBufferCount -= Time.deltaTime;
        }
        #endregion

        CheckUserInput();
    }

    private void FixedUpdate()
    {
        _wasGrounded = _grounded;

        //Cache is grounded state
        _grounded = IsGrounded();

        Movement();
    }

    private void CheckUserInput()
    {
        //Lock and unlock the mouse cursor
        LockCursor.instance.UpdateCursorLock();

        //Cache the attack animation state
        _attacking = _playerAnimation._animator.GetCurrentAnimatorStateInfo(0).IsTag("attack");

        //Cache horizontal input from the player
        horizontalInput = Input.GetAxisRaw("Horizontal");

        #region Player Flip Logic

        //If moving right and facing left OR moving left and facing right
        //flip the character facing the correct direction.
        if (horizontalInput > 0 && !_facingRight || horizontalInput < 0 && _facingRight)
        {
            //Make sure to disable flipping while the Player is in the Attack animation
            if (!_playerAnimation._animator.GetCurrentAnimatorStateInfo(0).IsTag("attack"))
                FlipPlayer();
        }
        #endregion

        #region Jump

        //Referencing our timers from FixedUpdate()
        if (jumpBufferCount >= 0 && hangCounter > 0f)
        {
            _audioManager.Play("PlayerJumpGear");
            //_audioManager.Play("PlayerJump");
            _rigid.velocity = Vector2.up * jumpVelocity;
            _playerAnimation.Jump(true);
            StartCoroutine(ResetJumpRoutine());
        }

        //Released jump button and moving upwards
        if (Input.GetButtonUp("Jump") && _rigid.velocity.y > 0)
        {
            _rigid.velocity = new Vector2(_rigid.velocity.x, _rigid.velocity.y * 0.5f);
        }
        #endregion

        #region Attack

        //Left Mouse click + is grounded + not already attacking
        if (IsGrounded() && Input.GetButtonDown("Fire1") && !_attacking)
        {
            //Attack
            _audioManager.Play("PlayerSword");
            _audioManager.Play("PlayerAttack");
            _playerAnimation.Attack();
        }
        #endregion

        #region R-Key (Reset Player's position)

        //R Key was pressed - Reset Player position to home (0, 0, 0)
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = Vector3.zero;
        }
        #endregion

        //Tell the Animator component to animate based on user input
        _playerAnimation.Move(horizontalInput);

        //Play sounds effects of walking and rock debris from Player's feet
        PlayFootsteps();
    }

    private void PlayFootsteps()
    {
        if (_attacking || _landing)
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
        //State changed from not being on the ground to being on the ground
        if (_wasGrounded != _grounded && IsGrounded())
        {
            _landing = true;
            _audioManager.Play("PlayerLand");
            _playerAnimation.Jump(false);
        }

        #region Player Attacking

        if (_attacking)
        {
            //Stop all movement on the Horizontal axis.
            _rigid.velocity = new Vector2(0, _rigid.velocity.y);
            return;
        }
        #endregion

        //if (IsGrounded())
        //    _playerAnimation.Jump(false);

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
            //Hit the ground layer
            rayColor = Color.green;

            _landing = false;

            if (!_onPlatformLastFrame)
            {
                _onPlatformLastFrame = raycastHit.collider;
            }

            if (!_resetJump)
                return true;
        }
        else 
        {
            //Not touching the ground layer
            rayColor = Color.red;
        }

        //Draw a few rays around the player's lower half to show the collision
        //Debug.DrawRay(_boxCollider.bounds.center + new Vector3(_boxCollider.bounds.extents.x, 0), Vector2.down * (_boxCollider.bounds.extents.y + extraHeight), rayColor);
        //Debug.DrawRay(_boxCollider.bounds.center - new Vector3(_boxCollider.bounds.extents.x, 0), Vector2.down * (_boxCollider.bounds.extents.y + extraHeight), rayColor);
        //Debug.DrawRay(_boxCollider.bounds.center - new Vector3(_boxCollider.bounds.extents.x, _boxCollider.bounds.extents.y), (Vector2.right * _boxCollider.bounds.extents.x) * 2, rayColor);
        //Debug.Log(raycastHit.collider);

        //return raycastHit.collider != null;
        return false;
    }

    private void FlipPlayer()
    {
        _facingRight = !_facingRight;
        
        //This flip method worked until I noticed the issue with SwordEffect animation not flipping correctly.
        //transform.rotation = Quaternion.Euler(0, _facingRight ? 0 : 180, 0);

        transform.localScale = new Vector3(_facingRight ? 1 : -1, 1, 1);
    }

    IEnumerator ResetJumpRoutine()
    {
        _resetJump = true;
        jumpBufferCount = 0;
        hangCounter = 0;
        yield return new WaitForSeconds(0.1f);
        _resetJump = false;
    }
}