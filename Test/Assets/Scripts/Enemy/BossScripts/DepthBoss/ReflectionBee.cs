using System.Collections;
using Player;
using UnityEngine;

namespace Enemy.BossScripts.DepthBoss
{
    public class ReflectionBee : MonoBehaviour
    {
        //一定ダメージ受けると機能停止して弾を通すようにして、一定時間後に元に戻す
        [SerializeField] [JapaneseLabel("盾のHP")]
        private float shieldMaxHP;

        [SerializeField] [JapaneseLabel("盾の現在HP")]
        private float shieldHP;

        [SerializeField] [JapaneseLabel("復活までの時間")]
        private float revivaltime;

        [SerializeField] private Animator animator;
        bool dead = false;
        Rigidbody rb;
        private Collider col;
        [SerializeField] [JapaneseLabel("地面レイヤー")]
        public LayerMask groundLayer;
        private void Start()
        {
            shieldHP = shieldMaxHP;
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }

        private void Update()
        {
            dead = animator.GetBool("Defeat");
            if (dead)
            {
                rb.useGravity = true;
                Destroy(col);
                //StartCoroutine(FryEnemyDeathAnimation(gameObject));
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out Bullet bullet))
            {
                var incomingPower = bullet.GetPower();
                var speed = incomingPower.magnitude;

                var normal = transform.up.normalized;

                // Vector3.Reflectで反射ベクトルを求める
                var reflectedDirection = Vector3.Reflect(incomingPower.normalized, normal);

                // Bulletに新しい方向とスピードを設定
                bullet.SetDirection(reflectedDirection);
                bullet.SetSpeed(speed);
                bullet.UpdatePower();

                bullet.OnReflect();

                shieldHP -= bullet.Damage;
                if (shieldHP <= 0)
                {
                    shieldHP = 0;
                    gameObject.SetActive(false);
                    animator.SetBool("Stag", true);
                    Invoke("ShieldReset", revivaltime);
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
        private void ShieldReset()
        {
            shieldHP = shieldMaxHP;
            gameObject.SetActive(true);
            animator.SetBool("Stag", false);
        }
    }
}