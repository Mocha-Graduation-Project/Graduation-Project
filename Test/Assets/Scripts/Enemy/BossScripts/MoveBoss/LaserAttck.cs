using System.Collections;
using UnityEngine;

public class LaserAttck : MonoBehaviour,IState
{
    private EnemyAI enemyAI;
    public LaserAttck(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }
    
    int moveCounter;
    MoveBoss moveBoss;
    GameObject rotateAxis;
    GameObject moveEnemy;
    private float t;
    private float startTime;
    private float rotateSpeed = 0.1f;
    private bool finishMoving;
    private bool finishRotating;
    private float angleZ;
    private float angleZ90;
    Vector3 rotateAxisRotate;
    private float rotateTime;
    private bool isWaitng;
    
    //移動に関する座標
    private Vector3 startPos;
    private Vector3 centerPos;

    public void Enter()
    {
        Debug.Log("5_Enter");
        moveBoss = (MoveBoss)enemyAI;
        moveCounter = 0;
        moveEnemy = enemyAI.MoveObj;
        enemyAI.StopAttack();
        t = 0f;
        Initialization();
        finishMoving = false;
        finishRotating = false;
        isWaitng = true;

        centerPos = enemyAI.CenterPos;
        startPos = moveEnemy.transform.position;
        
        rotateAxis = enemyAI.RotateAxis;
        rotateAxisRotate = new Vector3(0, 0, 0);
        angleZ90 = 90f / moveBoss.Rotate90PerSec;
        //ランダムで回転方向を決める
        int random = Random.Range(0, 2);
        Debug.Log("random:" + random);
        switch (random)
        {
            case 0:
                angleZ90 *= 1;
                break;
            case 1:
                angleZ90 *= (-1);
                break;
            default:
                Debug.Log("error:" + random);
                break;
        }
        Debug.Log("回転方向:" + angleZ90);
        
        if (rotateAxis.transform.eulerAngles == rotateAxisRotate)
        {
            finishRotating = true;
        }
        else if (rotateAxis.transform.eulerAngles == new Vector3(0, 0, 180))
        {
            rotateAxisRotate = new Vector3(0, 0, 180);
            finishRotating = true;
            angleZ = 90;
        }
        else
        {
            angleZ = 45;
        }
        
        if (moveBoss.Is360Rotate == false)
        {
            if (rotateAxis.transform.eulerAngles == new Vector3(0, 0, 0))
            {
                rotateAxisRotate = new Vector3(0, 0, 180);
            }
            else if (rotateAxis.transform.eulerAngles == new Vector3(0, 0, 180))
            {
                rotateAxisRotate = new Vector3(0, 0, 0);
            }
        }
    }

    public void Execute()
    {
        //Debug.Log("5_Execute");
        switch (moveCounter)
        {
            case 0:
                //Move(moveBoss.CenterPos,moveBoss.MoveTime);
                if (enemyAI.EnemyMove(moveEnemy, startPos, centerPos, enemyAI.CSVData.enemiesData[enemyAI.DataNumber].verticalTime, startTime) ==
                    true)
                {
                    NextMove();
                }

                if (finishRotating != true)
                {
                    finishRotating = Rotate(rotateAxis, rotateAxisRotate, angleZ,
                        enemyAI.CSVData.enemiesData[enemyAI.DataNumber].rotateTime, ref t);
                }
                FinishCheck();
                break;
            case 1:
                if (isWaitng == false)
                {
                    //360度回転させる
                    if (Rotate(rotateAxis, rotateAxisRotate, angleZ90,
                            moveBoss.RotateLazerTime, ref t) == true)
                    {
                        finishRotating = true;
                    }
                    FinishCheck();
                }
                else
                {
                    if (WaitCoolTime(0.2f) == false)
                    {
                        isWaitng = false;
                    }
                }
                break;
            case 2:
                if (WaitCoolTime(enemyAI.CSVData.enemiesData[enemyAI.DataNumber].coolTime) == false)
                {
                    enemyAI.Change();
                }
                break;
        }
    }
    
    public void Exit()
    {
        Debug.Log("5_Exit");
    }
    
    void Initialization()
    {
        startTime = Time.time;
        t = 0f;
    }

    void NextMove()
    {
        finishMoving = true;
    }

    bool Rotate(GameObject rotateObj, Vector3 angles, float rotatePerSpeed, float rotateTime,
        ref float time)
    {
        time += Time.deltaTime;
        rotateObj.transform.Rotate(0, 0, rotatePerSpeed * Time.deltaTime);
        if (time >= rotateTime)
        {
            rotateObj.transform.localEulerAngles = angles;
            Debug.Log(rotateObj.name + "/LaserEND:" + rotateObj.transform.localEulerAngles.z);
            return true;
        }
        return false;
    }
    
    void FinishCheck()
    {
        if (finishRotating == true && finishMoving == true)
        {
            moveCounter++;
            finishRotating = false;
            switch (moveCounter)
            {
                case 0:
                    break;
                case 1:
                    moveBoss.LazerOn();
                    break;
                case 2:
                    moveBoss.boolReset();
                    moveBoss.LazerOff();
                    break;
            }
            Initialization();
        }
    }

    bool WaitCoolTime(float waitTime)
    {
        bool isCoolTime = true;
        float diff = Time.time - startTime;
        if (diff >= waitTime)
        {
            Initialization();
            isCoolTime = false;
        }

        return isCoolTime;
    }
}
