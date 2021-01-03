using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    //Here is the "blueprint" that defines an Enemy

    [SerializeField] protected float health;
    [SerializeField] protected float speed;
    [SerializeField] protected int diamonds;

    public abstract void Update();

    public virtual void Attack()
    {
        Debug.Log($"{this.gameObject.name} Base attack");
    }
}
