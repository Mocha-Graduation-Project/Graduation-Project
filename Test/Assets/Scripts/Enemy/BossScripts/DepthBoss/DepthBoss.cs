using System;
using Player;
using Scripts;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEditor;
using UnityEngine.Serialization;



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

    [SerializeField] [JapaneseLabel("手前側のz座標")] private float flontZPos;
    
    [Space(15)]
    [SerializeField] [JapaneseLabel("パターン移行のクールタイム")] float coolTime;
    
    [SerializeField] [JapaneseLabel("攻撃表示のUI")]　AttckWarningUI attckWarningUI;

    [Space(5)]
    [Header("パターン3")] 
    [SerializeField] [JapaneseLabel("左右タックルの最大ループ数")] private int maxLRTackle;

    [JapaneseLabel("左側に行く基準の座標")] private float left33Pos;
    
    [JapaneseLabel("右側に行く基準の座標")] private float right33Pos;

    [JapaneseLabel("左右タックルのループ回数")] private int LRTackleCounter;

    [JapaneseLabel("タックルの回数")] private int tackleCounter;

    [SerializeField] [JapaneseLabel("落下後の待機時間")] private float fallAttckWaitTime;
    
    [JapaneseLabel("落下する座標")] private Vector3 fallingAttckPos;

    public float FlontZPos{get{ return flontZPos; }}
    public float FallAttckWaitTime { get { return fallAttckWaitTime; } }
    public AttckWarningUI AttckWarningUI{ get{ return attckWarningUI; } }
    public Vector3 FallingAttckPos{ get{ return fallingAttckPos; } set { fallingAttckPos = value; } }
    public bool IsAttack { get { return isAttack; } }

    public override void SetUp()
    {
        animator = GetComponent<Animator>();
        stateMachine = new StateMachine();
        Invoke("RandomSetPattern", enemyData.coolTime);
        LRTackleCounter = 0;
        tackleCounter = 0;
        isAttack = true;
        if (attckWarningUI != null)
        {
            attckWarningUI.SetFallingAttckEnemy(this.gameObject);
        }
        else
        {
             Debug.LogWarning("AttckWarningUI is not assigned.", this);
        }
        
        if (loopAreaCollider != null)
        {
            float scale = loopAreaCollider.transform.localScale.x;
            float leftPos = loopAreaCollider.transform.position.x - scale * 0.5f;
            float rightPos = loopAreaCollider.transform.position.x + scale * 0.5f;

            left33Pos = leftPos + scale * 0.33f;
            right33Pos = rightPos - scale * 0.33f;
        }
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
        Invoke("Change", enemyData.coolTime);
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
