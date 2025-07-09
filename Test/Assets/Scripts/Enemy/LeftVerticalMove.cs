using UnityEngine;

public class LeftVerticalMove : MonoBehaviour,IState
{
    //右の縦移動
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
    
    public LeftVerticalMove(EnemyAI enemyAI)
    {
        this.enemyAI = enemyAI;
    }

    public void Enter()
    {
        Debug.Log("3_Enter");
        moveCounter = 0;
        moveBoss = GameObject.Find("MoveBoss").GetComponent<MoveBoss>();
        rotateAxis = moveBoss.RotateAxis;
        boss = moveBoss.Boss;
        Initialization();
        finishMoving = false;
        finishRotating = false;
        startPos = boss.transform.position;
    }

    public void Execute()
    {
        //Debug.Log("3_Execute");
        MoveLeftCenter();
        Rotate(new Vector3(0, 0, 0));
        if (finishMoving == true && finishRotating == true)
        {
            moveBoss.Change();
        }
    }

    public void Exit()
    {
        Debug.Log("3_Exit");
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
            moveCounter++;
            return;
        }
        float rate = diff / time;
        boss.transform.position = Vector3.Lerp(startPos, endPos, rate);
    }

    void Rotate(Vector3 angles)
    {
        //Debug.Log("angle:"+rotateAxis.transform.rotation.eulerAngles);
        if (finishRotating == true || rotateAxis.transform.eulerAngles == angles)
        {
            finishRotating = true;
            return;
        }

        t += Time.deltaTime;
        rotateAxis.transform.Rotate(0, 0, 45 * Time.deltaTime);
        if (t >= 2.0f)
        {
            rotateAxis.transform.eulerAngles = angles;
            finishRotating = true;
        }
    }

    void MoveLeftCenter()
    {
        //Debug.Log("MoveRightCenter");
        if (finishMoving == true) {return;}
        
        switch (moveCounter)
        {
            case 0:
                Move(moveBoss.LeftCenterPos, moveBoss.MoveTime);
                break;
            case 1:
                Move(x = new Vector3(moveBoss.LeftCenterPos.x, moveBoss.UpCenterPos.y, 0), moveBoss.MoveTime);
                break;
            case 2:
                Move(x = new Vector3(moveBoss.LeftCenterPos.x, moveBoss.DownCenterPos.y, 0), moveBoss.MoveTime * 2);
                break;
            case 3:
                Move(moveBoss.LeftCenterPos, moveBoss.MoveTime);
                break;
            case 4:
                Debug.Log("終了");
                Initialization();
                finishMoving = true;
                break;
        }
    }
}
