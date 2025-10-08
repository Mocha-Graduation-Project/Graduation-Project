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
        //startPos = depthBoss.CenterPos;
        startPos = boss.transform.position;
        upMaxPos = new Vector3(startPos.x, startPos.y + depthBoss.EnemyData.UpRange, startPos.z);
        upFlontPos = new Vector3(upMaxPos.x, upMaxPos.y, depthBoss.FlontZPos);
        downFlontPos = new Vector3(upMaxPos.x, startPos.y + depthBoss.EnemyData.DownRange, depthBoss.FlontZPos);
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
                Move(startPos, upMaxPos, depthBoss.EnemyData.MoveVerticalTime);
                break;
            case 1: //画面外で前(プレイヤーの居るz座標)まで移動
                Move(upMaxPos, upFlontPos, depthBoss.EnemyData.WaitTime);
                break;
            case 2: //プレイヤーの居る座標に向かって落下攻撃
                Move(upFlontPos, downFlontPos, depthBoss.EnemyData.MoveVerticalTime);
                break;
            case 3: //待機
                Move(downFlontPos, downFlontPos, depthBoss.FallAttckWaitTime);
                break;
            case 4: //画面外へ上昇
                Move(downFlontPos, upFlontPos, depthBoss.EnemyData.MoveVerticalTime);
                break;
            case 5: //画面外で後ろ(元居たz座標)まで移動
                Move(upFlontPos, upMaxPos, depthBoss.EnemyData.WaitTime);
                break;
            case 6: //画面外から中央へ戻る
                Move(upMaxPos, startPos, depthBoss.EnemyData.MoveVerticalTime);
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
    
    void Move(Vector3 start,Vector3 end,float time)
    {
        float diff = Time.time - startTime;
        if (diff > time)
        {
            //Debug.Log("Change");
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
                    depthBoss.AttckWarningUI.SetWarning(AttckWarningUI.AttckType.fallingAttck, depthBoss.EnemyData.WaitTime);
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
            return;
        }
        float rate = diff / time;
        boss.transform.position = Vector3.Lerp(start, end, rate);
    }
}
