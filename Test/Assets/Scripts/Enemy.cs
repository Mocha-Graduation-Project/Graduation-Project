using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

namespace Scripts
{


    public class Enemy : MonoBehaviour
    {
        enum BulletType
        {
            enemyBullet,
            reflectionBullet,
        }
        [SerializeField] private BulletType bulletType;
        
        enum EnemyType
        {
            normal,
            shield,
        }

        [SerializeField] private EnemyType enemyType;

        public int HP;
        [SerializeField] private TextMeshProUGUI DamageText;
        Player player => Player.Instance;
        private bool isfirst = true;
        [SerializeField] private GameObject Bullet;
        [SerializeField] private float BulletRate;
        private AudioSource audioSource;
        [SerializeField] private AudioClip ShotSound;
        [SerializeField] private AudioClip DamageSound;

        [SerializeField] private EnemySpawnManager enemySpawn;

        private void Start()
        {
            DamageText.enabled = false;
            audioSource = GetComponent<AudioSource>();
            Invoke("Attack", BulletRate);
            enemySpawn = GameObject.FindObjectOfType<EnemySpawnManager>();
        }

        private void Attack()
        {
            GameObject bullets = Instantiate(Bullet, transform.position, Quaternion.identity);
            switch (bulletType)
            {
                case BulletType.enemyBullet:
                    EnemyBullet bullet = bullets.GetComponent<EnemyBullet>();
                    bullet.SetPower(transform.position);
                    break;
                case BulletType.reflectionBullet:
                    Bullet reflectionBullet = bullets.GetComponent<Bullet>();
                    reflectionBullet.SetPowerEnemy(transform.position);
                    break;
            }
            audioSource.PlayOneShot(ShotSound);
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
            if (collision.gameObject.tag == "Attack")
            {
                Debug.Log("当たった");
                HP--;
                DamageText.enabled = true;
                DamageText.text = "1";
                audioSource.PlayOneShot(DamageSound);
            }

            else if (collision.gameObject.tag == "Bullet")
            {
                Debug.Log("当たった");
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                HP -= bullet.Damage;
                DamageText.enabled = true;
                DamageText.text = bullet.Damage.ToString();
                audioSource.PlayOneShot(DamageSound);
            }

            if (HP < 0)
            {
                switch (enemyType)
                {
                    case EnemyType.normal:
                        enemySpawn.RemoveEnemy(this.gameObject);
                        break;
                    case EnemyType.shield:
                        enemySpawn.RemoveEnemy(this.gameObject.transform.parent.gameObject);
                        break;
                }
                //enemySpawn.RemoveEnemy(this.gameObject);
            }
        }
    }
}