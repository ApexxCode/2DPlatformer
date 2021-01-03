using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MossGiant : Enemy
{
    private void Start()
    {
        Attack();
    }
    
    public override void Update()
    {
        Debug.Log($"MossGiant updating...");
    }
}
