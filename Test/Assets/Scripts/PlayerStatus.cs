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
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerBulletTag) || other.CompareTag(enemyBulletTag))
        {
            Damage(1);
        }
    }

    public void Damage(int damage)
    {
        playerHp -= damage;
        uiLife.RemoveLife();
        Debug.Log("PlayerHP:"+playerHp);

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
