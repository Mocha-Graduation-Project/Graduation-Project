using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Player;
using Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
using Random = UnityEngine.Random;



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
    
    [Serializable]
    public class BeeHead
    {
        [JapaneseLabel("発射場所")] public GameObject shotPos;
        [JapaneseLabel("盾")] public GameObject beeShield;
        [JapaneseLabel("何度に回転するか")] public float rotatez;
    }

    [SerializeField] private StateMachine stateMachine;

    [Header("共通")]
    [SerializeField] [JapaneseLabel("現在の行動パターン")]Patterns pattern;

    [Space(5)] [Header("パターン1,2,3,4")]
    
    [Header("回転蜂")] public List<BeeHead> beeHeads = new();
    
    [JapaneseLabel("回転する蜂の頭")] private GameObject rotateBeeHead;
    private GameObject shotObj;
    private float rotateZ;
    private float rotateSpeed;
    private float startTime;
    private bool isactive;
    private bool isBackRotate;
    [JapaneseLabel("攻撃時の回転")] private bool finishRotation;

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
    private float zeroHp = 0;
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
    
    [SerializeField] private Animator animator;

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
        rotateZ = 0;
        rotateSpeed = 180f;
        startTime = 0;
        isactive = false;
        isBackRotate = false;
        finishRotation = false;
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        //stateMachine.Update();
        //蜂の頭を射出方向へ回転
        if (isactive == true && finishRotation != true && isBackRotate == false)
        {
            finishRotation = EnemyRotate(rotateBeeHead, new Vector3(0, 0, rotateZ), rotateSpeed, 0.5f, ref startTime);
        }

        if (isBackRotate == true && finishRotation != true)
        {
            if (EnemyRotate(rotateBeeHead, Vector3.zero, (-1) * rotateSpeed, 0.5f,
                    ref startTime) == true)
            {
                finishRotation = false;
                isactive = false;
                isBackRotate = false;
            }

        }
    }

    public override void CustomMove()
    {
        stateMachine.Update();
    }

    public override void BeforeAttack()
    {
        base.BeforeAttack();
        //どちらのShotPosから出すか判定
        shotObj = null;
        isactive = false;
        for (int i = 0; i < beeHeads.Count; i++)
        {
            //Debug.Log(beeShields[i].name + ":" + beeShields[i].activeSelf);
            if (beeHeads[i].beeShield.activeSelf == true)
            {
                if (isactive == false)
                {
                    isactive = true;
                    SetBeeShot(beeHeads[i]);
                }
                else
                {
                    switch (pattern)
                    {
                        case Patterns.pattern1:
                            if (shotObj.transform.position.x > beeHeads[i].shotPos.transform.position.x)
                            {
                                SetBeeShot(beeHeads[i]);
                            }
                            break;
                        case Patterns.pattern2:
                            if (shotObj.transform.position.x < beeHeads[i].shotPos.transform.position.x)
                            {
                                SetBeeShot(beeHeads[i]);
                            }
                            break;
                        case Patterns.pattern3:
                        case Patterns.pattern4:
                            if (shotObj.transform.position.y > beeHeads[i].shotPos.transform.position.y)
                            {
                                SetBeeShot(beeHeads[i]);
                            }
                            break;
                    }
                }
            }
        }
        if (isactive == true)
        {
            ResetTimer();
        }
    }

    void SetBeeShot(BeeHead setBeeHead)
    {
        shotObj = setBeeHead.shotPos;
        rotateBeeHead = setBeeHead.shotPos.transform.parent.gameObject;
        rotateZ = setBeeHead.rotatez;
        if (rotateZ < 0)
        {
            //rotateSpeed = -180f;
            //上のコメントアウトは、後々プレイヤー側のバリアが破壊されたらそちらに回転した時用の処理です
            rotateZ = 90;
            rotateSpeed = 180f;
        }
        else
        {
            rotateSpeed = 180f;
        }

        //パターン3,4が発射レートが高い為、攻撃までに回転を終える為の調整
        if (pattern == Patterns.pattern3 || pattern == Patterns.pattern4)
        {
            rotateSpeed *= 2f;
        }
    }

    public override void EnemyAttack()
    {
        if (isactive == true)
        {
            beforeAttackText.After();

            // for (int i = 0; i < beeHeads.Count; i++)
            // {
            //     Debug.Log("Shot[" + beeHeads[i].shotPos.name + "]/" + beeHeads[i].shotPos.transform.position);
            // }
            // Debug.Log("Attck:" + shotPos);
            GameObject bullets = Instantiate(enemyData.bulletObj, shotObj.transform.position, Quaternion.identity);

            Bullet reflectionBullet = bullets.GetComponent<Bullet>();
            
            reflectionBullet.SetStraightPowerEnemy(straightObj.transform.rotation.eulerAngles);
            
            PlayAttckSound();
            //蜂頭を元の方向に戻す
            isBackRotate = true;
            finishRotation = false;
            ResetTimer();
        }
        
        StartAttack();
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
        
        //戻っていなければ蜂頭を元の方向に戻す
        if (isBackRotate == false && shotObj != null)
        {
            isBackRotate = true;
            finishRotation = false;
            ResetTimer();
        }
    }

    private void ChangePattern(Patterns nextPattern)
    {
        switch (nextPattern)
        {
            case Patterns.none:
                break;
            case Patterns.pattern1:
                enemyData.bulletRate = bulletRate1_2;
                Debug.Log(enemyData.bulletRate);
                stateMachine.ChangeState(new RightVerticalMove(this));
                animator.SetTrigger("MoveRight");
                break;
            case Patterns.pattern2:
                enemyData.bulletRate = bulletRate1_2;
                Debug.Log(enemyData.bulletRate);
                stateMachine.ChangeState(new LeftVerticalMove(this));
                animator.SetTrigger("MoveLeft");
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
                animator.SetBool("LaserAttck", true);
                break;
        }
        pattern = nextPattern;
    }

    private void RandomSetPattern()
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

    private bool CheckFlag5()
    {
        if (pattern == Patterns.none)
        {
            return false;
        }
        
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

    public override bool EnemyRotate(GameObject rotateObj, Vector3 angles, float rotatePerSpeed, float rotateTime, ref float time)
    {
        //Debug.Log("OverrideMoveBossEnemyRotate");
        time += Time.deltaTime;
        rotateObj.transform.Rotate(0, 0, rotatePerSpeed * Time.deltaTime);
        if (rotatePerSpeed > 0 && rotateObj.transform.localEulerAngles.z > angles.z)
        {
            //Debug.Log(rotateObj.name + "OVER");
            rotateObj.transform.localEulerAngles = angles;
            return true;
        }
        else if ((rotatePerSpeed < 0 && rotateObj.transform.localEulerAngles.z < angles.z) ||
                 (rotatePerSpeed < 0 && rotateObj.transform.localEulerAngles.z > 270))
        {
            //Debug.Log(rotateObj.name + "-OVER");
            rotateObj.transform.localEulerAngles = angles;
            return true;
        }
        if (time >= rotateTime)
        {
            rotateObj.transform.localEulerAngles = angles;
            //Debug.Log(rotateObj.name + "/END:" + rotateObj.transform.localEulerAngles.z);
            return true;
        }
        return false;
    }

    public override void DeathProcess()
    {
        pattern = Patterns.none;
        if (pattern == Patterns.pattern5)
        {
            LazerOff();
        }
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
    //上昇後の攻撃animation
    public void SpinAttack()
    {
        animator.SetBool("Spin", true);
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void Defeat()
    {
        animator.SetTrigger("Defeat");
    }

    public void boolReset()
    {
        animator.SetBool("MoveRight", false);
        animator.SetBool("MoveLeft", false);
        animator.SetBool("Spin", false);
        animator.SetBool("LaserAttck", false);
    }

    private void ResetTimer()
    {
        startTime = 0;
    }
    
}
