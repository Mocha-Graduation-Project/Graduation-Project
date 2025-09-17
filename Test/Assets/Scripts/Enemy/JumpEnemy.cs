using UnityEngine;

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
    }
    public void Jump()
    {
        if (!isGrounded) return; 
            Debug.Log("Jump");
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            //rigidbody2D.AddForce(Vector2.up * jumpPower);
            jumpTime = 0;
    }

    private void OnDestroy()
    {
        Destroy(hitBox);
    }
}
