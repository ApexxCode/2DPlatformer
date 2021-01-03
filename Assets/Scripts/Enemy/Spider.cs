using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : Enemy
{
    private void Start()
    {
        Attack();
    }

    public override void Update()
    {
        Debug.Log($"Spider upating...");
    }

    public override void Attack()
    {
        Debug.Log("Spider attack");
    }
}
