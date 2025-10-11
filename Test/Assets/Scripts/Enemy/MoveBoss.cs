using Scripts;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    [SerializeField] private Scripts.Enemy enemyScript;

    [JapaneseLabel("初期")] private Vector3 basePos;
    
    [JapaneseLabel("中央")] private Vector3 centerPos;

    [Space(15)] 
    [SerializeField] [JapaneseLabel("パターン開始位置までの\n移動にかかる時間")][Space(5)]  private float moveTime;
    
    [Space(15)]
    [SerializeField] [JapaneseLabel("パターン移行のクールタイム")] float coolTime;

    [Space(5)]
    [Header("パターン1,2,3,4")] 
    [SerializeField] [JapaneseLabel("パターン1～4に使うデータ")]
    private PatrolEnemyData enemyData;

    [SerializeField] [JapaneseLabel("1,2の発射レート")]
    private float bulletRate1_2;
    
    [SerializeField] [JapaneseLabel("3,4の発射レート")]
    private float bulletRate3_4;

    [Space(5)]
    [Header("パターン5")] 
    [SerializeField] [JapaneseLabel("レーザー")] private GameObject[] laser;
    [SerializeField] [JapaneseLabel("90度回転するのにかかる秒数")] private float rotate90PerSec;
    [SerializeField] [JapaneseLabel("パターン5発動のHPの割合(%)")][Space(5)] 
    private int changeHPPercent;
    [SerializeField] [JapaneseLabel("パターン5を行うようになるHP")][Space(5)] 
    private float changeHP;
    [SerializeField] private bool patten5Flag;
    [JapaneseLabel("パターン1～4を行った回数")]
    private int actioncounter;
    [JapaneseLabel("パターンの総数")]
    private int maxAction;

    [SerializeField] [JapaneseLabel("ループ回数")]
    private int loopCounter;
    [SerializeField] [JapaneseLabel("ループさせる最大回数")]
    private int maxLoop;

    private Vector3 rightCenterPos;
    private Vector3 leftCenterPos;
    private Vector3 upCenterPos;
    private Vector3 downCenterPos;
    
    public Vector3 CenterPos{get{ return centerPos; }}
    public GameObject RotateAxis{get{ return rotateAxis; }}
    public GameObject Boss{get{ return boss; }}
    public float MoveTime{get{ return moveTime; }}
    public float CoolTime{get{ return coolTime; }}
    
    public Scripts.Enemy EnemyScript { get { return enemyScript; } }

    public PatrolEnemyData EnemyData { get { return enemyData; } }
    
    public GameObject[]  Laser { get { return laser; } }
    public float Rotate90PerSec { get { return rotate90PerSec; } }
    
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

        actioncounter = 0;
        loopCounter = 0;
        maxAction = 4;

        changeHP = enemyScript.HP * (changeHPPercent * 0.01f);
        //enemyScript.StopAttck();
        Debug.Log("ChangeHP:" + changeHP);
        patten5Flag = false;
        
        stateMachine=new StateMachine();
        RandomSetPattern();
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    public void Change()
    {
        Debug.Log("パターン変更");
        if (CheckFlag5() == true)
        {
            return;
        }
        
        switch (pattern)
        {
            case Patterns.none:
                break;
            case Patterns.pattern1:
                ChangePattern(Patterns.pattern3);
                break;
            case Patterns.pattern2:
                ChangePattern(Patterns.pattern4);
                break;
            case Patterns.pattern3:
                ChangePattern(Patterns.pattern2);
                break;
            case Patterns.pattern4:
                ChangePattern(Patterns.pattern1);
                break;
            case Patterns.pattern5:
                RandomSetPattern();
                break;
        }
        //Debug.Log("パターン"+pattern);
    }

    void ChangePattern(Patterns nextPattern)
    {
        switch (nextPattern)
        {
            case Patterns.none:
                break;
            case Patterns.pattern1:
                enemyScript.BulletRate = bulletRate1_2;
                stateMachine.ChangeState(new RightVerticalMove(this));
                break;
            case Patterns.pattern2:
                enemyScript.BulletRate = bulletRate1_2;
                stateMachine.ChangeState(new LeftVerticalMove(this));
                break;
            case Patterns.pattern3:
                enemyScript.BulletRate = bulletRate3_4;
                stateMachine.ChangeState(new UpHorizontalMove(this));
                break;
            case Patterns.pattern4:
                enemyScript.BulletRate = bulletRate3_4;
                stateMachine.ChangeState(new DownHorizontalMove(this));
                break;
            case Patterns.pattern5:
                stateMachine.ChangeState(new LaserAttck(this));
                break;
        }
        pattern = nextPattern;
    }

    void RandomSetPattern()
    {
        int random = Random.Range(0, 2);
        Debug.Log("random:" + random);
        switch (random)
        {
            case 0:
                ChangePattern(Patterns.pattern1);
                break;
            case 1:
                ChangePattern(Patterns.pattern2);
                break;
            default:
                Debug.Log("error:" + random);
                break;
        }
    }

    bool CheckFlag5()
    {
        if (patten5Flag == true)
        {
            actioncounter++;
            if (actioncounter == maxAction)
            {
                actioncounter = 0;
                loopCounter++;
                if (loopCounter == maxLoop)
                {
                    loopCounter = 0;
                    ChangePattern(Patterns.pattern5);
                    return true;
                }
            }
        }
        else
        {
            if (enemyScript.HP <= changeHP)
            {
                patten5Flag = true;
                ChangePattern(Patterns.pattern5);
                return true;
            }
        }
        return false;
    }

    public void InvertActiveLazer()
    {
        foreach (GameObject obj in laser)
        {
            if (obj.activeInHierarchy == true)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }

    public void LazerOn()
    {
        foreach (GameObject obj in laser)
        {
            if (obj.transform.parent.gameObject.activeInHierarchy == true)
            {
                obj.SetActive(true);
            }
        }
    }

    public void LazerOff()
    {
        foreach (GameObject obj in laser)
        {
            obj.SetActive(false);
        }
    }
}
