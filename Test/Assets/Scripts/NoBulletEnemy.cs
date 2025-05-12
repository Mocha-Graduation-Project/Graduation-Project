using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

namespace Scripts
{


    public class NoBulletEnemyEnemy : MonoBehaviour
    {
        public int HP;
        [SerializeField] private TextMeshProUGUI DamageText;
        Player player => Player.Instance;
        private bool isfirst = true;
        private AudioSource audioSource;
        [SerializeField] private AudioClip DamageSound;

        [SerializeField] private EnemySpawnManager enemySpawn;

        private void Start()
        {
            DamageText.enabled = false;
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
                enemySpawn.RemoveEnemy(this.gameObject);
                //Destroy(this.gameObject);
            }
        }
    }
}