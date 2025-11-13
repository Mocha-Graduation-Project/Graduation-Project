using System;
using System.Collections;
using Scriptable;
using Scripts.Scriptable;
using UnityEngine;
using UnityEngine.InputSystem;

//using System.Numerics;

namespace Player
{
    public class Bullet : MonoBehaviour
    {
        private static readonly int PowerLevel = Shader.PropertyToID("_PowerLevel");
        [SerializeField] private SoundData soundData;

        [SerializeField] private MeshRenderer meshRendererChild;
        [SerializeField] private CharacterParams characterParams;

        [SerializeField] private Renderer trailRenderer;
        [JapaneseLabel("弾くたびに＋〇〇速度を追加")] private float addSpeed;
        [NonSerialized] public Transform arrowTransform;

        private readonly float attackCoolMaxTime = 1f;
        private float attackCoolTime;
        private AudioSource audioSource;
        private int count = 1;
        [JapaneseLabel("現在の移動方向")] private Vector3 currentDirection = Vector3.right;

        [JapaneseLabel("現在の速度")] private float currentSpeed;

        [NonSerialized] [JapaneseLabel("初期ダメージ値")]
        public int Damage = 1;

        [JapaneseLabel("1回目〇ダメージ、2回目〇ダメージ...")]
        private int[] damageByReflectionCount;

        private bool destroyed; //Destroyで消してもAttckに反応することがあるので仮で配置、バグ治せれば消す
        [JapaneseLabel("敵のレイヤー")] private int enemyLayer;
        [JapaneseLabel("ヒットストップ時間")] private float hitStopDuration;
        private bool isAttack;
        private bool isPaused;
        private bool isQuick;

        [JapaneseLabel("反射処理中かどうか")] [NonSerialized]
        public bool isReflecting;

        private Vector2 lastInputDirection = Vector2.right;

        private Material material;
        [JapaneseLabel("最大スピード")] private float maxBulletSpeed;
        private readonly int maxReflectionCount = 4;

        private PlayerInput moveAction;
        private Vector3 power;
        [NonSerialized] public float PowerDirection;
        [NonSerialized] public Vector2 quickAttackDirectionInput = Vector2.zero;
        private Rigidbody rb;

        [JapaneseLabel("反射クールタイムの秒数")] private float reflectCooldown = 0.5f;

        [JapaneseLabel("反射クールタイムのタイマー")] [NonSerialized]
        private float reflectCooldownTimer;

        [JapaneseLabel("反射後の無敵時間")]　private float reflectInvincible = 1;
        [NonSerialized] public int reflectionCount;
        private AudioClip reflectionSound;
        private Vector2 savedQuickDirection;

        private Vector3 SavePower;

        private float staminaDrainPerSecond;
        private AudioClip wallReflectionSound;
        private Player player => Player.Instance;
        private PlayerStatus pStatus => PlayerStatus.Instance;

        private void Awake()
        {
            PlayerParamReset();
            rb = GetComponent<Rigidbody>(); // Rigidbodyを取得
            if (rb != null) rb.isKinematic = true;
            audioSource = GetComponent<AudioSource>();
            reflectionSound = soundData.reflectionSound;
            wallReflectionSound = soundData.wallSound;
        }

        private void Start()
        {
            if (gameObject.CompareTag("EnemyBullet"))
                currentDirection = power * PowerDirection;
            else
                currentDirection = new Vector3(player.direction, 0, 0).normalized;

            //currentDirection = characterParams.power.normalized * PowerDirection;
            currentSpeed = characterParams.power.magnitude;
            arrowTransform = player.Arrow.transform;
            UpdatePower();

            reflectionCount = 0;
            //moveAction = GetComponent<PlayerInput>();
            // moveAction.actions["Attack"].canceled += OffAttack;
            enemyLayer = LayerMask.NameToLayer("Enemy");
        }

