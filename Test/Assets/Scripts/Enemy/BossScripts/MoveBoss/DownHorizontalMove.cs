using UnityEngine;

public class DownHorizontalMove : MonoBehaviour,IState
{
    //上の横移動
    private EnemyAI enemyAI;
    public DownHorizontalMove(EnemyAI enemyAI)
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
    private MoveBoss moveBoss;
    
    //移動に関する座標
    private Vector3 centerPos;
    private Vector3 startPos;
    private Vector3 upLeftPos;
    private Vector3 upRightPos;
    private float upPosY;
    private float leftPosX;
    private float rightPosX;
    
    public void Enter()
    {
         Debug.Log("4_Enter");
         moveCounter = 0;
         moveEnemy = enemyAI.moveObj;
         moveBoss = moveEnemy.GetComponentInChildren<MoveBoss>();
         enemyAI.StopAttack();
         isCoolTime = true;
         Initialization();
         finishMoving = false;
         finishRotating = false;
         
         centerPos = enemyAI.centerPos;
         startPos = moveEnemy.transform.position;
         upPosY = centerPos.y + enemyAI.enemyData.upRenge;
         leftPosX = centerPos.x - enemyAI.enemyData.leftRenge;
         rightPosX = centerPos.x + enemyAI.enemyData.rightRenge;
         upLeftPos = new Vector3(leftPosX, upPosY, centerPos.z);
         upRightPos = new Vector3(rightPosX, upPosY, centerPos.z);
         
         rotateAxis = enemyAI.rotateAxis;
         rotateAxisRotate = new Vector3(0, 0, 90);
         if (rotateAxis.transform.eulerAngles == rotateAxisRotate)
         {
             finishRotating = true;
         }
         else //if (rotateAxis.transform.eulerAngles.z == rotateAxisRotate.z)
         {
             angleZ = -45;
         }
    }
    
    public void Execute()
    {
         //Debug.Log("4_Execute");
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
         MoveUpCenter();
         if (finishRotating != true)
         {
             finishRotating = enemyAI.EnemyRotate(rotateAxis, rotateAxisRotate, angleZ, enemyAI.enemyData.rotateTime,
                 ref t);
         }
         if (finishMoving == true && finishRotating == true)
         {
             enemyAI.Change();
         }
    }
    
    public void Exit()
    {
        Debug.Log("4_Exit");
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
             moveBoss.SpinAttack();
         }
     }
     
     void MoveUpCenter()
     {
         //Debug.Log("MoveDownCenter");
         if (finishMoving == true) {return;}
         
         switch (moveCounter)
         {
             case 0:
                 if (enemyAI.EnemyMove(moveEnemy, startPos, upLeftPos, enemyAI.enemyData.moveHorizontalTime,
                         startTime))
                 {
                     NextMove();
                 }
                 break;
             case 1:
                 if (enemyAI.EnemyMove(moveEnemy, upLeftPos, upRightPos, enemyAI.enemyData.moveVerticalTime * 2,
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
