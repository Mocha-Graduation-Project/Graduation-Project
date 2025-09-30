using System;
using Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    [Space(5)]
    [Header("パターン1,2")] 
    [SerializeField] [JapaneseLabel("パターン1,2に使うデータ")]
    private PatrolEnemyData enemyData;

    [Space(5)]
    [Header("パターン3")] 
    [SerializeField] [JapaneseLabel("左右タックルを繰り返す回数")] private int maxLRTackle;

    [SerializeField] [JapaneseLabel("左右タックルのループ回数")] private int LRTackleCounter;
    [JapaneseLabel("タックルの回数")] private int tackleCounter;
    
    public GameObject Boss{get{ return boss; }}
    public bool IsAttck{get{ return isAttck; }}
    public Vector3 CenterPos{get{ return centerPos; }}
    public float FlontZPos{get{ return flontZPos; }}
    public float CoolTime{get{ return coolTime; }}
    public PatrolEnemyData EnemyData { get { return enemyData; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        centerPos = this.transform.position;
        stateMachine = new StateMachine();
        RandomSetPattern();
        LRTackleCounter = 0;
        tackleCounter = 0;
        isAttck = false;
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    public void Change()
    {
        //ここに左右タックルを一定回数繰り返したらパターン3へ移行する処理を追加する
        
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
        if (collision.gameObject.tag == "Player")
        {
            PlayerStatus playerStatus = collision.gameObject.GetComponentInChildren<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.Damage(1);
            }
        }
    }
}