        private void FixedUpdate()
        {
            if (!isPaused)
            {
                //return;
                // 最大スピード制限
                currentSpeed = Mathf.Min(currentSpeed, maxBulletSpeed);

                // 移動
                //transform.position += currentDirection * (currentSpeed * Time.deltaTime);
                if (rb != null)
                {
                    // 現在の位置 + (方向 * 速度 * 時間) を計算
                    var newPosition = rb.position + currentDirection * (currentSpeed * Time.deltaTime);
                    rb.MovePosition(newPosition);
                }
                else
                {
                    // Rigidbodyがない場合のフォールバック（従来の処理）
                    transform.position += currentDirection * (currentSpeed * Time.deltaTime);
                }
            }

            if (reflectCooldownTimer > 0)
            {
                // Time.timeScaleの影響を受けない unscaledDeltaTime を使うことを推奨
                reflectCooldownTimer -= Time.unscaledDeltaTime;
                if (reflectCooldownTimer <= 0)
                    // クールタイムが終了したら、次の反射を許可
                    isReflecting = false;
            }


            // 回転
            if (currentDirection != Vector3.zero)
            {
                var angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            var input = player.InputMove;

            // 入力があれば更新、なければ前回の方向を維持
            if (input.sqrMagnitude > 0.01f) lastInputDirection = input.normalized;

            if (isQuick)
                //player.currentStamina -= quickStaminaDrainPerSecond * Time.deltaTime * 5;
                if (player.PlayerCombat.currentStamina <= 0)
                {
                    player.PlayerCombat.currentStamina = 0;
                    player.PlayerCombat.AttackFinish();
                    if (attackCoolTime > attackCoolMaxTime)
                        //QuickAttack(); 
                        attackCoolTime = 0;
                }

            if (isAttack && arrowTransform != null)
            {
                // 矢印の方向ベクトルを取得
                var dir = arrowTransform.right; // 右方向が矢印の先なら .right、上方向なら .up

                // currentDirectionを矢印方向に更新
                currentDirection = dir.normalized;

                // 回転も矢印の回転に合わせる（オプション）
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }

            attackCoolTime += Time.deltaTime;
            player.staminaSlider.value = player.PlayerCombat.currentStamina;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag("Ground") && !isAttack)
            {
                if (collider.gameObject.layer == LayerMask.NameToLayer("FloatFloor"))
                    return;

                //ResetBullet();
                //Time.timeScale = 1f;
                player.isMove = true;
                isAttack = false;
                destroyed = true;
                Destroy(gameObject);
            }

            if (collider.gameObject.CompareTag("Player") && !isAttack)
            {
                if (!CompareTag("EnemyBullet")) return;
                if (collider.TryGetComponent(out PlayerStatus status)) status.Damage(1);

                //ResetBullet();
                //Time.timeScale = 1f;
                player.isMove = true;
                isAttack = false;
                destroyed = true;
                Destroy(gameObject);
            }

            if (collider.gameObject.CompareTag("Attack") && !destroyed)
            {
                pStatus.StartReflectInvincibility(1000);
                player.isMove = false;
                isPaused = true;
                player.Arrow.SetActive(true);
                isAttack = true;
                Time.timeScale = 0.2f;
                power = Vector3.zero;
            }

            if (collider.gameObject.CompareTag("QuickAttack") && !destroyed)
                // クールタイム中ではない、かつ反射処理中でなければ実行
                if (reflectCooldownTimer <= 0 && !isReflecting)
                {
                    isReflecting = true;
                    reflectCooldownTimer = reflectCooldown;

                    pStatus.StartReflectInvincibility(1000);
                    player.isMove = false;
                    isQuick = true;
                    SavePower = -power;
                    QuickAttack();
                }
        }

        private void PlayerParamReset()
        {
            Damage = characterParams.damage;
            maxBulletSpeed = characterParams.maxBulletSpeed;
            power = characterParams.power;
            staminaDrainPerSecond = characterParams.staminaDrainPerSecond;
            //quickStaminaDrainPerSecond =  characterParams.quickStaminaDrainPerSecond;
            addSpeed = characterParams.addSpeed;
            damageByReflectionCount = characterParams.damageByReflectionCount;
            reflectInvincible = characterParams.reflectInvincible;
            hitStopDuration　= characterParams.hitStopDuration;
            reflectCooldown = characterParams.reflectCooldown;
        }

