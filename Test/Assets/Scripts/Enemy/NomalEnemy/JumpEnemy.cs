using UnityEngine;

namespace Enemy.Basic
{
    public class JumpEnemy : EnemyAI
    {
        [SerializeField] Animator animator;
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int JumpUp = Animator.StringToHash("JumpUp");
        private static readonly int JumpDown = Animator.StringToHash("JumpDown");
        
        [SerializeField] private float jumpTime;
        
        private bool isGrounded;

        [SerializeField] private GameObject hitBox;
    
        Rigidbody rigidbody;

        public override void SetUp()
        {
            animator = GetComponentInChildren<Animator>();
            jumpTime = 0;
            rigidbody = GetComponent<Rigidbody>();
        
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            if (hitBox != null)
            {
                hitBox.transform.parent = null;
            }
        }
        
        override protected void Update()
        {
            base.Update();
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, enemyData.groundLayer);
        }

        public void Jump()
        {
            if (!isGrounded) return; 
            Debug.Log("Jump");
            rigidbody.AddForce(Vector3.up * enemyData.jumpPower, ForceMode.Impulse);
            jumpTime = 0;
        }

        private void OnDestroy()
        {
            Destroy(hitBox);
        }
    }
}
