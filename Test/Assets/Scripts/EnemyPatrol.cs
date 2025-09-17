using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] PatrolEnemyData patrolEnemyData;
    private float leftMax, rightMax, upMax, downMax;
    [SerializeField] private bool moveable;
    private Collider loopAreaCollider;
    private float enemySize = 1.0f;
    private Vector3 basePos;
    float startTime;
    private bool isMoveRightOrUp;
    private float moveTime;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 endPos;
    
    private void Awake()
    {
        GameObject loopAreaObj = GameObject.FindWithTag("LoopArea");
        if (loopAreaObj != null)
        {
            loopAreaCollider = loopAreaObj.GetComponent<Collider>();
        }
        else
        {
            Debug.LogError("LoopAreaColliderが見つかりません。LoopAreaタグを持つGameObjectを配置してください。");
            return;
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
        Bounds bounds = loopAreaCollider.bounds;
        basePos = transform.position;
        //Debug.Log("basePos:"+basePos);
        
        leftMax = basePos.x - patrolEnemyData.LeftRange;
        rightMax = basePos.x + patrolEnemyData.RightRange;
        downMax = basePos.y - patrolEnemyData.DownRange;
        upMax = basePos.y + patrolEnemyData.UpRange;
        // Debug.Log("画面左:"+bounds.min.x+"画面右:"+bounds.max.x+"画面上:"+bounds.max.y+"画面下:"+bounds.min.y);
        // Debug.Log("left:"+leftMax+"right:"+rightMax+"up:"+upMax+"down:"+downMax);

        leftMax = bounds.min.x + enemySize < leftMax ? leftMax : bounds.min.x + enemySize;
        rightMax = bounds.max.x - enemySize > rightMax ? rightMax : bounds.max.x - enemySize;
        downMax = bounds.min.y + enemySize < downMax ? downMax : bounds.min.y + enemySize;
        upMax = bounds.max.y - enemySize > upMax ? upMax : bounds.max.y - enemySize;
        
        //Debug.Log("left:"+leftMax+"right:"+rightMax+"up:"+upMax+"down:"+downMax);
        
        startPos = basePos;
        
        switch (patrolEnemyData.State)
        {
            case PatrolEnemyData.EnemyState.vertical:
                endPos = new Vector3(basePos.x, upMax, basePos.z);
                moveTime = patrolEnemyData.MoveVerticalTime;
                break;
            case PatrolEnemyData.EnemyState.horizontal:
                endPos = new Vector3(rightMax, basePos.y, basePos.z);
                moveTime = patrolEnemyData.MoveHorizontalTime;
                break;
            case PatrolEnemyData.EnemyState.none:
                break;
        }

        isMoveRightOrUp = true;
        moveable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveable == true)
        {
            Move(startPos, endPos, moveTime);
        }
    }

    void Move(Vector3 start, Vector3 end, float time)
    {
        float diff = Time.time - startTime;
        if (diff > time)
        {
            NextMove();
            return;
        }
        float rate = diff / time;
        transform.position = Vector3.Lerp(start, end, rate);
    }

    void NextMove()
    {
        switch (patrolEnemyData.State)
        {
            case PatrolEnemyData.EnemyState.vertical:
                startPos = transform.position;
                moveTime = patrolEnemyData.MoveVerticalTime * 2;
                if (isMoveRightOrUp == true)
                {
                    endPos = new Vector3(startPos.x, downMax, startPos.z);
                }
                else
                {
                    endPos = new Vector3(basePos.x, upMax, basePos.z);
                }
                break;
            case PatrolEnemyData.EnemyState.horizontal:
                startPos = transform.position;
                moveTime = patrolEnemyData.MoveHorizontalTime * 2;
                if (isMoveRightOrUp == true)
                {
                    endPos = new Vector3(leftMax, basePos.y, basePos.z);
                }
                else
                {
                    endPos = new Vector3(rightMax, basePos.y, basePos.z);
                }
                break;
            case PatrolEnemyData.EnemyState.none:
                break;
        }
        moveable = false;
        isMoveRightOrUp = !isMoveRightOrUp;
        Invoke("ChangeMoveable", patrolEnemyData.WaitTime);
    }
    
    void Initialization()
    {
        startTime = Time.time;
    }

    void ChangeMoveable()
    {
        moveable = !moveable;
        Initialization();
    }
}
