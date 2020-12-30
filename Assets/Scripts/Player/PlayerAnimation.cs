using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator theAnimator;

    private void Awake()
    {
        
    }

    private void Start()
    {
        theAnimator = GetComponentInChildren<Animator>();
    }

    public void Idle()
    {
        
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