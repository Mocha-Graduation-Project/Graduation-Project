using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour
{
    public int HP;
    [SerializeField] private TextMeshProUGUI DamageText;
    Player player => Player.Instance;
    private bool isfirst = true;
    [SerializeField] private GameObject Bullet;
    [SerializeField] private float BulletRate;

    private void Start()
    {
        DamageText.enabled = false;
        Invoke("Attack", BulletRate);
    }
    private void Attack()
    {
        GameObject bullets = Instantiate(Bullet, transform.position, Quaternion.identity);
        EnemyBullet bullet = bullets.GetComponent<EnemyBullet>();
        bullet.SetPower(transform.position);
        Invoke("Attack", BulletRate);
    }
    private void Update()
    {
        if (!GetComponent<Renderer>().isVisible)
        {
            if (isfirst)
                isfirst = false;
            else
            {
                Vector3 pos = transform.position;
                if (pos.x < 0)
                    transform.position = new Vector3(8.5f, pos.y, pos.z);
                else
                    transform.position = new Vector3(-8.5f, pos.y, pos.z);
            }
          
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Attack")
        {
            Debug.Log("当たった");
            HP--;
            DamageText.enabled = true;
            DamageText.text = "1";
        }
            
        else if(collision.gameObject.tag =="Bullet")
        {
            Debug.Log("当たった");
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            HP -= bullet.Damage;
            DamageText.enabled = true;
            DamageText.text = bullet.Damage.ToString();
        }

        if (HP < 0)
            Destroy(this.gameObject);
    }
}
