using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
    public int maxHealth;
    protected int currentHealh;
    protected Animator animator;

    protected void  Init()
    {
        animator = GetComponent<Animator>();
        maxHealth = currentHealh;  
    }

    protected void Walk(bool walk)
    {
        animator.SetBool("Walk",walk);
    }

    protected void Attack()
    {
        animator.SetTrigger("Attack");
    }
}
