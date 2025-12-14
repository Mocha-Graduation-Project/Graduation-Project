using UnityEngine;

public class FallingAttack : MonoBehaviour, IState
{
    private EnemyAI enemyAI;
    public FallingAttack(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }
    
    int moveCounter;
    DepthBoss depthBoss;
    GameObject moveEnemy;
    private float t;
    private float startTime;
    
    private Vector3 startPos;
    private Vector3 upMaxPos;
    private Vector3 upFlontPos;
    private Vector3 downFlontPos;
    private bool isCoolTime;

    public void Enter()
    {
        Debug.Log("2_3_Enter");
        moveCounter = 0;
        depthBoss = (DepthBoss)enemyAI;
        moveEnemy = enemyAI.moveObj;
        isCoolTime = true;
        Initialization();

        startPos = enemyAI.centerPos;
        upMaxPos = new Vector3(startPos.x, startPos.y + enemyAI.enemyData.upRenge, startPos.z);
        upFlontPos = new Vector3(upMaxPos.x, upMaxPos.y, depthBoss.FlontZPos);
        downFlontPos = new Vector3(upMaxPos.x, startPos.y - enemyAI.enemyData.downRenge, depthBoss.FlontZPos);
    }

    public void Execute()
    {
        //Debug.Log("2_3_Execute");
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
            case 0: //中央から画面外へ
                if (enemyAI.EnemyMove(moveEnemy, startPos, upMaxPos,
                        enemyAI.enemyData.moveVerticalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 1: //画面外で前(プレイヤーの居るz座標)まで移動
                if (enemyAI.EnemyMove(moveEnemy, upMaxPos, upFlontPos,
                        enemyAI.enemyData.moveWaitTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 2: //プレイヤーの居る座標に向かって落下攻撃
                if (enemyAI.EnemyMove(moveEnemy, upFlontPos, downFlontPos,
                        enemyAI.enemyData.moveVerticalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 3: //待機
                if (enemyAI.EnemyMove(moveEnemy, downFlontPos, downFlontPos,
                        depthBoss.FallAttckWaitTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 4: //画面外へ上昇
                if (enemyAI.EnemyMove(moveEnemy, downFlontPos, upFlontPos,
                        enemyAI.enemyData.moveVerticalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 5: //画面外で後ろ(元居たz座標)まで移動
                if (enemyAI.EnemyMove(moveEnemy, upFlontPos, upMaxPos,
                        enemyAI.enemyData.moveWaitTime, startTime) == true)
                {
                    NextMove();
                }                break;
            case 6: //画面外から中央へ戻る
                if (enemyAI.EnemyMove(moveEnemy, upMaxPos, startPos,
                        enemyAI.enemyData.moveVerticalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 7: //次のパターンへ
                depthBoss.Change();
                break;
        }
    }
    
    public void Exit()
    {
        Debug.Log("2_3_Exit");
    }
    
    void Initialization()
    {
        startTime = Time.time;
    }

    void NextMove()
    {
        Initialization();
        moveCounter++;
        switch (moveCounter)
        {
            case 1:
                //プレイヤーのx座標をupFlontPos、downFlontPosに入れる
                //float pos = depthBoss.Player.transform.position.x;
                float pos = enemyAI.player.transform.position.x;
                upFlontPos = new Vector3(pos, upFlontPos.y, upFlontPos.z);
                downFlontPos = new Vector3(pos, downFlontPos.y, downFlontPos.z);
                depthBoss.FallingAttckPos = downFlontPos;
                depthBoss.AttckWarningUI.SetWarning(AttckWarningUI.AttckType.fallingAttck, enemyAI.enemyData.moveWaitTime);
                break;
            case 3: 
                //攻撃中を解除
                depthBoss.AttackFalse();
                break;
            case 5:
                //攻撃中を有効
                depthBoss.AttackTrue();
                break;
        }
    }
}
