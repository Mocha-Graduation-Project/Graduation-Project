using System;
using System.Collections;
using Component;
using Player;
using Scripts.Scriptable;
using Scripts.UI;
using Systems;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.VFX;

public class EnemyAI : MonoBehaviour
{
    private StartUpEnemyData csvData;
    [JapaneseLabel("何番目のデータを取得するか")] private int dataNumber;
    [JapaneseLabel("SetUpが正常に完了したか")] private bool isSetUp;

    [SerializeField] [JapaneseLabel("現在のHP")] private int hp;
    [NonSerialized] [JapaneseLabel("初期(中央)座標")] private Vector3 centerPos;
    [SerializeField] [JapaneseLabel("動かすオブジェクト")] private GameObject moveObj;
    [SerializeField] [JapaneseLabel("プレイヤーの方を向くオブジェクト")] private GameObject playerLookObj;
    [SerializeField] [JapaneseLabel("回転軸")] private GameObject rotateAxis;
    [SerializeField] [JapaneseLabel("弾を出す場所")] private GameObject shotObj;
    [SerializeField] [JapaneseLabel("弾発射時の爆発エフェクト")] private GameObject shotBombEffect;
    [SerializeField] [JapaneseLabel("ストレート時の角度参照オブジェクト")] private GameObject straightObj;
    [SerializeField] private DamageUI damageText;
   // [SerializeField] [JapaneseLabel("警告UI")] private BeforeAttack beforeAttackText;
    [SerializeField] private EnemySpawnManager enemySpawnManager;
    [JapaneseLabel("死亡エフェクト")][SerializeField]private GameObject deathEffectPrefab;
    [JapaneseLabel("被弾エフェクト")] [SerializeField] private GameObject damageEffectPrefab;
    [JapaneseLabel("攻撃エフェクト")][SerializeField] private GameObject attackEffect;
    [Header("<エフェクトリスト>")]
    [SerializeField] private GameObject[] effectPrefabs; 
    [JapaneseLabel("プレイヤー")] public Player.Player player => Player.Player.Instance;
    [JapaneseLabel("敵のHPバー")] private Slider enemyHPSlider;
    [SerializeField] [JapaneseLabel("音源")] private SoundData soundData;
    private AudioSource audioSource;
    private AudioClip EnemyShotSound;
    private AudioClip DamageSound;
    private AudioClip EnemyDestorySound;
    [SerializeField] [JapaneseLabel("発射する弾")] private GameObject bulletObj;
    [JapaneseLabel("弾を発射するレート")] private float bulletRate;
    
    protected Collider loopAreaCollider;
    
    //移動に関する変数(overrideしないで動く用)
    private bool isCoolTime;
    private float t;
    private int moveCounter;
    private Vector3 startPos;
    private Vector3 rightCenterPos;
    private Vector3 leftCenterPos;
    private Vector3 upCenterPos;
    private Vector3 downCenterPos;
    private float rightPosX;
    private float leftPosX;
    private float upPosY;
    private float downPosY;
    
    public StartUpEnemyData CSVData{ get { return csvData; } }
    public int DataNumber { get { return dataNumber; } }
    public int Hp { get { return hp; } }
  //  public BeforeAttack BeforeAttackText { get { return beforeAttackText; } }
    public Vector3 CenterPos { get { return centerPos; } }
    public GameObject MoveObj {get  { return moveObj; } }
    public GameObject BulletObj {get  { return bulletObj; } }
    public GameObject StraightObj { get { return straightObj; } }
    public GameObject RotateAxis { get { return rotateAxis; } }
    public bool IsSetUp { get => isSetUp; set => isSetUp = value; }

