using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using Scripts.UI;
#if UNITY_EDITOR
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
#endif
namespace Scripts
{


    public class NoBulletEnemyEnemy : MonoBehaviour
    {
        public int HP;
        
        Player player => Player.Instance;
        private bool isfirst = true;
        private AudioSource audioSource;
        [SerializeField] private AudioClip DamageSound;

        [SerializeField] private EnemySpawnManager enemySpawn;

        [SerializeField] private DamageUI damageText;
        // [SerializeField][JapaneseLabel("警告UI")] private BeforeAttack beforeAttackText;
        // [SerializeField][JapaneseLabel("攻撃の〇秒前")] private float beforeAttackTime;


        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            enemySpawn = GameObject.FindObjectOfType<EnemySpawnManager>();
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
            //if (collision.gameObject.tag == "Attack")
            //{
            //    Debug.Log("当たった");
            //    HP--;
            //    damageText.ShowDamage(1);
            //    audioSource.PlayOneShot(DamageSound);
            //}

            if (collision.gameObject.tag == "Bullet")
            {
                Debug.Log("当たった");
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                HP -= bullet.Damage;
                damageText.ShowDamage(bullet.Damage);
                audioSource.PlayOneShot(DamageSound);
            }

            if (HP < 0)
            {
                enemySpawn.RemoveEnemy(this.gameObject);
                //Destroy(this.gameObject);
            }
        }
    }
}