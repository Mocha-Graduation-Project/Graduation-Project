using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.VFX;

namespace Enemy.BossScripts.DepthBoss
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ReflectionBee : MonoBehaviour
    {
        //一定ダメージ受けると機能停止して弾を通すようにして、一定時間後に元に戻す
        [SerializeField] [JapaneseLabel("盾のHP")]
        private float shieldMaxHP;

        [SerializeField] [JapaneseLabel("盾の現在HP")]
        private float shieldHP;

        [SerializeField] [JapaneseLabel("復活までの時間")]
        private float revivaltime;

        [SerializeField] [JapaneseLabel("撃破時の爆発エフェクト")]
        private GameObject breakBombEffect;

        [SerializeField] private Animator animator;
        private bool dead = false;
        private bool isDeathProcessingStarted = false;
        private Rigidbody rb;
        private Collider col;
        private bool isShieldDown = false;
        [SerializeField] [JapaneseLabel("地面レイヤー")]
        public LayerMask groundLayer;

        public bool IsShieldDown => isShieldDown;

        private void Start()
        {
            shieldHP = shieldMaxHP;
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }

        private void Update()
        {
            // 既に死亡処理が始まっているか、盾がダウン中ならUpdate処理をスキップ
            if (isDeathProcessingStarted || isShieldDown) return;

            dead = animator.GetBool("Defeat");

            // deadがtrueになり、まだ死亡処理が開始されていない場合
            if (dead && !isDeathProcessingStarted)
            {
                isDeathProcessingStarted = true; // 死亡処理フラグを立てる

                // 盾のダウン処理が動いている可能性があるので停止する
                if (isShieldDown)
                {
                    StopAllCoroutines();
                }

                rb.useGravity = true;
                //Destroy(col);

                // 死亡アニメーションを開始
                StartCoroutine(FryEnemyDeathAnimation(gameObject));
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out Bullet bullet))
            {
                // 壁の法線ベクトル (このオブジェクトの「上」方向) を渡す
                Vector3 normal = transform.up; 

                // BulletのReflectFromWall関数を呼び出し、反射が成功したかを受け取る
                bool didReflect = bullet.ReflectFromWall(normal);
                
                // 反射に成功した場合（クールダウン中でなかった場合）のみ、HPを減らす
                if (didReflect)
                {
                    shieldHP -= bullet.Damage;
                    
                    //特定の時間盾を無効化する
                    if (shieldHP <= 0)
                    {
                        StartCoroutine(ShieldDownCoroutine());
                    }
                }
            }
        }
        private IEnumerator FryEnemyDeathAnimation(GameObject enemy)
        {
            var duration = 1.5f; // 演出にかける時間
            var startTime = Time.time;
            var startPosition = enemy.transform.position;

            var currentRotationSpeed = 360f * 0.1f;

            var targetY = startPosition.y - 100f;
            var rayDistance = 200f;
            const float OFFSET_TO_BOTTOM = 1f;

            RaycastHit hit;
            if (Physics.Raycast(startPosition, Vector3.down, out hit, rayDistance, groundLayer))
                targetY = hit.point.y + OFFSET_TO_BOTTOM;
            var enemyCollider = enemy.GetComponent<Collider>();
            if (enemyCollider != null) enemyCollider.enabled = false;
            var isGrounded = false;

            while (Time.time < startTime + duration)
            {
                var elapsed = Time.time - startTime;
                var progress = elapsed / duration;
                // 回転
                if (!isGrounded)
                {
                    enemy.transform.Rotate(0, 0, currentRotationSpeed * Time.deltaTime, Space.Self);
                }

                // 落下位置
                var dropAmount = 5f;
                var newPosition = startPosition + new Vector3(
                    0,
                    Mathf.Lerp(0, -dropAmount, progress * progress),
                    0
                );

                // 地面到達チェックと制限
                if (newPosition.y <= targetY)
                {
                    newPosition.y = targetY; // 地面より下にいかないように固定

                    if (!isGrounded)
                    {
                        currentRotationSpeed = 0f;
                        isGrounded = true;
                    }
                }

                enemy.transform.position = newPosition;

                // 演出時間の調整
                //if (isGrounded && Time.time > startTime + 0.5f) break;

                yield return null;
            }

            // 演出終了後、最終的な位置を地面に固定
            enemy.transform.position = new Vector3(
                enemy.transform.position.x,
                targetY,
                enemy.transform.position.z
            );
        }
        private IEnumerator ShieldDownCoroutine()
        {
            isShieldDown = true; // ダウン状態に設定
            shieldHP = 0;
            breakBombEffect.GetComponent<VisualEffect>().SendEvent("OnPlay");
            animator.SetBool("Stag", true);
            col.enabled = false; // ★コライダーを無効化し、弾が当たらないようにする

            // 復活時間待機
            yield return new WaitForSeconds(revivaltime);

            // 復活処理 (元のShieldResetの処理)
            shieldHP = shieldMaxHP;
            animator.SetBool("Stag", false);
            col.enabled = true; // ★コライダーを再度有効化
            isShieldDown = false; // ダウン状態を解除
        }
    }
}