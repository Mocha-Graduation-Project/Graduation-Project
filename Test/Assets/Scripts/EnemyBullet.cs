using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    Player player => Player.Instance;
    [SerializeField] private UnityEngine.Vector3 Power;

   
    public void SetPower(UnityEngine.Vector3 Pos)
    {
        float Angle = Mathf.Atan2(player.gameObject.transform.position.y-Pos.y, player.gameObject.transform.position.x-Pos.x);
        //Debug.Log(Angle);
        UnityEngine.Vector3 direction = new UnityEngine.Vector3(Mathf.Cos(Angle), Mathf.Sin(Angle), 0).normalized;
        Power = direction * 5f;
    }
    void Update()
    {
        transform.position += Power * Time.deltaTime;

        if (!GetComponent<Renderer>().isVisible)
        {
          Destroy(gameObject);  
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Player")
        {
            if (collision.TryGetComponent<PlayerStatus>(out PlayerStatus status))
            {
                status.Damage(1);
            }
            Destroy(this.gameObject);
        }

    }
}
