using UnityEngine;

public class LeftTackle : MonoBehaviour, IState
{
    private readonly EnemyAI enemyAI;
    public LeftTackle(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }
    
    int moveCounter;
    DepthBoss depthBoss;
    GameObject boss;
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
        boss = depthBoss.Boss;
        isCoolTime = true;
        Initialization();
        startPos = depthBoss.CenterPos;
        rightMaxPos = new Vector3(startPos.x + depthBoss.EnemyData.RightRange, startPos.y, startPos.z);
        rightFlontPos = new Vector3(rightMaxPos.x, rightMaxPos.y, depthBoss.FlontZPos);
        leftMaxPos = new Vector3(startPos.x - depthBoss.EnemyData.LeftRange, startPos.y, startPos.z);
        leftFlontPos = new Vector3(leftMaxPos.x, leftMaxPos.y, depthBoss.FlontZPos);
    }

    public void Execute()
    {
        //Debug.Log("2_2_Execute");
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
            case 0: //中央から画面端に消える
                Move(startPos, leftMaxPos, depthBoss.EnemyData.MoveHorizontalTime);
                break;
            case 1: //画面外で前(プレイヤーの居るz座標)まで移動
                Move(leftMaxPos, leftFlontPos, depthBoss.EnemyData.WaitTime);
                break;
            case 2: //プレイヤーに向かってタックル(画面外から画面外へ)
                Move(leftFlontPos, rightFlontPos, depthBoss.EnemyData.MoveHorizontalTime * 2);
                break;
            case 3: //画面外で後ろ(元居たz座標)まで移動
                Move(rightFlontPos, rightMaxPos, depthBoss.EnemyData.WaitTime);
                break;
            case 4: //画面端から中央へ移動
                Move(rightMaxPos, startPos, depthBoss.EnemyData.MoveHorizontalTime);
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
    
    void Move(Vector3 start,Vector3 end,float time)
    {
        float diff = Time.time - startTime;
        if (diff > time)
        {
            //Debug.Log("Change");
            Initialization();
            moveCounter++;
            return;
        }
        float rate = diff / time;
        boss.transform.position = Vector3.Lerp(start, end, rate);
    }
}
