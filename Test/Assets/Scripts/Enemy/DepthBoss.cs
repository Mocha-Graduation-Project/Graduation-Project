using UnityEngine;

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
    
    [JapaneseLabel("初期(中央)位置")] private Vector3 centerPos;

    [SerializeField] [JapaneseLabel("手前側のz座標")] private float flontZPos;
    
    [Space(15)]
    [SerializeField] [JapaneseLabel("パターン移行のクールタイム")] float coolTime;
    
    [Space(5)]
    [Header("パターン1,2")] 
    [SerializeField] [JapaneseLabel("パターン1,2に使うデータ")]
    private PatrolEnemyData enemyData;
    
    public GameObject Boss{get{ return boss; }}
    public Vector3 CenterPos{get{ return centerPos; }}
    public float CenterZPos{get{ return flontZPos; }}
    public float CoolTime{get{ return coolTime; }}
    public PatrolEnemyData EnemyData { get { return enemyData; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateMachine = new StateMachine();
        RandomSetPattern();
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    public void Change()
    {
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
}
