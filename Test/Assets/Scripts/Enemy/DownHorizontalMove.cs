using UnityEngine;

public class DownHorizontalMove : MonoBehaviour,IState
{
    //上の横移動
    private readonly EnemyAI enemyAI;
    
    int moveCounter;
    MoveBoss moveBoss;
    GameObject rotateAxis;
    GameObject boss;
    private float t;
    private float startTime;
    private Vector3 startPos;
    private float rotateSpeed = 0.1f;
    private Vector3 x;
    private bool finishMoving;
    private bool finishRotating;
    private float angleZ;
    Vector3 rotateAxisRotate;
    private bool isCoolTime;
    
    public DownHorizontalMove(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        Debug.Log("4_Enter");
        moveCounter = 0;
        moveBoss = GameObject.Find("MoveBoss").GetComponent<MoveBoss>();
        boss = moveBoss.Boss;
        moveBoss.EnemyScript.StopAttck();
        isCoolTime = true;
        Initialization();
        finishMoving = false;
        finishRotating = false;
        startPos = boss.transform.position;
        rotateAxis = moveBoss.RotateAxis;
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
            if (diff < moveBoss.CoolTime)
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
        Rotate(rotateAxisRotate);
        if (finishMoving == true && finishRotating == true)
        {
            moveBoss.Change();
        }
    }

    public void Exit()
    {
        Debug.Log("4_Exit");
    }

    void Initialization()
    {
        startTime = Time.time;
        //startPos = boss.transform.position;
    }

    void Move(Vector3 endPos,float time)
    {
        float diff = Time.time - startTime;
        if (diff > time)
        {
            //Debug.Log("Change");
            Initialization();
            startPos = boss.transform.position;
            if (moveCounter == 0) { moveBoss.EnemyScript.ReStartAttck();}
            moveCounter++;
            return;
        }
        float rate = diff / time;
        boss.transform.position = Vector3.Lerp(startPos, endPos, rate);
    }

    void Rotate(Vector3 angles)
    {
        //Debug.Log("angle:"+rotateAxis.transform.rotation.eulerAngles);
        if (finishRotating == true)
        {
            return;
        }
        t += Time.deltaTime;
        rotateAxis.transform.Rotate(0, 0, angleZ * Time.deltaTime);
        if (t >= 2.0f)
        {
            rotateAxis.transform.eulerAngles = angles;
            finishRotating = true;
        }
    }
    
    void MoveUpCenter()
    {
        //Debug.Log("MoveDownCenter");
        if (finishMoving == true) {return;}
        
        switch (moveCounter)
        {
            case 0:
                Move(x = new Vector3(moveBoss.LeftCenterPos.x, moveBoss.UpCenterPos.y, 0), moveBoss.MoveTime);
                break;
            case 1:
                Move(x = new Vector3(moveBoss.RightCenterPos.x, moveBoss.UpCenterPos.y, 0), moveBoss.EnemyData.MoveVerticalTime * 2);
                break;
            case 2:
                Debug.Log("終了");
                Initialization();
                finishMoving = true;
                break;
        }
    }
}
