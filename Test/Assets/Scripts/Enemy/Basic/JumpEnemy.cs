using UnityEngine;

namespace Enemy.Basic
{
    public class JumpEnemy : MonoBehaviour
    {
        [SerializeField] private float jumpPower;
        [SerializeField] private float jumpCoolTime;
        [SerializeField] private float jumpTime;
    
        [SerializeField] private LayerMask groundLayer;
        private bool isGrounded;

        [SerializeField] private GameObject hitBox;
    
        Rigidbody rigidbody;
    
        void Start()
        {
            jumpTime = 0;
            rigidbody = GetComponent<Rigidbody>();
        
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            if (hitBox != null)
            {
                hitBox.transform.parent = null;
            }
        
        }
    
        void Update()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
            if (isGrounded) 
            {
                jumpTime += Time.deltaTime; 
            }
        }
        public void Jump()
        {
            if (!isGrounded || jumpTime < jumpCoolTime) return;
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            jumpTime = 0;
        }

        private void OnDestroy()
        {
            Destroy(hitBox);
        }
    }
}
