using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Scripts.UI;
using TMPro;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
using DG.Tweening;

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
            boss,
        }

        [SerializeField] private EnemyType enemyType;

        public int HP;
        [SerializeField] private DamageUI damageText;
        [SerializeField][JapaneseLabel("警告UI")] private BeforeAttack beforeAttackText;
        [SerializeField][JapaneseLabel("攻撃の〇秒前")] private float beforeAttackTime;
        [SerializeField][JapaneseLabel("警告マークの点滅間隔")] private float blinkDuration = 0.2f;
        [SerializeField][JapaneseLabel("Canvas")] private GameObject canvas;

        [SerializeField] [JapaneseLabel("弾を出す場所")] private GameObject shotObj;
        Player player => Player.Instance;
        private bool isfirst = true;
        [SerializeField] private GameObject Bullet;
        [SerializeField] private float BulletRate;
        private AudioSource audioSource;
        [SerializeField] private AudioClip ShotSound;
        [SerializeField] private AudioClip DamageSound;

        [SerializeField] private EnemySpawnManager enemySpawn;
        
        private Collider2D loopAreaCollider;

        private float minX, maxX, minY, maxY;

        private float enemySize = 0.5f;
        
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

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            Invoke("BeforeAttack", BulletRate - beforeAttackTime);
            Invoke("Attack", BulletRate);
            enemySpawn = GameObject.FindObjectOfType<EnemySpawnManager>();
            
            Bounds bounds = loopAreaCollider.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
            minY = bounds.min.y;
            maxY = bounds.max.y;

            if (shotObj == null)
            {
                shotObj = this.gameObject;
            }
        }

        private void Attack()
        {
            beforeAttackText.After();
            // プレイヤーの位置に応じて左右を向く
            Vector3 enemyPos = transform.position;
            Vector3 playerPos = player.transform.position;

            // プレイヤーが右にいれば右を向く、左にいれば左を向く（y軸回転）
            float targetYRotation = (playerPos.x > enemyPos.x) ? 0f : 180f;
            
            transform.DORotate(new Vector3(0f, targetYRotation, 0f), 0.3f, RotateMode.Fast);


            GameObject bullets = Instantiate(Bullet, shotObj.transform.position, Quaternion.identity);
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
            Invoke("BeforeAttack", BulletRate - beforeAttackTime);
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
                    if (pos.x > maxX) pos.x = maxX - enemySize;
                    else if (pos.x < minX) pos.x = minX + enemySize;

                    if (pos.y > maxY)
                    {
                        pos.y = maxY - enemySize;
                        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);
                    }
                    else if (pos.y < minY) pos.y = minY + enemySize;

                    transform.position = pos;
                }

            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Attack")
            {
                Debug.Log("当たった");
                HP--;
                // DamageText.enabled = true;
                // DamageText.text = "1";
                damageText.ShowDamage(1);
                audioSource.PlayOneShot(DamageSound);
            }

            else if (collision.gameObject.tag == "Bullet")
            {
                Debug.Log("当たった");
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                HP -= bullet.Damage;
                // DamageText.enabled = true;
                // DamageText.text = bullet.Damage.ToString();
                damageText.ShowDamage(bullet.Damage);
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
                    case EnemyType.boss:
                        enemySpawn.RemoveEnemy(this.gameObject.transform.parent.gameObject);
                        break;
                }
                //enemySpawn.RemoveEnemy(this.gameObject);
            }
        }

        private void BeforeAttack()
        {
            beforeAttackText.Warning(blinkDuration);
        }
    }
}