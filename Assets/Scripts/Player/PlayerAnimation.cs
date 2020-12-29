using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator theAnimator;

    private void Awake()
    {
        theAnimator = GetComponentInChildren<Animator>();
    }

    public void Move(float move)
    {
        theAnimator.SetFloat("move", Mathf.Abs(move));
    }
}