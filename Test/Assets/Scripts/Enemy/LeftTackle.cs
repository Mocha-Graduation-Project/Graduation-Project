using UnityEngine;

public class LeftTackle : MonoBehaviour, IState
{
    private EnemyAI enemyAI;
    public LeftTackle(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }
    
    int moveCounter;
    DepthBoss depthBoss;
    GameObject moveEnemy;
    private float t;
    private float startTime;
    
    private Vector3 startPos;
    private Vector3 rightMaxPos;
    private Vector3 leftMaxPos;
    private Vector3 rightFlontPos;
    private Vector3 leftFlontPos;
    private bool isCoolTime;

    public void Enter()
    {
        Debug.Log("2_2_Enter");
        moveCounter = 0;
        depthBoss = GameObject.Find("DepthBoss").GetComponent<DepthBoss>();
        moveEnemy = enemyAI.moveObj;
        isCoolTime = true;
        Initialization();
        
        startPos = enemyAI.centerPos;
        rightMaxPos = new Vector3(startPos.x + enemyAI.enemyData.rightRenge, startPos.y, startPos.z);
        rightFlontPos = new Vector3(rightMaxPos.x, rightMaxPos.y, depthBoss.FlontZPos);
        leftMaxPos = new Vector3(startPos.x - enemyAI.enemyData.leftRenge, startPos.y, startPos.z);
        leftFlontPos = new Vector3(leftMaxPos.x, leftMaxPos.y, depthBoss.FlontZPos);
    }

    public void Execute()
    {
        //Debug.Log("2_2_Execute");
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

        switch (moveCounter)
        {
            case 0: //中央から画面端に消える
                if (enemyAI.EnemyMove(moveEnemy, startPos, leftMaxPos,
                        enemyAI.enemyData.moveHorizontalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 1: //画面外で前(プレイヤーの居るz座標)まで移動
                if (enemyAI.EnemyMove(moveEnemy, leftMaxPos, leftFlontPos,
                        enemyAI.enemyData.moveWaitTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 2: //プレイヤーに向かってタックル(画面外から画面外へ)
                if (enemyAI.EnemyMove(moveEnemy, leftFlontPos, rightFlontPos,
                        enemyAI.enemyData.moveHorizontalTime * 2, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 3: //画面外で後ろ(元居たz座標)まで移動
                if (enemyAI.EnemyMove(moveEnemy, rightFlontPos, rightMaxPos,
                        enemyAI.enemyData.moveWaitTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 4: //画面端から中央へ移動
                if (enemyAI.EnemyMove(moveEnemy, rightMaxPos, startPos,
                        enemyAI.enemyData.moveHorizontalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 5: //次のパターンへ
                depthBoss.Change();
                break;
        }
    }
    
    public void Exit()
    {
        Debug.Log("2_2_Exit");
    }
    
    void Initialization()
    {
        startTime = Time.time;
    }
    
    void NextMove()
    {
        Initialization();
        moveCounter++;
        if (moveCounter == 1)
        {
            depthBoss.AttckWarningUI.SetWarning(AttckWarningUI.AttckType.leftTackle, enemyAI.enemyData.moveWaitTime);
        }
    }
}