    public void Awake()
    {
        if (LoopManager.Instance != null)
        {
            loopAreaCollider = LoopManager.Instance.AreaLoopCollider;
        }
        else
        {
            Debug.LogError("LoopManagerが見つかりません。シーンに配置してください。");
        }
        if (EnemyHPSlider.Instance != null)
        {
            enemyHPSlider = EnemyHPSlider.Instance.Slider;
        }
        else
        {
            Debug.LogError("EnemyHPSliderが見つかりません。シーンに配置してください。");
        }
        EnemyShotSound = soundData.enemyShotSound;
        DamageSound = soundData.damageSound;
        EnemyDestorySound = soundData.enemyDestroySound;
        isSetUp = false;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        centerPos = this.transform.position;
        audioSource = GetComponent<AudioSource>();
        enemySpawnManager = EnemySpawnManager.Instance;
        csvData = FindAnyObjectByType<StartUpEnemyData>();
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (isSetUp == false)
        {
            //Debug.Log("セットアップが完了していません");
            return;
        }
        //プレイヤーの方を向く処理
        if (player != null && playerLookObj != null)
        {
            //Debug.Log("EnemyAIUpdate");
            Vector3 targetDir = Vector3.zero;
            bool shouldRotate = false;

            switch (csvData.enemiesData[dataNumber].playerLookType)
            {
                case StartUpEnemyData.PlayerLookType.look:
                    targetDir = player.transform.localPosition - playerLookObj.transform.localPosition;
                    shouldRotate = true;
                    break;
                case StartUpEnemyData.PlayerLookType.lookY:
                     // playerLookObjがルートならWorldで計算してOK。
                    Vector3 worldLookPos = new Vector3(player.transform.position.x, playerLookObj.transform.position.y, player.transform.position.z);
                    targetDir = worldLookPos - playerLookObj.transform.position;
                    shouldRotate = true;
                    break;
                case StartUpEnemyData.PlayerLookType.dontLook:
                    break;
            }

            if (shouldRotate && targetDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                // DOLookAt(0.5f) の挙動に近いスムーズな回転
                playerLookObj.transform.rotation = Quaternion.Slerp(playerLookObj.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        switch (csvData.enemiesData[dataNumber].moveType)
        {
            case StartUpEnemyData.MoveType.dontMove:
                break;
            case StartUpEnemyData.MoveType.custom:
                CustomMove();
                break;
            case StartUpEnemyData.MoveType.vertical:
                VerticalMove();
                break;
            case StartUpEnemyData.MoveType.horizontal:
                HorizontalMove();
                break;
        }
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
        rotateObj.transform.Rotate(0, 0, rotatePerSpeed * Time.deltaTime);
        if (time >= rotateTime)
        {
            rotateObj.transform.localEulerAngles = angles;
            Debug.Log(rotateObj.name + "/END:" + rotateObj.transform.localEulerAngles.z);
            return true;
        }
        return false;
    }

    //被弾
    void OnTriggerEnter(Collider collider)
    {
        if (isSetUp == false)
        {
            Debug.Log("セットアップが完了していません");
            return;
        }
        
        if (collider.gameObject.tag == "Bullet")
        {
            Debug.Log("当たった");
            Bullet bullet = collider.gameObject.GetComponent<Bullet>();
            TakeDamage(bullet.Damage);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        hp -= damage;
        damageEffectPrefab.GetComponent<VisualEffect>().SendEvent("OnPlay");
                
        if (enemyHPSlider != null)
        {
            enemyHPSlider.value = hp;
        }
        damageText.ShowDamage(damage);
        audioSource.PlayOneShot(DamageSound);
        
        if (hp <= 0)
        {
            if (csvData.enemiesData[dataNumber].enemyAttackType != StartUpEnemyData.EnemyAttackType.dontAttack)
            {
                StopAttack();
            }

            // 敵死亡イベントをログ
            string enemyTypeStr = csvData.enemiesData[dataNumber].enemyType.ToString();
            LudiscanManager.Instance.LogEnemyDeath(
                enemyId: gameObject.name,
                enemyType: enemyTypeStr,
                position: transform.position
            );

            DeathProcess();

            Debug.Log("Dead:" + csvData.enemiesData[dataNumber].enemyType);
            switch (csvData.enemiesData[dataNumber].enemyType)
            {
                case StartUpEnemyData.EnemyType.normal:
                    enemySpawnManager.RemoveEnemy(this.gameObject,deathEffectPrefab);
                    break;
                case StartUpEnemyData.EnemyType.shield:
                    enemySpawnManager.RemoveEnemy(this.gameObject.transform.parent.gameObject,deathEffectPrefab);
                    break;
                case StartUpEnemyData.EnemyType.boss:
                    enemySpawnManager.RemoveEnemy(this.gameObject.transform.parent.gameObject,deathEffectPrefab);
                    break;
                case StartUpEnemyData.EnemyType.humanoid:
                    enemySpawnManager.RemoveEnemy(this.gameObject, deathEffectPrefab);
                    break;
            }
        }
    }
    
    //攻撃
    public virtual void EnemyAttack()
    {
        //Debug.Log("Attack");
      //  beforeAttackText.After();
        GameObject bullets = Instantiate(bulletObj, shotObj.transform.position, Quaternion.identity);

        Bullet reflectionBullet = bullets.GetComponent<Bullet>();
        switch (csvData.enemiesData[dataNumber].enemyAttackType)
        {
            case StartUpEnemyData.EnemyAttackType.playerAim:
                reflectionBullet.SetPowerEnemy(transform.position);
                break;
            case StartUpEnemyData.EnemyAttackType.straight:
                reflectionBullet.SetStraightPowerEnemy(straightObj.transform.rotation.eulerAngles,false);
                break;
        }

        shotBombEffect.GetComponent<VisualEffect>().SendEvent("OnPlay");
        PlayAttckSound();
        StartAttack();
    }
    
    public virtual void BeforeAttack()
    {
        if (csvData.enemiesData[dataNumber].enemyAttackType == StartUpEnemyData.EnemyAttackType.dontAttack) { return; }
        if (attackEffect != null)
        {
            attackEffect.GetComponent<VisualEffect>().SendEvent("OnPlay");
        }
     //   beforeAttackText.Warning(csvData.enemiesData[dataNumber].brinkDuration);
    }

    public void StopAttack()
    {
        CancelInvoke();
     //   beforeAttackText.After();
    }

    public void StartAttack()
    {
        //Debug.Log("AttackStart");
        Invoke("BeforeAttack", bulletRate - csvData.enemiesData[dataNumber].beforeAttackTime);
        Invoke("EnemyAttack", bulletRate);
    }

    public void VerticalMove()
    {
        if (isCoolTime == true)
        {
            float diff = Time.time - t;
            if (diff < csvData.enemiesData[dataNumber].waitTime)
            {
                //Debug.Log("クールタイム中");
                return;
            }
            else
            {
                ResetStartTime();
                isCoolTime = false;
            }
        }
        
        switch (moveCounter)
        {
            case 0:
                if (EnemyMove(moveObj, startPos, upCenterPos, csvData.enemiesData[dataNumber].verticalTime, t) == true)
                {
                    NextMove();
                }
                break;
            case 1:
                if (EnemyMove(moveObj, upCenterPos, downCenterPos, csvData.enemiesData[dataNumber].verticalTime * 2, t) == true)
                {
                    NextMove();
                }
                break;
            case 2:
                if (EnemyMove(moveObj, downCenterPos, upCenterPos, csvData.enemiesData[dataNumber].verticalTime * 2, t) == true)
                {
                    NextMove();
                }
                break;
        }
    }

    public void HorizontalMove()
    {
        if (isCoolTime == true)
        {
            float diff = Time.time - t;
            if (diff < csvData.enemiesData[dataNumber].waitTime)
            {
                //Debug.Log("クールタイム中");
                return;
            }
            else
            {
                ResetStartTime();
                isCoolTime = false;
            }
        }
        
        switch (moveCounter)
        {
            case 0:
                if (EnemyMove(moveObj, startPos, rightCenterPos, csvData.enemiesData[dataNumber].horizontalTime, t) == true)
                {
                    NextMove();
                }
                break;
            case 1:
                if (EnemyMove(moveObj, rightCenterPos, leftCenterPos, csvData.enemiesData[dataNumber].horizontalTime * 2, t) == true)
                {
                    NextMove();
                }
                break;
            case 2:
                if (EnemyMove(moveObj, leftCenterPos, rightCenterPos, csvData.enemiesData[dataNumber].horizontalTime * 2, t) == true)
                {
                    NextMove();
                }
                break;
        }
    }

    void ResetStartTime()
    {
        t = Time.time;
    }

    void NextMove()
    {
        isCoolTime = true;
        moveCounter++;
        ResetStartTime();
        if (moveCounter > 2)
        {
            moveCounter = 1;
        }
    }
    
    public IEnumerator SetNumber(int enemyID)
    {
        //Debug.Log("待機中");
        yield return new WaitUntil(() => StartUpEnemyData.IsInitialized == true);
        //Debug.Log("待機終了");
        
        dataNumber = -1;
        for (int i = 1; i < csvData.enemiesData.Count; i++)
        {
            if (enemyID == csvData.enemiesData[i].id)
            {
                dataNumber = i;
            }
        }

        if (dataNumber == -1)
        {
            //idが存在しない場合
            Debug.Log("IDが存在しません");
            yield break;
        }

        hp = csvData.enemiesData[dataNumber].maxHP;
        ChangeBulletRate(csvData.enemiesData[dataNumber].bulletRate);
        
        //攻撃
        switch (csvData.enemiesData[dataNumber].enemyAttackType)
        {
            case StartUpEnemyData.EnemyAttackType.dontAttack:
                break;
            case StartUpEnemyData.EnemyAttackType.playerAim:
                StartAttack();
                break;
            case StartUpEnemyData.EnemyAttackType.straight:
                StartAttack();
                break;
        }

        switch (csvData.enemiesData[dataNumber].enemyType)
        {
            case StartUpEnemyData.EnemyType.boss:
            case StartUpEnemyData.EnemyType.humanoid:
                enemyHPSlider = GameObject.FindWithTag("EnemyHPBar").GetComponent<Slider>();
                enemyHPSlider.maxValue = hp;
                enemyHPSlider.value = hp;
                break;
        }

        // 敵スポーンイベントをログ
        string enemyTypeStr = csvData.enemiesData[dataNumber].enemyType.ToString();
        LudiscanManager.Instance.LogEnemySpawn(
            enemyId: gameObject.name,
            enemyType: enemyTypeStr,
            position: transform.position
        );
        
        Invoke("SetUp", 0.05f);
    }
    public virtual void SetUp()
    {
        //overrideしない場合の移動に使う変数を代入
        startPos = transform.position;
        rightPosX = centerPos.x + csvData.enemiesData[dataNumber].rightRenge;
        leftPosX = centerPos.x - csvData.enemiesData[dataNumber].leftRenge;
        upPosY = centerPos.y + csvData.enemiesData[dataNumber].upRenge;
        downPosY = centerPos.y - csvData.enemiesData[dataNumber].downRenge;
        rightCenterPos = new Vector3(rightPosX, centerPos.y, centerPos.z);
        leftCenterPos = new Vector3(leftPosX, centerPos.y, centerPos.z);
        upCenterPos = new Vector3(centerPos.x, upPosY, centerPos.z);
        downCenterPos = new Vector3(centerPos.x, downPosY, centerPos.z);

        isCoolTime = false;
        moveCounter = 0;
        ResetStartTime();

        isSetUp = true;
        //Debug.Log("セットアップ完了");
    }
    public virtual void Change() {}
    public virtual void CustomMove() {}
    public virtual void DeathProcess(){}

    public void ChangeBulletRate(float rate)
    {
        bulletRate = rate;
    }

    public void PlayAttckSound()
    {
        audioSource.PlayOneShot(EnemyShotSound);
    }
    public void PlayEffect(int effectIndex)
    {
        if (effectPrefabs == null || effectIndex < 0 || effectIndex >= effectPrefabs.Length) return;
        
        GameObject effectToPlay = effectPrefabs[effectIndex];
        if (effectToPlay != null)
        {
            VisualEffect vfx = effectToPlay.GetComponent<VisualEffect>();
            if (vfx != null)
            {
                vfx.SendEvent("OnPlay");
            }
        }
    }
}
