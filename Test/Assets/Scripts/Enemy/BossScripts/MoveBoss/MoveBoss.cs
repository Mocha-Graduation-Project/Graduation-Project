using System;
using System.Collections.Generic;
using Enemy.BossScripts.DepthBoss;
using NUnit.Framework.Internal;
using Player;
using Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
using UnityEngine.VFX;
using Random = UnityEngine.Random;
using System.IO;



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
        [JapaneseLabel("攻撃前の眼のエフェクト")] public GameObject attackEyeEffect;
    }

    [SerializeField] private StateMachine stateMachine;

    [Header("共通")]
    [SerializeField] [JapaneseLabel("現在の行動パターン")]Patterns pattern;

    [JapaneseLabel("MoveBossDataの値")] private int moveBossNum = 1;

    [Space(5)] [Header("パターン1,2,3,4")]
    
    [Header("回転蜂")] public List<BeeHead> beeHeads = new();

    [JapaneseLabel("パターン1,2の発射レート")] private float bulletRate1_2;
    [JapaneseLabel("パターン3,4の発射レート")] private float bulletRate3_4;
    
    [JapaneseLabel("回転する蜂の頭")] private GameObject rotateBeeHead;
    private GameObject shotObj;
    private GameObject attackEffect;
    private float rotateZ;
    private float rotateSpeed;
    private float startTime;
    private bool isactive;
    private bool isBackRotate;
    [JapaneseLabel("攻撃時の回転")] private bool finishRotation;

    [Space(5)] [Header("パターン5")]
    [SerializeField] [JapaneseLabel("レーザーのVFX")] private VisualEffect[] laserVFX;
    [SerializeField] [JapaneseLabel("レーザーの当たり判定")] private GameObject[] laser;
    [JapaneseLabel("パターン5を行うようになるHP")] private float changeHP;
    [JapaneseLabel("パターン5を行うようになるHP割合")] private float changeHPPercent;
    [JapaneseLabel("90度回転するのにかかる時間")] private float rotate90PerSec;
    [JapaneseLabel("360度回転するか(falseの場合、180度回転)")] private bool is360Rotate;
    [JapaneseLabel("レーザー回転にかかる時間")] private float rotateLazerTime;
    private bool patten5Flag;
    [JapaneseLabel("パターン1～4を行った回数")] private int actioncounter;
    [JapaneseLabel("パターン5に入るまでの1～4のループ回数")] private int maxLoop;
    [JapaneseLabel("パターンの総数")] private int maxAction;

    [JapaneseLabel("現在のループ回数")] private int loopCounter;
    
    public float Rotate90PerSec { get { return rotate90PerSec; } }
    public bool Is360Rotate { get { return is360Rotate; } }
    public float RotateLazerTime { get { return rotateLazerTime; } }
    
    [SerializeField] private Animator animator;

    public override void SetUp()
    {
        string dataPath = "CSVData/MoveBossData";
        TextAsset dataAsset = (TextAsset)Resources.Load<TextAsset>(dataPath);
        StringReader reader = new StringReader(dataAsset.text);
        int i = 0;
        while (reader.Peek() != -1)
        {
            string lineData = reader.ReadLine();
            string[] lineSprit = lineData.Split(',');

            if (i == moveBossNum)
            {
                float.TryParse(lineSprit[0], out bulletRate1_2);
                float.TryParse(lineSprit[1], out bulletRate3_4);
                float.TryParse(lineSprit[2], out rotate90PerSec);
                float.TryParse(lineSprit[3], out changeHPPercent);
                int.TryParse(lineSprit[4], out maxLoop);
                switch (lineSprit[5])
                {
                    case "TRUE":
                        is360Rotate = true;
                        rotateLazerTime = rotate90PerSec * 4.0f;
                        break;
                    case "FALSE":
                        is360Rotate = false;
                        rotateLazerTime = rotate90PerSec * 2.0f;
                        break;
                }
            }
            i++;
        }
        
        actioncounter = 0;
        loopCounter = 0;
        maxAction = 4;
        changeHP = CSVData.enemiesData[DataNumber].maxHP * (changeHPPercent * 0.01f);
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

        foreach (VisualEffect vfx in laserVFX)
        {
            if (vfx != null)
            {
                vfx.SetFloat("BeamLifeTime", rotateLazerTime);
            }
        }

        IsSetUp = true;
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
            if (beeHeads[i].beeShield.GetComponent<ReflectionBee>().IsShieldDown == false)
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
            attackEffect.GetComponent<VisualEffect>().SendEvent("OnPlay");
        }
    }

    void SetBeeShot(BeeHead setBeeHead)
    {
        shotObj = setBeeHead.shotPos;
        attackEffect = setBeeHead.attackEyeEffect;
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
           // BeforeAttackText.After();

            // for (int i = 0; i < beeHeads.Count; i++)
            // {
            //     Debug.Log("Shot[" + beeHeads[i].shotPos.name + "]/" + beeHeads[i].shotPos.transform.position);
            // }
            // Debug.Log("Attck:" + shotPos);
            GameObject bullets = Instantiate(BulletObj, shotObj.transform.position, Quaternion.identity);

            Bullet reflectionBullet = bullets.GetComponent<Bullet>();
            
            reflectionBullet.SetStraightPowerEnemy(StraightObj.transform.rotation.eulerAngles,true);
            
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
                ChangeBulletRate(bulletRate1_2);
                stateMachine.ChangeState(new RightVerticalMove(this));
                animator.SetTrigger("MoveRight");
                break;
            case Patterns.pattern2:
                ChangeBulletRate(bulletRate1_2);
                stateMachine.ChangeState(new LeftVerticalMove(this));
                animator.SetTrigger("MoveLeft");
                break;
            case Patterns.pattern3:
                ChangeBulletRate(bulletRate3_4);
                stateMachine.ChangeState(new UpHorizontalMove(this));
                break;
            case Patterns.pattern4:
                ChangeBulletRate(bulletRate3_4);
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
            if (Hp <= changeHP)
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
        foreach (VisualEffect vfx in laserVFX)
        {
            if (vfx != null)
            {
                vfx.Play();
            }
        }
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
