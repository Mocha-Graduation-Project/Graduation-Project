using System;
using System.Collections;
using Scriptable;
using Scripts.Scriptable;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        public event Action OnShoot;
        public event Action OnReflect;

        private Player player;
        private Animator animator;
        [SerializeField] private AudioSource audioSource1;
        [SerializeField] private AudioSource audioSource2;

        // 参照
        private CharacterParams characterParams;
        private SoundData soundData;
        private SceneButtonManager sceneButtonManager;
        private Slider staminaSlider;
        private Image BulletUI;

        // 状態
        [NonSerialized] public int direction = 1;
        [NonSerialized][JapaneseLabel("攻撃中か")] public bool IsAttacking = false;
        [NonSerialized][JapaneseLabel("発射中か")] private bool IsShot = false;
        [NonSerialized] public Vector2 quickAttackDirection = Vector2.zero;

        // オブジェクト
        private GameObject Bullets;
        [JapaneseLabel("弾発射位置")][SerializeField] private GameObject ShotPosition;
        [JapaneseLabel("即弾き判定")][SerializeField] private GameObject QuickAttackCollision;
        [JapaneseLabel("クイック軸")] public GameObject quickAxis;
        
        // 角度のスナップ設定
        // [Header("弾き方向の調整")] [JapaneseLabel("90度を何分割するか(1=90度毎, 2=45度毎)")]
        // private int angleSnapSegments;
        [Header("弾き方向の調整")]
        [JapaneseLabel("スナップする角度の間隔 (45=8方向, 30=12方向)")]
        private float angleStep = 45f; 

        [JapaneseLabel("斜め方向の判定の広さ (0.5=均等, 0.6=広め)")]
        private float diagonalMagnetism = 0.5f;
        
        // アニメーション関連
        private static readonly int IsShot1 = Animator.StringToHash("isShot");
        private static readonly int AttackDirection = Animator.StringToHash("AttackDirection");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");
        private static readonly int Idle = Animator.StringToHash("Idle");
        AnimatorStateInfo animatorStateInfo;
        
        // サウンド関連
        private AudioClip shotSound;

        // 反射スタミナ
        private float maxStamina;
        [NonSerialized] public float currentStamina;
        private float staminaRecoveryPerSecond;
        [NonSerialized] public float staminaDrainPerSecond;
        private Vector2 lastAimInput = Vector2.zero;
        private float deadZone;
        private float collisionRadius;
        
        // 射撃スタミナ
        private float maxShotStamina = 1f;
        private float shotStaminaRecoveryPerSecond;
        private float overheatRecoveryPerSecond;
        private float currentShotStamina;
        private float shotStaminaDrainPerSecond;
        private float shotCoolTime;
        private bool Overheat = false;

        [SerializeField] private bool attackAnimationBestTime = true;
        private Coroutine attackCoroutine;
        
        private void Awake()
        {
            player = GetComponent<Player>();
            animator = GetComponent<Animator>();
        }

        // Player.cs からパラメータを受け取る
        public void Initialize(CharacterParams @params, SoundData sData)
        {
            characterParams = @params;
            soundData = sData;
            
            // UI参照をPlayerから取得
            staminaSlider = player.staminaSlider;
            BulletUI = player.BulletUI;

            // パラメータ設定
            Bullets = characterParams.bullets;
            shotSound = soundData.shotSound;
            maxStamina = characterParams.maxStamina;
            staminaDrainPerSecond = characterParams.staminaDrainPerSecond;
            staminaRecoveryPerSecond = characterParams.staminaRecoveryPerSecond;
            shotStaminaRecoveryPerSecond = characterParams.shotStaminaRecoveryPerSecond;
            overheatRecoveryPerSecond = characterParams.overheatRecoveryPerSecond;
            shotStaminaDrainPerSecond = characterParams.shotStaminaDrainPerSecond;
            shotCoolTime = characterParams.shotCoolTime;
            collisionRadius = characterParams.collisionRadius;
            deadZone = characterParams.deadZone;
            angleStep = characterParams.angleStep;

            // 初期化
            currentStamina = maxStamina;
            if (staminaSlider != null) staminaSlider.maxValue = currentStamina;
            currentShotStamina = maxShotStamina;
        }

        private void Start()
        {
            sceneButtonManager = SceneButtonManager.Instance;
        }

        private void Update()
        {
            // Player.csからisMoveとdirectionを同期
            bool isMove = player.isMove;
            direction = player.direction;
            
            // 射撃スタミナ/UI更新
            if (BulletUI != null) BulletUI.fillAmount = currentShotStamina;
            switch (Overheat)
            {
                case false when currentShotStamina < maxShotStamina:
                    currentShotStamina += shotStaminaRecoveryPerSecond * Time.deltaTime;
                    break;
                case true when currentShotStamina < maxShotStamina:
                    currentShotStamina += overheatRecoveryPerSecond * Time.deltaTime;
                    break;
            }
            if (currentShotStamina <= 0) Overheat = true;
            if (currentShotStamina >= maxShotStamina) Overheat = false;
            if (BulletUI != null) BulletUI.color = Overheat ? Color.red : Color.white;
            
            // 移動中かチェック
            if (!isMove)
            {
                return;
            }
            
            // 反射スタミナ/UI更新
            if (!IsAttacking && currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryPerSecond * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
            if (staminaSlider != null) staminaSlider.value = currentStamina;

            // アニメーション状態チェック
            animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (animatorStateInfo.IsName("isShot") && animatorStateInfo.normalizedTime >= 1.0f)
            {
                IsShot = false;
            }
        }
        
        // (InputHandlerから呼ばれる)メソッド 
        
        public void HandleQuickAttackAim(Vector2 input)
        {
            if (input.sqrMagnitude > deadZone)
            {
                // 元の角度 (-180 ～ 180度)
                float rawAngle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

                // 負の角度を正の角度 (0 ～ 360) に変換して計算しやすくする
                float positiveAngle = (rawAngle < 0) ? rawAngle + 360f : rawAngle;

                // スナップ処理（重み付け対応）
                float snappedAngle = SnapAngleWeighted(positiveAngle, angleStep, diagonalMagnetism);

                float snappedRad = snappedAngle * Mathf.Deg2Rad;
                quickAttackDirection = new Vector2(Mathf.Cos(snappedRad), Mathf.Sin(snappedRad));

                // 矢印の回転反映
                quickAxis.transform.rotation = Quaternion.Euler(0f, 0f, snappedAngle - 90);
                quickAxis.SetActive(true);

                // トリガー判定（入力変化の確認用には元の入力か、あるいはスナップ後の変化を見る）
                if (!IsAttacking && lastAimInput.sqrMagnitude <= deadZone)
                {
                    OnQuickAttackTriggered(snappedAngle); 
                }
            }
            else
            {
                quickAxis.SetActive(false);
            }
            lastAimInput = input;
        }
        // 指定した間隔で角度をスナップさせる。斜めの判定幅(weight)を考慮する。
        private float SnapAngleWeighted(float angle, float step, float weight)
        {
            float indexFloat = angle / step;
            int index = Mathf.FloorToInt(indexFloat);
            
            float remainder = indexFloat - index;
            
            bool isEvenIndex = (index % 2 == 0);
            float threshold = isEvenIndex ? (1f - weight) : weight;

            int snappedIndex = index;
            if (remainder >= threshold)
            {
                snappedIndex = index + 1;
            }

            return snappedIndex * step;
        }

        public void PerformShot()
        {
            if (Overheat == false && IsShot == false)
            {
                IsShot = true;
                currentShotStamina -= shotStaminaDrainPerSecond;
                animator.SetTrigger(IsShot1);
                OnShoot?.Invoke();
                StartCoroutine(ShootWithCoolDown(0.45f, shotCoolTime));
            }
        }
        //弾き処理
        private void OnQuickAttackTriggered(float angle)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            // 射撃などの他のアクションを強制キャンセル
            if (IsShot)
            {
                IsShot = false;
                animator.ResetTrigger(IsShot1);
                // ShootWithCoolDownコルーチンが回っていれば止める等の処理が必要だが、
                // PerformShotで生成したコルーチンを変数に保持していないため、ここではフラグとトリガーのリセットのみ行う
                // (IsShot = false により PerformShot のガードは効く)
            }

            int attackDirection = angle switch
            {
                >= 45 and < 135 => 0,
                >= -135 and < -45 => 2,
                >= -45 and < 45 => (direction == 1) ? 1 : 3,
                _ => (direction == -1) ? 1 : 3
            };
            
            PlayAttackAnimation(attackDirection);
            if (!attackAnimationBestTime)
            {
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                IsAttacking = true;
                QuickAttackCollision.gameObject.SetActive(false); // Reset first
                QuickAttackCollision.gameObject.SetActive(true);
                //OnReflect?.Invoke();
                attackCoroutine = StartCoroutine(DeactivateAttackCollisionAfterDelay(collisionRadius));
            }
        }
        
        public void Attacking() // Animation Event
        {
            if(!attackAnimationBestTime) return;
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            IsAttacking = true;
            QuickAttackCollision.gameObject.SetActive(false); // Reset first
            QuickAttackCollision.gameObject.SetActive(true);
            attackCoroutine = StartCoroutine(DeactivateAttackCollisionAfterDelay(collisionRadius));
        }
        
        private void ShotFinish() // Animation Event
        {
            IsShot = false;
        }

        public void AttackFinish()
        {
            QuickAttackCollision.gameObject.SetActive(false);
            OnReflect?.Invoke();
        }

        private void PlayAttackAnimation(int attackDirection)
        {
            //animator.SetTrigger(Idle);
           // animator.SetInteger(AttackDirection,attackDirection);
            
            // 方向に応じたステート名を指定して遷移
            string stateName = attackDirection switch
            {
                0 => "UPAttack",
                1 => "Attack",
                2 => "DawnAttack",
                3 => "BackAttack",
                _ => "Attack"
            };
            
            animator.CrossFadeInFixedTime(stateName, 0.05f); 
        }
        //弾き判定解除遅延
        private IEnumerator DeactivateAttackCollisionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            QuickAttackCollision.gameObject.SetActive(false);
            IsAttacking = false;
        }
        //射撃クールダウン
        private IEnumerator ShootWithCoolDown(float preShotDelay, float coolDown)
        {
            yield return new WaitForSeconds(preShotDelay);
    
            audioSource1.PlayOneShot(shotSound);
            var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
            var bullet = bullets.GetComponent<Bullet>();
            bullet.PowerDirection = direction;
    
            yield return new WaitForSeconds(coolDown);
    
            IsShot = false;
        }
    }
}