using UnityEngine;

public class MoveBoss : EnemyAI
{
    public enum Patterns
    {
        none,
        pattern1,
        pattern2,
        pattern3,
        pattern4,
        pattern5,
    }

    [SerializeField] private StateMachine stateMachine;

    [Header("共通")]
    [SerializeField] [JapaneseLabel("現在の行動パターン")]Patterns pattern;

    [SerializeField] [JapaneseLabel("回転軸")] private GameObject rotateAxis;

    [SerializeField] [JapaneseLabel("動かすオブジェクト")] private GameObject boss;

    [SerializeField] [JapaneseLabel("初期")] private Vector3 basePos;
    
    [Space(15)]
    [SerializeField] [JapaneseLabel("中央")] private Vector3 centerPos;

    [Space(15)] 
    [SerializeField] [JapaneseLabel("移動にかかる時間")] private float moveTime;

    [Space(5)]
    [Header("パターン1,2,3,4")] 
    [SerializeField] [JapaneseLabel("パターン1～4の挙動")]private EnemyPatrol enemyPatrol;

    [SerializeField] [JapaneseLabel("パターン1～4に使うデータ")]
    private PatrolEnemyData enemyData;

    private Vector3 rightCenterPos;
    private Vector3 leftCenterPos;
    private Vector3 upCenterPos;
    private Vector3 downCenterPos;
    
    public Vector3 CenterPos{get{ return centerPos; }}
    public GameObject RotateAxis{get{ return rotateAxis; }}
    public GameObject Boss{get{ return boss; }}
    public float MoveTime{get{ return moveTime; }}
    
    public Vector3 RightCenterPos{get{ return rightCenterPos; }}
    public Vector3 LeftCenterPos{get{ return leftCenterPos; }}
    public Vector3 UpCenterPos{get{ return upCenterPos; }}
    public Vector3 DownCenterPos{get{ return downCenterPos; }}
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        basePos = transform.position;
        centerPos.x = (enemyData.RightRange - enemyData.LeftRange) / 2;
        centerPos.y = (enemyData.UpRange - enemyData.DownRange) / 2;
        
        rightCenterPos.x = centerPos.x + enemyData.RightRange;
        rightCenterPos.y = centerPos.y;
        leftCenterPos.x = centerPos.x - enemyData.LeftRange;
        leftCenterPos.y = centerPos.y;
        upCenterPos.x = centerPos.x;
        upCenterPos.y = centerPos.y + enemyData.UpRange;
        downCenterPos.x = centerPos.x;
        downCenterPos.y = centerPos.y - enemyData.DownRange;
        
        Debug.Log(basePos + "/" + centerPos);
        Debug.Log(rightCenterPos);
        
        stateMachine=new StateMachine();
        stateMachine.ChangeState(new RightVerticalMove(this));
        pattern = Patterns.pattern1;
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    public void Change()
    {
        Debug.Log("パターン変更");
        switch (pattern)
        {
            case Patterns.none:
                break;
            case Patterns.pattern1:
                stateMachine.ChangeState(new LeftVerticalMove(this));
                pattern = Patterns.pattern3;
                break;
            case Patterns.pattern2:
                break;
            case Patterns.pattern3:
                stateMachine.ChangeState(new RightVerticalMove(this));
                pattern = Patterns.pattern1;
                break;
            case Patterns.pattern4:
                break;
        }
        Debug.Log("パターン"+pattern);
    }

    void ChangePattern(Patterns nextPattern)
    {
        pattern = nextPattern;
    }
}
