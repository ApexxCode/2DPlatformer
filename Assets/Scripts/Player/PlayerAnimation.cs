using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator theAnim;

    private void Awake()
    {
        theAnim = GetComponentInChildren<Animator>();
    }

    public void Move(float move)
    {
        if (theAnim != null)
        {
            theAnim.SetFloat("move", Mathf.Abs(move));
        }
    }
}