using Player;
using UnityEngine;

public class HumanoidBossQuickAttackArea : MonoBehaviour
{
    [SerializeField] private HumanoidBoss humanoidBoss;
    private int quickAttackAbleDamage = 16;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            
            if (bullet.Damage <= quickAttackAbleDamage)
            {
                humanoidBoss.QuickAttack();
            }
        }
    }
}
