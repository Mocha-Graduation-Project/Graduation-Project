using UnityEngine;

public class LaserAttck : MonoBehaviour,IState
{
    private readonly EnemyAI enemyAI;
    
    int moveCounter;
    MoveBoss moveBoss;
    GameObject rotateAxis;
    GameObject boss;
    private float t;
    private float startTime;
    private Vector3 startPos;
    private float rotateSpeed = 0.1f;
    private bool finishMoving;
    private bool finishRotating;
    private float angleZ;
    private float angleZ90;
    Vector3 rotateAxisRotate;
    private bool isCoolTime;
    private bool is360Rotate;//360度回転か180回転か
    private float rotateTime;
    
    public LaserAttck(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        Debug.Log("5_Enter");
        is360Rotate = true;
        
        moveCounter = 0;
        moveBoss = GameObject.Find("MoveBoss").GetComponent<MoveBoss>();
        boss = moveBoss.Boss;
        moveBoss.EnemyScript.StopAttck();
        t = 0f;
        Initialization();
        finishMoving = false;
        finishRotating = false;
        isCoolTime = true;
        startPos = boss.transform.position;
        rotateAxis = moveBoss.RotateAxis;
        rotateAxisRotate = new Vector3(0, 0, 0);
        angleZ90 = 90f;
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
                Move(moveBoss.CenterPos,moveBoss.MoveTime);
                Rotate(rotateAxisRotate, 2, angleZ);
                FinishCheck();
                break;
            case 1:
                //360度回転させる
                if (WaitCoolTime() == false)
                {
                    Rotate(rotateAxisRotate, moveBoss.Rotate90PerSec * rotateTime, angleZ90);
                    FinishCheck();
                }
                break;
            case 2:
                if (WaitCoolTime() == false)
                {
                    moveBoss.Change();
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
    }
    
    void Move(Vector3 endPos,float time)
    {
        if (finishMoving == true) { return; }
        
        float diff = Time.time - startTime;
        if (diff > time)
        {
            //Debug.Log("Change");
            finishMoving = true;
            startPos = boss.transform.position;
            return;
        }
        float rate = diff / time;
        boss.transform.position = Vector3.Lerp(startPos, endPos, rate);
    }

    void Rotate(Vector3 angles, float time,float z)
    {
        //Debug.Log("angle:"+rotateAxis.transform.rotation.eulerAngles);
        if (finishRotating == true)
        {
            return;
        }
        t += Time.deltaTime;
        rotateAxis.transform.Rotate(0, 0, z * Time.deltaTime);
        if (t >= time)
        {
            rotateAxis.transform.eulerAngles = angles;
            finishRotating = true;
        }
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
                    moveBoss.LazerOff();
                    break;
            }
            //moveBoss.InvertActiveLazer();
            Initialization();
        }
    }

    bool WaitCoolTime()
    {
        if (isCoolTime == true)
        {
            float diff = Time.time - startTime;
            if (diff < moveBoss.CoolTime)
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
