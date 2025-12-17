using System;
using Player;
using Scripts;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEditor;
using UnityEngine.Serialization;

// #if UNITY_EDITOR
// [CustomEditor(typeof(EnemyAI))]
// #endif

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
    
    private Animator animator;

    [Header("共通")]
    [SerializeField] [JapaneseLabel("現在の行動パターン")]Patterns pattern;

    [SerializeField] [JapaneseLabel("攻撃中か")] private bool isAttack;
    
    [JapaneseLabel("DepthBossDataの値")] private int depthBossData = 0;

    [JapaneseLabel("左側に行く基準の座標")] private float left33Pos;
    
    [JapaneseLabel("右側に行く基準の座標")] private float right33Pos;

    [JapaneseLabel("左右タックルのループ回数")] private int LRTackleCounter;

    [JapaneseLabel("タックルの回数")] private int tackleCounter;
    
    public bool IsAttack { get { return isAttack; } }

    public override void SetUp()
    {
        animator = GetComponent<Animator>();
        stateMachine = new StateMachine();
        Invoke("RandomSetPattern", ExcelData.Enemy[DataNumber].coolTime);
        LRTackleCounter = 0;
        tackleCounter = 0;
        isAttack = true;
        //player = GameObject.FindGameObjectWithTag("Player");

        GameObject loopAreaObj = GameObject.FindWithTag("LoopArea");
        if (loopAreaObj != null)
        {
            float scale = loopAreaObj.transform.localScale.x;
            float leftPos = loopAreaObj.transform.position.x - scale * 0.5f;
            float rightPos = loopAreaObj.transform.position.x + scale * 0.5f;

            left33Pos = leftPos + scale * 0.33f;
            right33Pos = rightPos - scale * 0.33f;
        }

        IsSetUp = true;
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        //stateMachine.Update();
    }

    public override void CustomMove()
    {
        //stateMachine.Update();
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
                if (LRTackleCounter == ExcelData.DepthBoss[depthBossData].maxLRTackle)
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
                //stateMachine.ChangeState(new RightTackle(this));
                animator.SetTrigger("RightTackleTrigger");
                break;
            case Patterns.leftTackle:
                //stateMachine.ChangeState(new LeftTackle(this));
                animator.SetTrigger("LeftTackleTrigger");
                break;
            case Patterns.fallingAttack:
                //stateMachine.ChangeState(new FallingAttack(this));
                animator.SetTrigger("FallingAttckTrigger");
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

    public void AttackTrue()
    {
        isAttack = true;
    }

    public void AttackFalse()
    {
        Debug.Log("AttackFalse");
        isAttack = false;
    }

    public void AnimationEnd()
    {
        //アニメーションが終わって呼ばれたらクールタイム後に次の行動へ
        AttackTrue();
        Invoke("Change", ExcelData.Enemy[DataNumber].coolTime);
    }

    public void SwitchFallingAttack()
    {
        //プレイヤーの位置によって再生するアニメーション切り替え
        float pos = player.transform.position.x;
        if (pos <= left33Pos)
        {
            Debug.Log("LeftAttck");
            animator.SetTrigger("LeftFallingAttck");
        }
        else if (pos >= right33Pos)
        {
            Debug.Log("RightAttck");
            animator.SetTrigger("RightFallingAttck");
        }
        else
        {
            Debug.Log("CenterAttck");
            animator.SetTrigger("CenterFallingAttck");
        }
    }

    public void DepthBossDeath()
    {
        animator.SetTrigger("Death");
    }
}
