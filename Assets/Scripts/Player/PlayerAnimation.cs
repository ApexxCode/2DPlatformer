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

    public void Move(float value)
    {
        animator.SetFloat("Move", Mathf.Abs(value));
    }

    public void Jump(bool value)
    {
        animator.SetBool("Jumping", value);
    }
}