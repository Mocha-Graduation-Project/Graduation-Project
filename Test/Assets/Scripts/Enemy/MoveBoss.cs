using NUnit.Framework.Internal;
using Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(EnemyAI))]
#endif

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
    
    [Space(5)]
    [Header("パターン1,2,3,4")] 
    [SerializeField] [JapaneseLabel("パターン1～4に使うデータ")]
    private PatrolEnemyData patrolEnemyData;

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
    [JapaneseLabel("パターン5を行うようになるHP")][Space(5)] 
    private float changeHP;
    private bool patten5Flag;
    [JapaneseLabel("パターン1～4を行った回数")]
    private int actioncounter;
    [JapaneseLabel("パターンの総数")]
    private int maxAction;

    [SerializeField] [JapaneseLabel("ループ回数")]
    private int loopCounter;
    [SerializeField] [JapaneseLabel("ループさせる最大回数")]
    private int maxLoop;
    
    public GameObject[]  Laser { get { return laser; } }
    public float Rotate90PerSec { get { return rotate90PerSec; } }

    public override void SetUp()
    {
        actioncounter = 0;
        loopCounter = 0;
        maxAction = 4;
        changeHP = enemyData.maxHP * (changeHPPercent * 0.01f);
        //Debug.Log("ChangeHP:" + changeHP);
        patten5Flag = false;
        stateMachine=new StateMachine();
        RandomSetPattern();
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        //stateMachine.Update();
    }

    public override void CustomMove()
    {
        stateMachine.Update();
    }

    public override void Change()
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
                enemyData.bulletRate = bulletRate1_2;
                Debug.Log(enemyData.bulletRate);
                stateMachine.ChangeState(new RightVerticalMove(this));
                break;
            case Patterns.pattern2:
                enemyData.bulletRate = bulletRate1_2;
                Debug.Log(enemyData.bulletRate);
                stateMachine.ChangeState(new LeftVerticalMove(this));
                break;
            case Patterns.pattern3:
                enemyData.bulletRate = bulletRate3_4;
                stateMachine.ChangeState(new UpHorizontalMove(this));
                break;
            case Patterns.pattern4:
                enemyData.bulletRate = bulletRate3_4;
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
            if (hp <= changeHP)
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
