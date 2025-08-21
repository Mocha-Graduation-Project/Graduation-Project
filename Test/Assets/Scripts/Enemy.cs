using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Scripts.UI;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
#endif
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

        enum AttckType
        {
            playerAim,
            straight,
        }
        [SerializeField]  private AttckType attckType;
        
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
        [SerializeField] [JapaneseLabel("ストレートの参照オブジェ")] private GameObject straightObj;
        
        Player player => Player.Instance;
        private bool isfirst = true;

        [SerializeField] private bool dontAttck;
        [SerializeField] [JapaneseLabel("現在の発射する弾")] private GameObject bullet;
        [SerializeField] private GameObject enemyBullet;
        [SerializeField] private GameObject reflectionBullet;
        [SerializeField] private float bulletRate;
        private AudioSource audioSource;
        [SerializeField] private AudioClip ShotSound;
        [SerializeField] private AudioClip DamageSound;

        [SerializeField] private EnemySpawnManager enemySpawn;

        [SerializeField] [JapaneseLabel("敵のHPバー")] private Slider enemyHPSlider;
        
        private Collider loopAreaCollider;

        private float minX, maxX, minY, maxY;

        private float enemySize = 0.5f;
        
        public float BulletRate { get => bulletRate; set => bulletRate = value; }
        
        private void Awake()
        {
            GameObject loopAreaObj = GameObject.FindWithTag("LoopArea");
            if (loopAreaObj != null)
            {
                loopAreaCollider = loopAreaObj.GetComponent<Collider>();
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
            if (dontAttck == false)
            {
                Invoke("BeforeAttack", bulletRate - beforeAttackTime);
                Invoke("Attack", bulletRate);
            }
            enemySpawn = GameObject.FindObjectOfType<EnemySpawnManager>();
            
            Bounds bounds = loopAreaCollider.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
            minY = bounds.min.y;
            maxY = bounds.max.y;

            if (shotObj == null)
            {
                shotObj = this.gameObject;
                straightObj = shotObj;
            }
            else
            {
                straightObj = shotObj.transform.parent.gameObject;
            }

            switch (bulletType)
            {
                case BulletType.enemyBullet:
                    bullet = enemyBullet;
                    break;
                case BulletType.reflectionBullet:
                    bullet = reflectionBullet;
                    break;
            }

            switch (enemyType)
            {
                case EnemyType.boss:
                    enemyHPSlider = GameObject.FindWithTag("EnemyHPBar").GetComponent<Slider>();
                    enemyHPSlider.maxValue = HP;
                    enemyHPSlider.value = HP;
                    break;
            }
        }

        private void Attack()
        {
            if (dontAttck == true) { return; }
            
            beforeAttackText.After();
            // // プレイヤーの位置に応じて左右を向く
            // Vector3 enemyPos = transform.position;
            // Vector3 playerPos = player.transform.position;
            //
            // // プレイヤーが右にいれば右を向く、左にいれば左を向く（y軸回転）
            // float targetYRotation = (playerPos.x > enemyPos.x) ? 0f : 180f;
            //
            // transform.DORotate(new Vector3(0f, targetYRotation, 0f), 0.3f, RotateMode.Fast);


            GameObject bullets = Instantiate(bullet, shotObj.transform.position, Quaternion.identity);
            switch (bulletType)
            {
                case BulletType.enemyBullet:
                    EnemyBullet bullet = bullets.GetComponent<EnemyBullet>();
                    if (attckType == AttckType.playerAim)
                    {
                        bullet.SetPower(transform.position);
                    }
                    else if (attckType == AttckType.straight)
                    {
                        bullet.SetStraightPower(straightObj.transform.rotation.eulerAngles);
                    }
                    break;
                case BulletType.reflectionBullet:
                    Bullet reflectionBullet = bullets.GetComponent<Bullet>();
                    if (attckType == AttckType.playerAim)
                    {
                        reflectionBullet.SetPowerEnemy(transform.position);
                    }
                    else if (attckType == AttckType.straight)
                    {
                        reflectionBullet.SetStraightPowerEnemy(straightObj.transform.rotation.eulerAngles);
                    }
                    break;
            }
            audioSource.PlayOneShot(ShotSound);
            Invoke("BeforeAttack", bulletRate - beforeAttackTime);
            Invoke("Attack", bulletRate);
        }

        private void Update()
        { 
            if (player != null)
            {
                // DOLookAt(ターゲットの位置, 回転にかける時間)
                transform.DOLookAt(player.transform.position, 0.5f);
            }
            
            if (!GetComponent<Renderer>().isVisible)
            {
                if (isfirst)
                    isfirst = false;
                else
                {
                    Vector3 pos = transform.position;
                    if (pos.x > maxX)
                    {
                        pos.x = maxX - enemySize;
                    }
                    else if (pos.x < minX)
                    {
                        pos.x = minX + enemySize;
                    }

                    if (pos.y > maxY)
                    {
                        pos.y = maxY - enemySize;
                        GetComponent<Rigidbody>().linearVelocity = new Vector2(0, 0);
                    }
                    else if (pos.y < minY) pos.y = minY + enemySize;

                    transform.position = pos;
                }

            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == "Bullet")
            {
                Debug.Log("当たった");
                Bullet bullet = collider.gameObject.GetComponent<Bullet>();
                HP -= bullet.Damage;
                
                if (enemyHPSlider != null)
                {
                    enemyHPSlider.value = HP;
                }
                // DamageText.enabled = true;
                // DamageText.text = bullet.Damage.ToString();
                damageText.ShowDamage(bullet.Damage);
                audioSource.PlayOneShot(DamageSound);
            }

            if (HP <= 0)
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
            }
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            //if (collision.gameObject.tag == "Attack")
            //{
            //    Debug.Log("当たった");
            //    HP--;
            //    DamageText.enabled = true;
            //    DamageText.text = "1";
            //    damageText.ShowDamage(1);
            //    audioSource.PlayOneShot(DamageSound);
            //}

            if (collision.gameObject.tag == "Bullet")
            {
                Debug.Log("当たった");
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                HP -= bullet.Damage;
                
                if (enemyHPSlider != null)
                {
                    enemyHPSlider.value = HP;
                }
                // DamageText.enabled = true;
                // DamageText.text = bullet.Damage.ToString();
                damageText.ShowDamage(bullet.Damage);
                audioSource.PlayOneShot(DamageSound);
            }

            if (HP <= 0)
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
            if (dontAttck == true) { return; }
            
            beforeAttackText.Warning(blinkDuration);
        }

        public void StopAttck()
        {
            dontAttck = true;
            CancelInvoke();
            beforeAttackText.After();
        }

        public void ReStartAttck()
        {
            dontAttck = false;
            Invoke("BeforeAttack", bulletRate - beforeAttackTime);
            Invoke("Attack", bulletRate);
        }
    }
}