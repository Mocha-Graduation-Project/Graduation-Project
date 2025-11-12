using System;
using System.Collections;
using Scripts;
using Scripts.Scriptable;
using UnityEngine;
using UnityEngine.InputSystem;
//using System.Numerics;

namespace Player
{
    public class Bullet : MonoBehaviour
    {
        private static readonly int PowerLevel = Shader.PropertyToID("_PowerLevel");
        private UnityEngine.Vector3 power;
        Player player => Player.Instance;
        PlayerStatus pStatus => PlayerStatus.Instance;
        [NonSerialized]public float PowerDirection;
        [NonSerialized] public Vector2 quickAttackDirectionInput = Vector2.zero;
        private int count = 1;
        private bool isAttack = false;
        private bool isQuick = false;
        
        private Material material;
        private bool destroyed = false; //Destroyで消してもAttckに反応することがあるので仮で配置、バグ治せれば消す
        private bool isPaused = false;
        
        [SerializeField] private MeshRenderer meshRendererChild;
        [NonSerialized]public int reflectionCount;
        private int maxReflectionCount = 4;

        private UnityEngine.Vector3 SavePower;
        private Vector2 lastInputDirection = Vector2.right;
        
        private float staminaDrainPerSecond = 0f;
        
        private PlayerInput moveAction;
        
        private float attackCoolMaxTime = 1f;
        private float attackCoolTime = 0f;
        [NonSerialized][JapaneseLabel("初期ダメージ値")]public int Damage = 1;
        [JapaneseLabel("最大スピード")] private float maxBulletSpeed;
        [JapaneseLabel("弾くたびに＋〇〇速度を追加")] private float addSpeed;
        [JapaneseLabel("1回目〇ダメージ、2回目〇ダメージ...")] private int[] damageByReflectionCount;
        [SerializeField] private CharacterParams characterParams;

        [JapaneseLabel("現在の速度")]private float currentSpeed;
        [JapaneseLabel("現在の移動方向")]private Vector3 currentDirection = Vector3.right;
        
        [JapaneseLabel("反射後の無敵時間")]　private float reflectInvincible = 1;
        [NonSerialized] public Transform arrowTransform;
        
        [SerializeField] private Renderer trailRenderer;
        private Vector2 savedQuickDirection;
        [JapaneseLabel("ヒットストップ時間")]private float hitStopDuration;
        [JapaneseLabel("敵のレイヤー")]private int enemyLayer;
        
        [JapaneseLabel("反射処理中かどうか")][NonSerialized] private bool isReflecting = false;
        [JapaneseLabel("反射クールタイムのタイマー")][NonSerialized] private float reflectCooldownTimer = 0f;

        [JapaneseLabel("反射クールタイムの秒数")] private float reflectCooldown = 0.5f;

        private void Awake()
        {
            PlayerParamReset();
        }
        private void Start()
        {
            if (this.gameObject.CompareTag("EnemyBullet"))
            {
                currentDirection = power * PowerDirection;
            }
            else
            {
                currentDirection = new Vector3(player.direction, 0, 0).normalized;
            }
            
            //currentDirection = characterParams.power.normalized * PowerDirection;
            currentSpeed = characterParams.power.magnitude;
            arrowTransform = player.Arrow.transform;
            UpdatePower();

            reflectionCount = 0;
            //moveAction = GetComponent<PlayerInput>();
            // moveAction.actions["Attack"].canceled += OffAttack;
            enemyLayer = LayerMask.NameToLayer("Enemy");
        }

