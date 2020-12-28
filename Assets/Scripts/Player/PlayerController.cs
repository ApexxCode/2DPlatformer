using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveVelocity = 4f;
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rigidbody2d;
    private BoxCollider2D boxcollider2d;
    private float inputDirection;

    [SerializeField] private bool grounded;

    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxcollider2d = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        //Get horizontal input from the player
        inputDirection = Input.GetAxisRaw("Horizontal");
        
        CheckPlayerInput();
    }

    private void FixedUpdate()
    {
        //All physics code should be handled inside FixedUpdate()

        rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        MovePlayer(inputDirection);

        //Move the player
        //rigidbody2d.velocity = new Vector2(move, rigidbody2d.velocity.y);
    }

    private void CheckPlayerInput()
    {
        if (IsGrounded() && Input.GetButtonDown("Jump"))
        {
            rigidbody2d.velocity = Vector2.up * jumpVelocity;
        }

        if (Input.GetKey(KeyCode.A))
        {
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
                rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
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
        RaycastHit2D raycastHit = Physics2D.Raycast(boxcollider2d.bounds.center, Vector2.down, boxcollider2d.bounds.extents.y + extraHeight, groundLayer.value);
        Color rayColor;

        //If hit something...
        if (raycastHit.collider != null)
        {
            grounded = true;
            rayColor = Color.green;
        }
        else 
        {
            grounded = false;
            rayColor = Color.red;
        }
        
        Debug.DrawRay(boxcollider2d.bounds.center, Vector2.down * (boxcollider2d.bounds.extents.y + extraHeight), rayColor);
        Debug.Log(raycastHit.collider);

        return raycastHit.collider != null;
    }

    private void MovePlayer(float direction)
    {
        rigidbody2d.velocity = new Vector2(inputDirection * moveVelocity, rigidbody2d.velocity.y);
    }
}
