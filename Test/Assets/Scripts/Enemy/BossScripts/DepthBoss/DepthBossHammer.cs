using Player;
using UnityEngine;

public class DepthBossHammer : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // Debug.Log("PlayerHit");
            if (other.TryGetComponent<PlayerStatus>(out PlayerStatus status))
            {
                status.Damage(1);
            }
        }
        else if (other.tag == "Bullet")
        {
            // Debug.Log("BulletHit");
            if (other.TryGetComponent<Bullet>(out Bullet bullet))
            {
                enemyAI.TakeDamage(bullet.Damage);
            }
        }
    }
}
