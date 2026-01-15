using UnityEngine;

// #if UNITY_EDITOR
// [CustomEditor(typeof(EnemyAI))]
// #endif

public class Soldier : EnemyAI
{
    [SerializeField] private Animator animator;
    
    public override void TakeDamage(int damage)
    {
        if (Hp - damage > 0)
        {
            animator.SetTrigger("TakeDamage");
        }
        else
        {
            animator.SetTrigger("Dead");
        }
        base.TakeDamage(damage);
    }

    public override void BeforeAttack()
    {
        base.BeforeAttack();
        animator.SetTrigger("Shoot");
    }
}
