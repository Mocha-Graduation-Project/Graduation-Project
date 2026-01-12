using UnityEngine;

public class LeftVerticalMove : MonoBehaviour,IState
{
    //左の縦移動
    private EnemyAI enemyAI;
    public LeftVerticalMove(EnemyAI enemyAI)
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
    MoveBoss moveBoss;
    
    //移動に関する座標
    private Vector3 centerPos;
    private Vector3 startPos;
    private Vector3 leftCenterPos;
    private Vector3 leftUpPos;
    private Vector3 leftDownPos;
    private float leftPosX;
    private float upPosY;
    private float downPosY;

    public void Enter()
    {
        Debug.Log("2_Enter");
        moveCounter = 0;
        moveEnemy = enemyAI.MoveObj;
        moveBoss = moveEnemy.GetComponentInChildren<MoveBoss>();
        enemyAI.StopAttack();
        isCoolTime = true;
        Initialization();
        finishMoving = false;
        finishRotating = false;

        centerPos = enemyAI.CenterPos;
        startPos = moveEnemy.transform.position;
        leftPosX = centerPos.x - enemyAI.CSVData.enemiesData[enemyAI.DataNumber].leftRenge;
        upPosY = centerPos.y + enemyAI.CSVData.enemiesData[enemyAI.DataNumber].upRenge;
        downPosY = centerPos.y - enemyAI.CSVData.enemiesData[enemyAI.DataNumber].downRenge;
        leftUpPos = new Vector3(leftPosX, upPosY, centerPos.z);
        leftDownPos = new Vector3(leftPosX, downPosY, centerPos.z);
        leftCenterPos= new Vector3(leftPosX, centerPos.y, centerPos.z);
        
        rotateAxis = enemyAI.RotateAxis;
        rotateAxisRotate = new Vector3(0, 0, 180);
        if (rotateAxis.transform.eulerAngles == rotateAxisRotate)
        {
            finishRotating = true;
        }
        else if (rotateAxis.transform.eulerAngles == new Vector3(0, 0, 0))
        {
            angleZ = 90;
        }
        else //if (rotateAxis.transform.eulerAngles.z == rotateAxisRotate.z)
        {
            angleZ = 45;
        }
    }

    public void Execute()
    {
        //Debug.Log("2_Execute");
        if (isCoolTime == true)
        {
            float diff = Time.time - startTime;
            if (diff < enemyAI.CSVData.enemiesData[enemyAI.DataNumber].coolTime)
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
        MoveLeftCenter();
        if (finishRotating != true)
        {
            finishRotating =
                enemyAI.EnemyRotate(rotateAxis, rotateAxisRotate, angleZ, enemyAI.CSVData.enemiesData[enemyAI.DataNumber].rotateTime, ref t);
        }
        if (finishMoving == true && finishRotating == true)
        {
            enemyAI.Change();
        }
    }

    public void Exit()
    {
        Debug.Log("2_Exit");
    }

    void Initialization()
    {
        startTime = Time.time;
        //centerPos = moveEnemy.transform.position;
    }
    
    void NextMove()
    {
        moveCounter++;
        Initialization();
        centerPos = moveEnemy.transform.position;
        if (moveCounter == 1)
        {
            enemyAI.StartAttack();
            moveBoss.Attack();
        }
    }

    void MoveLeftCenter()
    {
        //Debug.Log("MoveRightCenter");
        if (finishMoving == true) {return;}
        
        switch (moveCounter)
        {
            case 0:
                if (enemyAI.EnemyMove(moveEnemy, startPos, leftCenterPos, enemyAI.CSVData.enemiesData[enemyAI.DataNumber].verticalTime,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 1:
                if (enemyAI.EnemyMove(moveEnemy, leftCenterPos, leftUpPos, enemyAI.CSVData.enemiesData[enemyAI.DataNumber].horizontalTime,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 2:
                if (enemyAI.EnemyMove(moveEnemy, leftUpPos, leftDownPos, enemyAI.CSVData.enemiesData[enemyAI.DataNumber].horizontalTime * 2,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 3:
                if (enemyAI.EnemyMove(moveEnemy, leftDownPos, leftCenterPos, enemyAI.CSVData.enemiesData[enemyAI.DataNumber].horizontalTime,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 4:
                Debug.Log("終了");
                Initialization();
                finishMoving = true;
                moveBoss.boolReset();
                break;
        } 
    }
}
