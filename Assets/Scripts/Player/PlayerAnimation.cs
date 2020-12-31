using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator _animator;
    public Animator _swordAnimator;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();

        //Reference the 2nd component down from inside the Playe object
        _swordAnimator = transform.GetChild(1).GetComponent<Animator>();
    }

    public void Attack()
    {
        _animator.SetTrigger("Attack");
        _swordAnimator.SetTrigger("SwordEffect");
    }

    public void Move(float value)
    {
        _animator.SetFloat("Move", Mathf.Abs(value));
    }

    public void Jump(bool value)
    {
        _animator.SetBool("Jumping", value);
    }
}