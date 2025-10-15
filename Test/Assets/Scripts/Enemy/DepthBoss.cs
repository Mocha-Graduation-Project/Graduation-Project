using System;
using Player;
using Scripts;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEditor;
using UnityEngine.Serialization;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyAI))]
#endif

public class DepthBoss : EnemyAI
{
    public enum Patterns
    {
        none,
        rightTackle,
        leftTackle,
        fallingAttack,
    }

    [SerializeField] private StateMachine stateMachine;

    [Header("共通")]
    [SerializeField] [JapaneseLabel("現在の行動パターン")]Patterns pattern;
    
    [SerializeField] [JapaneseLabel("動かすオブジェクト")] private GameObject boss;

    [SerializeField] [JapaneseLabel("攻撃中か")] private bool isAttck;
    
    [JapaneseLabel("初期(中央)位置")] private Vector3 centerPos;

    [SerializeField] [JapaneseLabel("手前側のz座標")] private float flontZPos;
    
    [Space(15)]
    [SerializeField] [JapaneseLabel("パターン移行のクールタイム")] float coolTime;
    
    [SerializeField] [JapaneseLabel("攻撃表示のUI")]　AttckWarningUI attckWarningUI;
    
    [Space(5)]
    [Header("パターン1,2")] 
    [SerializeField] [JapaneseLabel("パターン1,2に使うデータ")]
    private PatrolEnemyData patrolEnemyData;

    [Space(5)]
    [Header("パターン3")] 
    [SerializeField] [JapaneseLabel("左右タックルの最大ループ数")] private int maxLRTackle;

    [SerializeField] [JapaneseLabel("左右タックルのループ回数")] private int LRTackleCounter;

    [SerializeField] [JapaneseLabel("タックルの回数")] private int tackleCounter;

    [SerializeField] [JapaneseLabel("落下後の待機時間")] private float fallAttckWaitTime;
    
    [JapaneseLabel("落下する座標")] private Vector3 fallingAttckPos;
    
    GameObject player;
    
    public GameObject Boss{get{ return boss; }}
    public Vector3 CenterPos{get{ return centerPos; }}
    public float FlontZPos{get{ return flontZPos; }}
    public float CoolTime{get{ return coolTime; }}
    public PatrolEnemyData PatrolEnemyData { get { return patrolEnemyData; } }
    public float FallAttckWaitTime { get { return fallAttckWaitTime; } }
    public GameObject Player{ get{ return player; }}
    public AttckWarningUI AttckWarningUI{ get{ return attckWarningUI; } }
    public Vector3 FallingAttckPos{ get{ return fallingAttckPos; } set { fallingAttckPos = value; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("テスト:" + enemyData.enemyType);
        TestShow();
        centerPos = this.transform.position;
        stateMachine = new StateMachine();
        RandomSetPattern();
        LRTackleCounter = 0;
        tackleCounter = 0;
        isAttck = true;
        player = GameObject.FindGameObjectWithTag("Player");
        attckWarningUI = GameObject.FindGameObjectWithTag("WarningUI").GetComponent<AttckWarningUI>();
        attckWarningUI.SetFallingAttckEnemy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    public void Change()
    {
        //左右タックルを一定回数繰り返したらパターン3へ移行する
        if (pattern != Patterns.fallingAttack && pattern != Patterns.none)
        {
            tackleCounter++;
            if (tackleCounter != 0 && tackleCounter % 2 == 0)
            {
                LRTackleCounter++;
                if (LRTackleCounter == maxLRTackle)
                {
                    ChangePattern(Patterns.fallingAttack);
                    LRTackleCounter = 0;
                    tackleCounter = 0;
                    return;
                }
            }
        }
        
        switch (pattern)
        {
            case Patterns.none:
                break;
            case Patterns.rightTackle:
                ChangePattern(Patterns.leftTackle);
                break;
            case Patterns.leftTackle:
                ChangePattern(Patterns.rightTackle);
                break;
            case Patterns.fallingAttack:
                RandomSetPattern();
                break;
        }
    }

    void ChangePattern(Patterns nextPattern)
    {
        pattern = nextPattern;
        switch (nextPattern)
        {
            case Patterns.none:
                break;
            case Patterns.rightTackle:
                stateMachine.ChangeState(new RightTackle(this));
                break;
            case Patterns.leftTackle:
                stateMachine.ChangeState(new LeftTackle(this));
                break;
            case Patterns.fallingAttack:
                stateMachine.ChangeState(new FallingAttack(this));
                break;
        }
    }

    void RandomSetPattern()
    {
        int random = Random.Range(0, 2);
        Debug.Log("random:" + random);
        switch (random)
        {
            case 0:
                ChangePattern(Patterns.rightTackle);
                break;
            case 1:
                ChangePattern(Patterns.leftTackle);
                break;
            default:
                Debug.Log("error:" + random);
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isAttck == true && collision.gameObject.tag == "Player")
        {
            PlayerStatus playerStatus = collision.gameObject.GetComponentInChildren<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.Damage(1);
            }
        }
    }

    public void AttckTrue()
    {
        isAttck = true;
    }

    public void AttckFalse()
    {
        isAttck = false;
    }
}
