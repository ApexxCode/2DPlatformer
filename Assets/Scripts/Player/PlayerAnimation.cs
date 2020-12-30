using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator theAnimator;

    private void Start()
    {
        theAnimator = GetComponentInChildren<Animator>();
    }

    public void Attack()
    {
        theAnimator.SetTrigger("Attack");
    }

    public void Move(float move)
    {
        theAnimator.SetFloat("Move", Mathf.Abs(move));
    }

    public void Jump(bool jumping)
    {
        theAnimator.SetBool("Jumping", jumping);
    }
}