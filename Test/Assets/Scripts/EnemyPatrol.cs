using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] PatrolEnemyData patrolEnemyData;
    [SerializeField] float speed;
    private float leftMax, rightMax, upMax, downMax;
    Vector2 movement;
    [SerializeField] private bool moveable;
    [SerializeField] private float waitTime = 0;
    private Collider2D loopAreaCollider;
    private float enemySize = 0.5f;
    private Vector3 basePos;
    
    private void Awake()
    {
        GameObject loopAreaObj = GameObject.FindWithTag("LoopArea");
        if (loopAreaObj != null)
        {
            loopAreaCollider = loopAreaObj.GetComponent<Collider2D>();
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
        Bounds bounds = loopAreaCollider.bounds;
        basePos = transform.position;
        
        leftMax = basePos.x - patrolEnemyData.LeftRange;
        rightMax = basePos.x + patrolEnemyData.RightRange;
        downMax = basePos.y - patrolEnemyData.DownRange;
        upMax = basePos.y + patrolEnemyData.UpRange;

        leftMax = bounds.min.x + enemySize < leftMax ? leftMax : bounds.min.x + enemySize;
        rightMax = bounds.max.x - enemySize > rightMax ? rightMax : bounds.max.x - enemySize;
        downMax = bounds.min.y + enemySize < downMax ? downMax : bounds.min.y + enemySize;
        upMax = bounds.max.y - enemySize > upMax ? upMax : bounds.max.y - enemySize;
        
        // Debug.Log("画面左:"+bounds.min.x+"画面右:"+bounds.max.x+"画面上:"+bounds.max.y+"画面下:"+bounds.min.y);
        // Debug.Log("left:"+leftMax+"right:"+rightMax+"up:"+upMax+"down:"+downMax);
        ;
        switch (patrolEnemyData.State)
        {
            case PatrolEnemyData.EnemyState.vertical:
                movement = Vector2.up;
                break;
            case PatrolEnemyData.EnemyState.horizontal:
                movement = Vector2.right;
                break;
            default:
                return;
        }
        
        moveable = true;
        if (waitTime <= 0)
        {
            waitTime = 1.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (moveable == true)
        {
            transform.Translate(movement * speed * Time.deltaTime);

            //端に着いたか調べる
            CheckEdge();
        }
    }

    void CheckEdge()
    {
        Vector3 pos = transform.position;
        
        switch (patrolEnemyData.State)
        {
            case PatrolEnemyData.EnemyState.vertical:
                if (transform.position.y <= downMax)
                {
                    pos.y = downMax;
                    moveable = false;
                    movement = Vector2.up;
                }
                else if (transform.position.y >= upMax)
                {
                    pos.y = upMax;
                    moveable = false;
                    movement = Vector2.down;
                }
                break;
            case PatrolEnemyData.EnemyState.horizontal:
                if (transform.position.x <= leftMax)
                {
                    pos.x = leftMax;
                    moveable = false;
                    movement =Vector2.right;
                }
                else if (transform.position.x >= rightMax)
                {
                    pos.x = rightMax;
                    moveable = false;
                    movement = Vector2.left;
                }
                break;
            default:
                return;
        }

        if (moveable == false)
        {
            Invoke("ChangeMoveable", waitTime);
        }
    }

    void ChangeMoveable()
    {
        moveable = !moveable;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //地面、もしくは壁に当たった時に折り返す(壁の場合は未実装)
        if(other.gameObject.CompareTag("Ground"))
        {
            moveable = false;
            movement *= -1;
            Invoke("ChangeMoveable", waitTime);
        }
    }
}
