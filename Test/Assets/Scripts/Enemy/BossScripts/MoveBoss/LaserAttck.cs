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
    private bool isCoolTime;
    private bool is360Rotate;//360度回転か180回転か
    private float rotateTime;
    
    //移動に関する座標
    private Vector3 startPos;
    private Vector3 centerPos;

    public void Enter()
    {
        Debug.Log("5_Enter");
        is360Rotate = true;
        moveBoss = GameObject.Find("MoveBoss").GetComponent<MoveBoss>();
        moveCounter = 0;
        moveEnemy = enemyAI.moveObj;
        enemyAI.StopAttack();
        t = 0f;
        Initialization();
        finishMoving = false;
        finishRotating = false;
        isCoolTime = true;

        centerPos = enemyAI.centerPos;
        startPos = moveEnemy.transform.position;
        
        rotateAxis = enemyAI.rotateAxis;
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
        
        if (is360Rotate == true)
        {
            rotateTime = 4;
        }
        else
        {
            rotateTime = 2;
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
                if (enemyAI.EnemyMove(moveEnemy, startPos, centerPos, enemyAI.enemyData.moveVerticalTime, startTime) ==
                    true)
                {
                    NextMove();
                }

                if (finishRotating != true)
                {
                    finishRotating = Rotate(rotateAxis, rotateAxisRotate, angleZ,
                        enemyAI.enemyData.rotateTime, ref t);
                }
                FinishCheck();
                break;
            case 1:
                //360度回転させる
                if (Rotate(rotateAxis, rotateAxisRotate, angleZ90,
                        moveBoss.Rotate90PerSec * rotateTime, ref t) == true)
                {
                    finishRotating = true;
                }
                FinishCheck();
                break;
            case 2:
                if (WaitCoolTime() == false)
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
        isCoolTime = true;
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

    bool WaitCoolTime()
    {
        if (isCoolTime == true)
        {
            float diff = Time.time - startTime;
            if (diff < enemyAI.enemyData.coolTime)
            {
                //Debug.Log("クールタイム中");
                return isCoolTime;
            }
            else
            {
                Initialization();
                isCoolTime = false;
            }
        }
        return isCoolTime;
    }
}