        private void QuickAttack()
        {
            if (gameObject.CompareTag("EnemyBullet"))
            {
                gameObject.tag = "Bullet";
                var reflectionEnemyBullet = GetComponent<ReflectionEnemyBullet>();
                reflectionEnemyBullet.ChangeMaterial();
            }

            reflectionCount++;

            if (damageByReflectionCount != null && damageByReflectionCount.Length > 0)
            {
                var index = Mathf.Min(reflectionCount - 1, damageByReflectionCount.Length - 1);
                Damage = damageByReflectionCount[index];
            }

            var powerColor = Mathf.Clamp01(reflectionCount * 0.26f);
            meshRendererChild.material.SetFloat(PowerLevel, powerColor);
            trailRenderer.material.SetFloat(PowerLevel, powerColor);
            if (reflectionCount >= maxReflectionCount)
                powerColor = 1.0f;

            var inputMove = player.PlayerCombat.quickAttackDirection;
            if (inputMove.sqrMagnitude > 0.01f) // 入力がある場合
                currentDirection = new Vector3(inputMove.x, inputMove.y, 0).normalized;
            else
                currentDirection = new Vector3(player.direction, 0, 0).normalized;

            currentSpeed += addSpeed;
            currentSpeed = Mathf.Min(currentSpeed, maxBulletSpeed);
            UpdatePower();

            player.isMove = true;
            isQuick = false; // 即座に状態をリセット
            OnReflect();
            pStatus.StartReflectInvincibility(reflectInvincible);
            StartCoroutine(HitStopDuration());

            player.PlayerCombat.AttackFinish();
            isPaused = false;
        }

        //　壁反射のメソッド
        public bool ReflectFromWall(Vector3 reflectionNormal)
        {
            // クールタイム中、またはプレイヤーがスローモーション中(isAttack)は反射しない
            if (isReflecting || reflectCooldownTimer > 0 || isAttack) return false; // 反射しなかった

            // クールタイム開始
            isReflecting = true;
            reflectCooldownTimer = reflectCooldown;

            // 反射ベクトルを計算
            var incomingDirection = currentDirection.normalized;
            var reflectedDirection = Vector3.Reflect(incomingDirection, reflectionNormal.normalized);

            // 弾の方向に設定
            SetDirection(reflectedDirection);
            UpdatePower(); // power 変数を更新

            // 反射音を再生
            OnWallReflect();

            return true; // 反射に成功した
        }

        public Vector3 GetPower()
        {
            return power;
        }

        public void UpdatePower()
        {
            power = currentDirection.normalized * currentSpeed;
        }

        public void SetDirection(Vector3 newDirection)
        {
            currentDirection = newDirection.normalized;
        }

        public void SetSpeed(float newSpeed)
        {
            currentSpeed = newSpeed;
        }

        public void SetPowerEnemy(Vector3 pos)
        {
            const float CorrectionAimPos = 1.5f;
            var angle = Mathf.Atan2(player.gameObject.transform.position.y - pos.y + CorrectionAimPos,
                player.gameObject.transform.position.x - pos.x);
            //Debug.Log("角度:"+Angle);
            var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
            power = direction;
            PowerDirection = 1f;
            //Debug.Log("Pos:"+Pos+"/Power:"+power);;
        }

        public void SetStraightPowerEnemy(Vector3 angle)
        {
            var direction = Vector3.zero;

            switch (angle.z)
            {
                case >= 0 and <= 45:
                    direction = Vector3.left;
                    break;
                case > 45 and <= 135:
                    direction = Vector3.down;
                    break;
                case > 135 and <= 180:
                    direction = Vector3.right;
                    break;
                default:
                    Debug.Log("範囲外");
                    break;
            }

            power = direction;
            PowerDirection = 1f;
        }

        public void OnReflect()
        {
            audioSource.PlayOneShot(reflectionSound);
        }

        private void OnWallReflect()
        {
            audioSource.PlayOneShot(wallReflectionSound);
        }

        public void AttackFalse()
        {
            isAttack = false;
            isQuick = false;
        }

        private IEnumerator HitStopDuration()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);

            Time.timeScale = 1f;
        }
    }
}