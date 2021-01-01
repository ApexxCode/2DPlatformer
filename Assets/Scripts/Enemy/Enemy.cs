using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Here is the "blueprint" that defines an Enemy

    public float health;
    public float speed;
    public int diamonds;

    public void Attack()
    {
        Debug.Log($"My name is: {this.gameObject.name}");
    }
}
