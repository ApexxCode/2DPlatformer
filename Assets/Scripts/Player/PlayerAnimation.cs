using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void Move(float move)
    {
        animator.SetFloat("Move", Mathf.Abs(move));
    }

    public void Jump(bool jumping)
    {
        animator.SetBool("Jumping", jumping);
    }
}