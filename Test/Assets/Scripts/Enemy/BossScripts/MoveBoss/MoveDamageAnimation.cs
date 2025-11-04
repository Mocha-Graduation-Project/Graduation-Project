using UnityEngine;

namespace Enemy.BossScripts.MoveBoss
{
    public class MoveDamageAnimation : MonoBehaviour
    {
        private static readonly int Damage = Animator.StringToHash("Damage");
        private Animator animator;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet"))
            {
                animator.SetTrigger(Damage);
            }
        }
    }
}