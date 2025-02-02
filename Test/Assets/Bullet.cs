using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private UnityEngine.Vector3 Power;
    Player player => Player.Instance;
    public float PowerDirection;
    private int count = 1;
    private bool isAttack = false;
    public int Damage = 1;
    private bool isReflection = false;

    private void Start()
    {
        Power *= PowerDirection;
    }
    void Update()
    {
        transform.position += Power*Time.deltaTime;

        if (!GetComponent<Renderer>().isVisible)
        {
            if(count >= 1)
            {
                count--;
                return;
            }
            UnityEngine.Vector3 pos = transform.position;
            if (pos.x < 0)
                transform.position = new UnityEngine.Vector3(8.8f, pos.y, pos.z);
            else
                transform.position = new UnityEngine.Vector3(-8.8f, pos.y, pos.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Attack")
        {
            player.isMove = false;
            player.Arrow.SetActive(true);
            isAttack = true;
            Time.timeScale = 0.2f;
            Power = UnityEngine.Vector3.zero;
            Invoke("Attack", 0.1f);

        }
        else if(collision.gameObject.tag !="Player"||!isAttack)
        {
            if(!isReflection)
            {
                Destroy(this.gameObject);
                Time.timeScale = 1f;
                player.isMove = true;
                isAttack = false;
            }
           
        }
           
       
    }

    private void Attack()
    {
        Damage++;
        isReflection = true;
        player.BulletTime -= 2.5f;
        player.Arrow.SetActive(false);
        player.isMove = true;
        isAttack = false;
        PowerDirection *= 1.2f;
        if (PowerDirection < 0)
            PowerDirection *= -1;
        float Angle = Mathf.Atan2(player.InputMove.y, player.InputMove.x);
        UnityEngine.Vector3 direction = new UnityEngine.Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0);
        Power = direction * PowerDirection * 10f;
        Debug.Log(direction);
        Time.timeScale =1f;
    }
}
