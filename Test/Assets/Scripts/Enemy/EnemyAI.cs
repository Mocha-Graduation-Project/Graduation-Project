using Player;
using Scripts.Scriptable;
using Scripts.UI;
using Systems;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;

    [JapaneseLabel("動かすオブジェクト")] public GameObject moveObj;
    [JapaneseLabel("弾を出す場所")] public GameObject shotObj;
    [JapaneseLabel("ストレート時の角度参照オブジェクト")] public GameObject straightObj;
    public DamageUI damageText;
    [JapaneseLabel("警告UI")] public BeforeAttack beforeAttackText;
    public EnemySpawnManager enemySpawnManager;
    [JapaneseLabel("プレイヤー")] public Player.Player player => Player.Player.Instance;
    private AudioSource audioSource;
    private AudioClip EnemyShotSound;
    private AudioClip DamageSound;
    private AudioClip EnemyDestorySound;
    
    private Collider loopAreaCollider;
    
    public void Awake()
    {
        GameObject loopAreaObj = GameObject.FindWithTag("LoopArea");
        if (loopAreaObj != null)
        {
            loopAreaCollider = loopAreaObj.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("LoopAreaColliderが見つかりません。LoopAreaタグを持つGameObjectを配置してください。");
            return;
        }
            
        EnemyShotSound = enemyData.soundData.EnemyShotSound;
        DamageSound = enemyData.soundData.DamageSound;
        EnemyDestorySound = enemyData.soundData.EnemyDestorySound;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        enemySpawnManager = GameObject.FindObjectOfType<EnemySpawnManager>();

        //攻撃
        switch (enemyData.enemyAttackType)
        {
            case EnemyData.EnemyAttackType.dontAttack:
                break;
            case EnemyData.EnemyAttackType.playerAim:
                StartAttack();
                break;
            case EnemyData.EnemyAttackType.straight:
                StartAttack();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestShow()
    {
        Debug.Log("EnemyAI:" + player);
    }
    
    public virtual bool EnemyMove(GameObject enemyPos,Vector3 start,Vector3 end,float time,float startTime)
    {
        float diff = Time.time - startTime;
        if (diff > time)
        {
            return true;
        }
        float rate = diff / time;
        enemyPos.transform.position = Vector3.Lerp(start, end, rate);

        return false;
    }

    public virtual void EnemyAttack()
    {
        beforeAttackText.After();

        GameObject bullets = Instantiate(enemyData.bulletObj, shotObj.transform.position, Quaternion.identity);

        Bullet reflectionBullet = bullets.GetComponent<Bullet>();
        switch (enemyData.enemyAttackType)
        {
            case EnemyData.EnemyAttackType.playerAim:
                reflectionBullet.SetPowerEnemy(transform.position);
                break;
            case EnemyData.EnemyAttackType.straight:
                reflectionBullet.SetStraightPowerEnemy(straightObj.transform.rotation.eulerAngles);
                break;
        }
        
        audioSource.PlayOneShot(EnemyShotSound);
        StartAttack();
    }
    
    public void BeforeAttack()
    {
        if (enemyData.enemyAttackType == EnemyData.EnemyAttackType.dontAttack) { return; }
            
        beforeAttackText.Warning(enemyData.blinkDuration);
    }

    public void StopAttack()
    {
        CancelInvoke();
        beforeAttackText.After();
    }

    public void StartAttack()
    {
        Invoke("BeforeAttack", enemyData.bulletRate - enemyData.beforeAttackTime);
        Invoke("EnemyAttack", enemyData.bulletRate);
    }
}
