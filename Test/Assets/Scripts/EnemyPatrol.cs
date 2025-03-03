using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] PatrolEnemyData patrolEnemyData;
    [SerializeField] float speed;
    private float minPos;
    private float maxPos;
    Vector2 movement;
    [SerializeField] private bool moveable;
    [SerializeField] private float waitTime = 0;
    [SerializeField] CameraAreaManager cameraAreaManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraAreaManager = GameObject.FindObjectOfType<CameraAreaManager>();
        
        switch (patrolEnemyData.State)
        {
            case PatrolEnemyData.EnemyState.vertical:
                //minPos = patrolEnemyData.MinVerticalPos;
                //maxPos = patrolEnemyData.MaxVerticalPos;
                minPos = cameraAreaManager.DownMax;
                maxPos = cameraAreaManager.UpMax;
                movement = Vector2.up;
                break;
            case PatrolEnemyData.EnemyState.horizontal:
                // minPos = patrolEnemyData.MinHorizontalPos;
                // maxPos = patrolEnemyData.MaxHorizontalPos;
                minPos = cameraAreaManager.LeftMax;
                maxPos = cameraAreaManager.RightMax;
                movement = Vector2.right;
                break;
            default:
                return;
        }
        Debug.Log(this.name+":"+minPos+":"+maxPos);
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
        switch (patrolEnemyData.State)
        {
            case PatrolEnemyData.EnemyState.vertical:
                if (transform.position.y <= minPos)
                {
                    transform.position = new Vector2(transform.position.x, minPos);
                    moveable = false;
                    movement = Vector2.up;
                }
                else if (transform.position.y >= maxPos)
                {
                    transform.position = new Vector2(transform.position.x, maxPos);
                    moveable = false;
                    movement = Vector2.down;
                }
                break;
            case PatrolEnemyData.EnemyState.horizontal:
                if (transform.position.x <= minPos)
                {
                    transform.position = new Vector2(minPos, transform.position.y);
                    moveable = false;
                    movement =Vector2.right;
                }
                else if (transform.position.x >= maxPos)
                {
                    transform.position = new Vector2(maxPos, transform.position.y);
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
