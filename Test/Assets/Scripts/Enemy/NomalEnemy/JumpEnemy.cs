using UnityEngine;

namespace Enemy.Basic
{
    public class JumpEnemy : EnemyAI
    {
        [SerializeField] Animator animator;
        // private static readonly int Idle = Animator.StringToHash("Idle");
        // private static readonly int JumpUp = Animator.StringToHash("JumpUp");
        // private static readonly int JumpDown = Animator.StringToHash("JumpDown");
        
        private float jumpTime;
        
        private bool isGrounded;

        [SerializeField] private GameObject hitBox;

        private Rigidbody rb;
        
        [SerializeField] [JapaneseLabel("地面のレイヤー")] private LayerMask groundLayer;

        public override void SetUp()
        {
            animator = GetComponentInChildren<Animator>();
            jumpTime = 0;
            rb = GetComponent<Rigidbody>();
        
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            if (hitBox != null)
            {
                hitBox.transform.parent = null;
            }

            IsSetUp = true;
        }
        
        override protected void Update()
        {
            base.Update();
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
            if (isGrounded) 
            {
                jumpTime += Time.deltaTime; 
            }
        }

        public override void EnemyAttack()
        {
            base.EnemyAttack();
            animator.SetTrigger("ShotTrigger");
        }

        public void Jump()
        {
            if (!isGrounded || jumpTime < CSVData.enemiesData[DataNumber].jumpCoolTime) return;
            animator.SetTrigger("JumpTrigger");
            rb.AddForce(Vector3.up * CSVData.enemiesData[DataNumber].jumpPower, ForceMode.Impulse);
            jumpTime = 0;
        }

        private void OnDestroy()
        {
            Destroy(hitBox);
        }
    }
}
