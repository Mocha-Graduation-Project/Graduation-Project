using Enemy.Basic;
using UnityEngine;

public class JumpEnemyHitBox : MonoBehaviour
{
    private JumpEnemy jumpEnemy;

    void Awake()
    {
        jumpEnemy = GetComponentInParent<JumpEnemy>();
    }

    void Update()
    {
        if (jumpEnemy != null)
        {
            transform.position = jumpEnemy.transform.position;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            jumpEnemy.Jump();
        }
    }
}