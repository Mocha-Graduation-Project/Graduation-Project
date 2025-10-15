using UnityEngine;

public class FallingAttack : MonoBehaviour, IState
{
    private readonly EnemyAI enemyAI;
    public FallingAttack(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }
    
    int moveCounter;
    DepthBoss depthBoss;
    GameObject boss;
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
        depthBoss = GameObject.Find("DepthBoss").GetComponent<DepthBoss>();
        boss = depthBoss.Boss;
        isCoolTime = true;
        Initialization();
        startPos = boss.transform.position;
        upMaxPos = new Vector3(startPos.x, startPos.y + depthBoss.PatrolEnemyData.UpRange, startPos.z);
        upFlontPos = new Vector3(upMaxPos.x, upMaxPos.y, depthBoss.FlontZPos);
        downFlontPos = new Vector3(upMaxPos.x, startPos.y + depthBoss.PatrolEnemyData.DownRange, depthBoss.FlontZPos);
    }

    public void Execute()
    {
        //Debug.Log("2_3_Execute");
        if (isCoolTime == true)
        {
            float diff = Time.time - startTime;
            if (diff < depthBoss.CoolTime)
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
                if (enemyAI.EnemyMove(boss, startPos, upMaxPos,
                        depthBoss.PatrolEnemyData.MoveVerticalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 1: //画面外で前(プレイヤーの居るz座標)まで移動
                if (enemyAI.EnemyMove(boss, upMaxPos, upFlontPos,
                        depthBoss.PatrolEnemyData.WaitTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 2: //プレイヤーの居る座標に向かって落下攻撃
                if (enemyAI.EnemyMove(boss, upFlontPos, downFlontPos,
                        depthBoss.PatrolEnemyData.MoveVerticalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 3: //待機
                if (enemyAI.EnemyMove(boss, downFlontPos, downFlontPos,
                        depthBoss.FallAttckWaitTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 4: //画面外へ上昇
                if (enemyAI.EnemyMove(boss, downFlontPos, upFlontPos,
                        depthBoss.PatrolEnemyData.MoveVerticalTime, startTime) == true)
                {
                    NextMove();
                }
                break;
            case 5: //画面外で後ろ(元居たz座標)まで移動
                if (enemyAI.EnemyMove(boss, upFlontPos, upMaxPos,
                        depthBoss.PatrolEnemyData.WaitTime, startTime) == true)
                {
                    NextMove();
                }                break;
            case 6: //画面外から中央へ戻る
                if (enemyAI.EnemyMove(boss, upMaxPos, startPos,
                        depthBoss.PatrolEnemyData.MoveVerticalTime, startTime) == true)
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
                float pos = depthBoss.Player.transform.position.x;
                upFlontPos = new Vector3(pos, upFlontPos.y, upFlontPos.z);
                downFlontPos = new Vector3(pos, downFlontPos.y, downFlontPos.z);
                depthBoss.FallingAttckPos = downFlontPos;
                depthBoss.AttckWarningUI.SetWarning(AttckWarningUI.AttckType.fallingAttck, depthBoss.PatrolEnemyData.WaitTime);
                break;
            case 3: 
                //攻撃中を解除
                depthBoss.AttckFalse();
                break;
            case 5:
                //攻撃中を有効
                depthBoss.AttckTrue();
                break;
        }
    }
}
