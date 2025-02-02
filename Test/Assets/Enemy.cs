using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour
{
    public int HP;
    [SerializeField] private float Speed;
    Player player => Player.Instance;
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Speed);
        if (!GetComponent<Renderer>().isVisible)
        {
            Vector3 pos = transform.position;
            if (pos.x < 0)
                transform.position = new Vector3(8.5f, pos.y, pos.z);
            else
                transform.position = new Vector3(-8.5f, pos.y, pos.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Attack")
            HP--;
        else if(collision.gameObject.tag =="Bullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            HP -= bullet.Damage;
        }

        if (HP < 0)
            Destroy(this.gameObject);
    }
}
