using UnityEngine;

public class JumpEnemy : MonoBehaviour
{
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpCoolTime;
    [SerializeField] private float jumpTime;
    
    Rigidbody2D rigidbody2D;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpTime = 0;
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.constraints = RigidbodyConstraints2D.None;
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation|RigidbodyConstraints2D.FreezePositionX;
    }

    // Update is called once per frame
    void Update()
    {
        Jump();
    }

    void Jump()
    {
        jumpTime += Time.deltaTime;
        if (jumpTime < jumpCoolTime) return;
        
        Debug.Log("Jump");
        rigidbody2D.AddForce(Vector2.up * jumpPower);
        jumpTime = 0;
    }
}
