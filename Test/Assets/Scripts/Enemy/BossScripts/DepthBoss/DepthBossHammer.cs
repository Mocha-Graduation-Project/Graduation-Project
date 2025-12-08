using Player;
using UnityEngine;

public class DepthBossHammer : MonoBehaviour
{
    [SerializeField] private DepthBoss depthBoss;
    
    private void OnTriggerEnter(Collider other)
    {
        if (depthBoss.IsAttack == true && other.tag == "Player")
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
                depthBoss.TakeDamage(bullet.Damage);
            }
        }
    }
}
