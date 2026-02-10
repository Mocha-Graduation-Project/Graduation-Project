using UnityEngine;
using UnityEngine.VFX;

// #if UNITY_EDITOR
// [CustomEditor(typeof(EnemyAI))]
// #endif

public class Shielder : EnemyAI
{
    [SerializeField] private Animator animator;

    [JapaneseLabel("シールド防御エフェクト")] [SerializeField] private GameObject shieldBlockEffect;
    
    private AudioClip shieldSound;
    
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

    public void BlockShield()
    {
        animator.SetTrigger("Block");
        shieldBlockEffect.GetComponent<VisualEffect>().SendEvent("OnPlay");
        AudioSource.PlayOneShot(shieldSound);
    }

    public override void OnTriggerEnter(Collider collider)
    {
        if (IsSetUp == false)
        {
            Debug.Log("セットアップが完了していません");
            return;
        }
    }

    public override void SetUp()
    {
        shieldSound = SoundData.shieldSound;
        IsSetUp=true;
    }
}
