using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Here is the "blueprint" that defines an Enemy

    protected float health;
    protected float speed;
    protected int diamonds;

    public void Attack()
    {
        Debug.Log($"{this.gameObject.name} attack!");
    }
}
