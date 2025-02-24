using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] CharacterData characterData;
    [SerializeField] private int playerHp;
    
    [SerializeField] UILife uiLife;
    
    private string playerBulletTag = "Bullet";
    private string enemyBulletTag = "EnemyBullet";
    
    [SerializeField]SceneManager sceneManager;
    
    public int PlayerHp{ get { return playerHp; } }

    Player player => Player.Instance;
    
    [SerializeField,JapaneseLabel("地面レイヤー")] private LayerMask groundLayer;
    [SerializeField,JapaneseLabel("足元")] private Transform groundCheck; 
    private float checkDistance = 0.05f; // Raycastの長さ

    [SerializeField]private Animator animator; 
    private bool isGrounded;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartSetUp();
        uiLife= uiLife.GetComponent<UILife>();
        sceneManager = GameObject.Find("SceneManager").GetComponent<SceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
    }
    private void CheckGround()
    {
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);
        
        animator.SetBool("isGround",isGrounded);
        
        Debug.DrawRay(groundCheck.position, Vector2.down * checkDistance, Color.red);
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag(playerBulletTag) || other.CompareTag(enemyBulletTag))
    //     {
    //         Damage(1);
    //     }
    // }
    public void Damage(int damage)
    {
        playerHp -= damage;
        uiLife.RemoveLife();
        Debug.Log("PlayerHP:"+playerHp);
        player.PlayDamageSound();
        if (playerHp <= 0 && sceneManager != null)
        {
            sceneManager.Retry();
        }
    }

    public void StartSetUp()
    {
        Debug.Log("StartSetUp");
        playerHp = characterData.InitialHp;
        for (int i = 0; i < characterData.InitialHp; i++)
        {
            uiLife.AddLife();
        }
    }
    
    
}
