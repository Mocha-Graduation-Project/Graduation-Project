using System.Collections;
using UnityEngine;

public class RightVerticalMove : MonoBehaviour,IState
{
    //右の縦移動
    private EnemyAI enemyAI;
    public RightVerticalMove(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }
    
    int moveCounter;
    GameObject rotateAxis;
    GameObject moveEnemy;
    private float t;
    private float startTime;
    private float rotateSpeed = 0.1f;
    private bool finishMoving;
    private bool finishRotating;
    private float angleZ;
    Vector3 rotateAxisRotate;
    private bool isCoolTime;
    
    //移動に関する座標
    private Vector3 centerPos;
    private Vector3 startPos;
    private Vector3 rightCenterPos;
    private Vector3 rightUpPos;
    private Vector3 rightDownPos;
    private float rightPosX;
    private float upPosY;
    private float downPosY;

    public void Enter()
    {
        Debug.Log("1_Enter");
        moveCounter = 0;
        moveEnemy = enemyAI.moveObj;
        enemyAI.StopAttack();
        isCoolTime = true;
        Initialization();
        finishMoving = false;
        finishRotating = false;

        centerPos = enemyAI.centerPos;
        startPos = moveEnemy.transform.position;
        rightPosX = centerPos.x + enemyAI.enemyData.rightRenge;
        upPosY = centerPos.y + enemyAI.enemyData.upRenge;
        downPosY = centerPos.y - enemyAI.enemyData.downRenge;
        rightUpPos = new Vector3(rightPosX, upPosY, centerPos.z);
        rightDownPos = new Vector3(rightPosX, downPosY, centerPos.z);
        rightCenterPos= new Vector3(rightPosX, centerPos.y, centerPos.z);
        
        rotateAxis = enemyAI.rotateAxis;
        rotateAxisRotate = new Vector3(0, 0, 0);
        if (rotateAxis.transform.eulerAngles == rotateAxisRotate)
        {
            finishRotating = true;
        }
        else if (rotateAxis.transform.eulerAngles == new Vector3(0, 0, 180))
        {
            angleZ = -90;
        }
        else //if (rotateAxis.transform.eulerAngles.z == rotateAxisRotate.z)
        {
            angleZ = -45;
        }
    }

    public void Execute()
    {
        //Debug.Log("1_Execute");
        if (isCoolTime == true)
        {
            float diff = Time.time - startTime;
            if (diff < enemyAI.enemyData.coolTime)
            {
                //Debug.Log("クールタイム中");
                return;
            }
            else
            {
                Initialization();
                isCoolTime = false;
            }
        }
        MoveRightCenter();
        if (finishRotating != true)
        {
            finishRotating =
                enemyAI.EnemyRotate(rotateAxis, rotateAxisRotate, angleZ, enemyAI.enemyData.rotateTime, ref t);
        }
        if (finishMoving == true && finishRotating == true)
        {
            enemyAI.Change();
        }
    }

    public void Exit()
    {
        Debug.Log("1_Exit");
    }

    void Initialization()
    {
        startTime = Time.time;
    }

    void NextMove()
    {
        moveCounter++;
        Initialization();
        centerPos = moveEnemy.transform.position;
        if (moveCounter == 1)
        {
            enemyAI.StartAttack();
        }
    }

    void Rotate(Vector3 angles)
    {
        //Debug.Log("angle:"+rotateAxis.transform.rotation.eulerAngles);
        if (finishRotating == true)
        {
            return;
        }
        t += Time.deltaTime;
        rotateAxis.transform.Rotate(0, 0, angleZ * Time.deltaTime);
        if (t >= 2.0f)
        {
            rotateAxis.transform.eulerAngles = angles;
            finishRotating = true;
        }
    }
    
    void MoveRightCenter()
    {
        //Debug.Log("MoveRightCenter");
        if (finishMoving == true) {return;}
        
        switch (moveCounter)
        {
            case 0:
                if (enemyAI.EnemyMove(moveEnemy, startPos, rightCenterPos, enemyAI.enemyData.moveVerticalTime,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 1:
                if (enemyAI.EnemyMove(moveEnemy, rightCenterPos, rightUpPos, enemyAI.enemyData.moveHorizontalTime,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 2:
                if (enemyAI.EnemyMove(moveEnemy, rightUpPos, rightDownPos, enemyAI.enemyData.moveHorizontalTime * 2,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 3:
                if (enemyAI.EnemyMove(moveEnemy, rightDownPos, rightCenterPos, enemyAI.enemyData.moveHorizontalTime,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 4:
                Debug.Log("終了");
                Initialization();
                finishMoving = true;
                break;
        }
    }
}