        void Update()
        {
            if (!isPaused)
            {
                //return;
                // 最大スピード制限
                currentSpeed = Mathf.Min(currentSpeed, maxBulletSpeed);
                       
                // 移動
                transform.position += currentDirection * (currentSpeed * Time.deltaTime);
            }
            
            if (reflectCooldownTimer > 0)
            {
                reflectCooldownTimer -= Time.deltaTime;
            }
            else
            {
                // クールタイムが終了したら、次の反射を許可
                isReflecting = false;
            }


            // 回転
            if (currentDirection != Vector3.zero)
            {
                float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            
            Vector2 input = player.InputMove;

            // 入力があれば更新、なければ前回の方向を維持
            if (input.sqrMagnitude > 0.01f)
            {
                lastInputDirection = input.normalized;
            }
            
            //スタミナ消費
            // if (isAttack)
            // {
            //     player.currentStamina -= staminaDrainPerSecond * Time.deltaTime * 5;
            //     if (player.currentStamina <= 0)
            //     {
            //         player.currentStamina = 0;
            //         player.AttackFinish();
            //         if (attackCoolTime > attackCoolMaxTime)
            //         {
            //             Attack(); 
            //             attackCoolTime = 0;
            //         }
            //
            //     }
            // }

            if (isQuick)
            {
                //player.currentStamina -= quickStaminaDrainPerSecond * Time.deltaTime * 5;
                if (player.currentStamina <= 0)
                {
                    player.currentStamina = 0;
                    player.AttackFinish();
                    if (attackCoolTime > attackCoolMaxTime)
                    {
                        //QuickAttack(); 
                        attackCoolTime = 0;
                    }

                }
            }
            if (isAttack && arrowTransform != null)
            {
                // 矢印の方向ベクトルを取得
                Vector3 dir = arrowTransform.right; // 右方向が矢印の先なら .right、上方向なら .up

                // currentDirectionを矢印方向に更新
                currentDirection = dir.normalized;

                // 回転も矢印の回転に合わせる（オプション）
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }
            
            attackCoolTime+= Time.deltaTime;
            player.staminaSlider.value = player.currentStamina;
            
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

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag("Ground")&& !isAttack)
            {
                if (collider.gameObject.layer == LayerMask.NameToLayer("FloatFloor"))
                    return;

                //ResetBullet();
                //Time.timeScale = 1f;
                player.isMove = true;
                isAttack = false;
                destroyed = true;
                Destroy(this.gameObject);
            }
            if (collider.gameObject.CompareTag("Player") && !isAttack)
            {
                if(!CompareTag("EnemyBullet")) return;
                if (collider.TryGetComponent<PlayerStatus>(out PlayerStatus status))
                {
                    status.Damage(1);
                }

                //ResetBullet();
                //Time.timeScale = 1f;
                player.isMove = true;
                isAttack = false;
                destroyed = true;
                Destroy(this.gameObject);
            }
            if (collider.gameObject.CompareTag("Attack") && !destroyed)
            {
                pStatus.StartReflectInvincibility(1000);
                player.isMove = false;
                isPaused = true;
                player.Arrow.SetActive(true);
                isAttack = true;
                Time.timeScale = 0.2f;
                power = UnityEngine.Vector3.zero;
                //Invoke("Attack", 0.3f);

            }

            if (collider.gameObject.CompareTag("QuickAttack") && !destroyed)
            {
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
        }
        
        // private void Attack()
        // {
        //     if (this.gameObject.CompareTag("EnemyBullet"))
        //     {
        //         this.gameObject.tag = "Bullet";
        //         ReflectionEnemyBullet reflectionEnemyBullet = GetComponent<ReflectionEnemyBullet>();
        //         reflectionEnemyBullet.ChangeMaterial();
        //     }
        //
        //     reflectionCount++;
        //     if (damageByReflectionCount != null && damageByReflectionCount.Length > 0)
        //     {
        //         int index = Mathf.Min(reflectionCount - 1, damageByReflectionCount.Length - 1);
        //         Damage = damageByReflectionCount[index];
        //     }
        //
        //     float powerColor = Mathf.Clamp01(reflectionCount * 0.26f);
        //     if (reflectionCount >= maxReflectionCount)
        //         powerColor = 1.0f;
        //     meshRendererChild.material.SetFloat("_PowerLevel", powerColor);
        //     trailRenderer.material.SetFloat("_PowerLevel", powerColor);
        //
        //     player.currentStamina -= 2.5f;
        //     player.Arrow.SetActive(false);
        //     player.isMove = true;
        //     isPaused = false; // スローモーション解除
        //
        //     PowerDirection *= 1.25f;
        //     if (PowerDirection < 0)
        //         PowerDirection *= -1;
        //     
        //     float angle = Mathf.Atan2(lastInputDirection.y, lastInputDirection.x);
        //     currentDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
        //
        //     // スピード増加
        //     currentSpeed += addSpeed;
        //     currentSpeed = Mathf.Min(currentSpeed, maxBulletSpeed);
        //     UpdatePower(); // 最終的な速度と方向でpowerを更新
        //
        //     Time.timeScale = 1f;
        //     player.IsAttacking = false;
        //     isAttack = false;
        //
        //     Invoke("AttackFalse", 0.2f);
        //
        //     player.PlayReflectionSound();
        //     pStatus.StartReflectInvincibility(reflectInvincible);
        //
        // }

        private void QuickAttack()
        {
            if (this.gameObject.CompareTag("EnemyBullet"))
            {
                this.gameObject.tag = "Bullet";
                ReflectionEnemyBullet reflectionEnemyBullet = GetComponent<ReflectionEnemyBullet>();
                reflectionEnemyBullet.ChangeMaterial();
            }
            
            // Damage = Mathf.Min(Damage + addDamage, maxDamage);
            reflectionCount++;
            
            if (damageByReflectionCount != null && damageByReflectionCount.Length > 0)
            {
                int index = Mathf.Min(reflectionCount - 1, damageByReflectionCount.Length - 1);
                Damage = damageByReflectionCount[index];
            }
            
            float powerColor = Mathf.Clamp01(reflectionCount * 0.26f);
            meshRendererChild.material.SetFloat(PowerLevel, powerColor);
            trailRenderer.material.SetFloat(PowerLevel, powerColor);
            if (reflectionCount >= maxReflectionCount)
                powerColor = 1.0f;
            
            Vector2 inputMove = player.quickAttackDirection;
            if (inputMove.sqrMagnitude > 0.01f) // 入力がある場合
            {
                currentDirection = new Vector3(inputMove.x, inputMove.y, 0).normalized;
            }
            else
            {
                currentDirection = new Vector3(player.direction, 0, 0).normalized; 
            }

            currentSpeed += addSpeed;
            currentSpeed = Mathf.Min(currentSpeed, maxBulletSpeed);
            UpdatePower();
            
            player.isMove = true;
            isQuick = false; // 即座に状態をリセット
            player.PlayReflectionSound();
            pStatus.StartReflectInvincibility(reflectInvincible);
            StartCoroutine(HitStopDuration());

            player.AttackFinish();
        }

        // private void OffAttack(InputAction.CallbackContext context)
        // {
        //     //CancelInvoke("Attack");
        //     if (isAttack)
        //     {
        //         Attack();
        //         player.PlayAttackAnimation();
        //     }
        //
        // }
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
            float angle = Mathf.Atan2(player.gameObject.transform.position.y - pos.y + CorrectionAimPos,
                player.gameObject.transform.position.x - pos.x);
            //Debug.Log("角度:"+Angle);
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
            power = direction;
            PowerDirection = 1f;
            //Debug.Log("Pos:"+Pos+"/Power:"+power);;
        }
        
        public void SetStraightPowerEnemy(Vector3 angle)
        {
            Vector3 direction=Vector3.zero;

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
            player.PlayReflectionSound();
        }
        public void AttackFalse()
        {
            isAttack = false;
            isQuick = false;
        }
        
        // public void ResetBullet()
        // {
        //     moveAction.actions["Attack"].canceled -= OffAttack;
        // }

        private IEnumerator HitStopDuration()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            
            Time.timeScale = 1f;
        }
    }
}
