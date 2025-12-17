using UnityEngine;

public class UpHorizontalMove : MonoBehaviour,IState
{
    //上の横移動
    private readonly EnemyAI enemyAI;
    public UpHorizontalMove(EnemyAI enemyAI)
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
    private Vector3 upRightPos;
    private Vector3 upLeftPos;
    private float upPosY;
    private float rightPosX;
    private float leftPosX;
    
    public void Enter()
    {
         Debug.Log("3_Enter");
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
         upPosY = centerPos.y + enemyAI.ExcelData.Enemy[enemyAI.DataNumber].upRenge;
         rightPosX = centerPos.x + enemyAI.ExcelData.Enemy[enemyAI.DataNumber].rightRenge;
         leftPosX = centerPos.x - enemyAI.ExcelData.Enemy[enemyAI.DataNumber].leftRenge;
         upRightPos = new Vector3(rightPosX, upPosY, centerPos.z);
         upLeftPos = new Vector3(leftPosX, upPosY, centerPos.z);
         
         rotateAxis = enemyAI.RotateAxis;
         rotateAxisRotate = new Vector3(0, 0, 90);
         if (rotateAxis.transform.eulerAngles == rotateAxisRotate)
         {
             finishRotating = true;
         }
         else //if (rotateAxis.transform.eulerAngles.z == rotateAxisRotate.z)
         {
             angleZ = 45;
         }
    }
    
    public void Execute()
    {
         //Debug.Log("3_Execute");
         if (isCoolTime == true)
         {
             float diff = Time.time - startTime;
             if (diff < enemyAI.ExcelData.Enemy[enemyAI.DataNumber].coolTime)
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
         MoveUpCenter();
         if (finishRotating != true)
         {
             finishRotating = enemyAI.EnemyRotate(rotateAxis, rotateAxisRotate, angleZ, enemyAI.ExcelData.Enemy[enemyAI.DataNumber].rotateTime,
                 ref t);
         }
         if (finishMoving == true && finishRotating == true)
         {
             enemyAI.Change();
         }
    }
    
    public void Exit()
    {
        Debug.Log("3_Exit");
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
            Debug.Log("回転");
            moveBoss.SpinAttack();
            enemyAI.StartAttack();
        }
    }
    
    void MoveUpCenter()
    {
        //Debug.Log("MoveUpCenter");
        if (finishMoving == true) {return;}
        
        switch (moveCounter)
        {
            case 0:
                if (enemyAI.EnemyMove(moveEnemy, startPos, upRightPos, enemyAI.ExcelData.Enemy[enemyAI.DataNumber].horizontalTime,
                        startTime))
                {
                    NextMove();
                }
                break;
            case 1:
                if (enemyAI.EnemyMove(moveEnemy, upRightPos, upLeftPos, enemyAI.ExcelData.Enemy[enemyAI.DataNumber].verticalTime * 2,
                        startTime) == true)
                {
                    NextMove();
                }
                break;
            case 2:
                Debug.Log("終了");
                Initialization();
                finishMoving = true;
                moveBoss.boolReset();
                break;
        }
    }
}
