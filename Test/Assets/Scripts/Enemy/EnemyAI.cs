using System;
using Player;
using Scripts.Scriptable;
using Scripts.UI;
using Systems;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;

    [JapaneseLabel("現在のHP")] public int hp;
    [NonSerialized] [JapaneseLabel("初期(中央)座標")] public Vector3 centerPos;
    [JapaneseLabel("動かすオブジェクト")] public GameObject moveObj;
    [JapaneseLabel("プレイヤーの方を向くオブジェクト")] public GameObject playerLookObj;
    [JapaneseLabel("回転軸")] public GameObject rotateAxis;
    [JapaneseLabel("弾を出す場所")] public GameObject shotObj;
    [JapaneseLabel("ストレート時の角度参照オブジェクト")] public GameObject straightObj;
    public DamageUI damageText;
    [JapaneseLabel("警告UI")] public BeforeAttack beforeAttackText;
    public EnemySpawnManager enemySpawnManager;
    [JapaneseLabel("プレイヤー")] public Player.Player player => Player.Player.Instance;
    [JapaneseLabel("敵のHPバー")] private Slider enemyHPSlider;
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
        centerPos = this.transform.position;
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

        SetUp();
        
        switch (enemyData.enemyType)
        {
            case EnemyData.EnemyType.boss:
                enemyHPSlider = GameObject.FindWithTag("EnemyHPBar").GetComponent<Slider>();
                enemyHPSlider.maxValue = hp;
                enemyHPSlider.value = hp;
                break;
        }
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        //プレイヤーの方を向く処理
        if (player != null)
        {
            // DOLookAt(ターゲットの位置, 回転にかける時間)
            transform.DOLookAt(player.transform.position, 0.5f);
        }

        switch (enemyData.moveType)
        {
            case EnemyData.MoveType.dontMove:
                break;
            case EnemyData.MoveType.custom:
                CustomMove();
                break;
            case EnemyData.MoveType.vertical:
                break;
            case EnemyData.MoveType.horizontal:
                break;
        }
    }

    public void TestShow()
    {
        Debug.Log("EnemyAI:" + enemySpawnManager);
    }
    
    //移動
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

    //回転
    public virtual bool EnemyRotate(GameObject rotateObj, Vector3 angles, float rotatePerSpeed, float rotateTime,
        ref float time)
    {
        time += Time.deltaTime;
        rotateAxis.transform.Rotate(0, 0, rotatePerSpeed * Time.deltaTime);
        if (time >= rotateTime)
        {
            rotateAxis.transform.eulerAngles = angles;
            return true;
        }
        return false;
    }

    //被弾
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Bullet")
        {
            Debug.Log("当たった");
            Bullet bullet = collider.gameObject.GetComponent<Bullet>();
            hp -= bullet.Damage;
                
            if (enemyHPSlider != null)
            {
                enemyHPSlider.value = hp;
            }
            // DamageText.enabled = true;
            // DamageText.text = bullet.Damage.ToString();
            damageText.ShowDamage(bullet.Damage);
            audioSource.PlayOneShot(DamageSound);
        }

        if (hp <= 0)
        {
            switch (enemyData.enemyType)
            {
                case EnemyData.EnemyType.normal:
                    enemySpawnManager.RemoveEnemy(this.gameObject);
                    break;
                case EnemyData.EnemyType.shield:
                    enemySpawnManager.RemoveEnemy(this.gameObject.transform.parent.gameObject);
                    break;
                case EnemyData.EnemyType.boss:
                    enemySpawnManager.RemoveEnemy(this.gameObject.transform.parent.gameObject);
                    break;
            }
        }
    }
    
    //攻撃
    public virtual void EnemyAttack()
    {
        //Debug.Log("Attack");
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
        //Debug.Log("AttackStart");
        Invoke("BeforeAttack", enemyData.bulletRate - enemyData.beforeAttackTime);
        Invoke("EnemyAttack", enemyData.bulletRate);
    }

    public virtual void SetUp() {}
    public virtual void Change() {}
    public virtual void CustomMove() {}
}
