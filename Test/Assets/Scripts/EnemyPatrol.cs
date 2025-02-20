using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyPatrol : MonoBehaviour
{
    [FormerlySerializedAs("enemyData")] [SerializeField] PatrolEnemyData patrolEnemyData;
    [SerializeField] float speed;
    private float minPos;
    private float maxPos;
    Vector2 movement;
    [SerializeField] private bool moveable;
    [SerializeField] private float waitTime = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (patrolEnemyData.State)
        {
            case PatrolEnemyData.EnemyState.vertical:
                minPos = patrolEnemyData.MinVerticalPos;
                maxPos = patrolEnemyData.MaxVerticalPos;
                movement = Vector2.up;
                break;
            case PatrolEnemyData.EnemyState.horizontal:
                minPos = patrolEnemyData.MinHorizontalPos;
                maxPos = patrolEnemyData.MaxHorizontalPos;
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
}
