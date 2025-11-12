using System;
using Component;
using Scripts.Scriptable;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;

namespace Player
{
    public class Player : MonoBehaviour
    {
        //コンポーネント
        public static Player Instance;
        private PlayerMove playerMove;

        //参照
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private SoundData soundData;
        [SerializeField] private GameObject playerUI;
        private PlayerInput MoveAction;
        [SerializeField]private AudioSource audioSource1;
        [SerializeField]private AudioSource audioSource2;
        [NonSerialized] public Vector2 InputMove = Vector2.zero;
        [SerializeField] MapManager mapManager;
        [SerializeField] SceneButtonManager sceneButtonManager;
        [NonSerialized]public Slider staminaSlider;
        [NonSerialized] public Vector2 quickAttackDirection = Vector2.zero;
        
        //プレイヤーのステータス
        [JapaneseLabel("判定消えるまでの時間")]　private float collisionRadius;
        
        //プレイヤーの状態
        [NonSerialized] public int direction = 1;
        [NonSerialized][JapaneseLabel("攻撃中か")]public bool IsAttacking = false;
        [NonSerialized][JapaneseLabel("発射中か")]private bool IsShot = false;
        [NonSerialized][JapaneseLabel("移動中か")] public bool isMove = true;

        private float quickAngle;
        //オブジェクト
        private GameObject Bullets;
        [JapaneseLabel("弾発射位置")][SerializeField] private GameObject ShotPosition;
        [JapaneseLabel("弾き判定")][SerializeField] private GameObject AttackCollision;
        [JapaneseLabel("即弾き判定")][SerializeField] private GameObject QuickAttackCollision;
        [JapaneseLabel("矢印")]public GameObject Arrow;
        [JapaneseLabel("クイック軸")] public GameObject quickAxis;

        [Header("<エフェクトリスト>")]
        [SerializeField]
        private VisualEffect[] effectPrefabs; 
        [Header("<エフェクト生存時間リスト>")]
        [SerializeField]
        private float[] effectDurations;
        [JapaneseLabel("バットのアニメーションからエフェクトがでるまでの時間")] private float butEffectDuration = 0.1f;
        
        private Image BulletUI;
        
        //アニメーション関連
        private Animator animator;
        private static readonly int IsShot1 = Animator.StringToHash("isShot");
        private static readonly int AttackDirection = Animator.StringToHash("AttackDirection");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");
        private static readonly int Idle = Animator.StringToHash("Idle");
        AnimatorStateInfo animatorStateInfo;
        
        //サウンド関連
        [JapaneseLabel("反射音")]private AudioClip reflectionSound;
        [JapaneseLabel("発射音")]private AudioClip shotSound;
        [JapaneseLabel("被ダメージ音")]private AudioClip damageSound;

        //反射
        [JapaneseLabel("最大反射スタミナ")] private float maxStamina = 100f;
        [JapaneseLabel("反射スタミナ回復量")] private float staminaRecoveryPerSecond = 10f;
        [NonSerialized,JapaneseLabel("現反射スタミナ")] public float currentStamina;
        [NonSerialized,JapaneseLabel("反射スタミナ消費量")]public float staminaDrainPerSecond = 20f;
        private Vector2 lastAimInput = Vector2.zero;
        [JapaneseLabel("スティックで弾きが発動するデットゾーン")]private float deadZone;
        
        //射撃
        [JapaneseLabel(("最大射撃スタミナ"))]private float maxShotStamina = 1f;
        [JapaneseLabel("射撃スタミナ回復量")]private float shotStaminaRecoveryPerSecond = 0.1f;
        [JapaneseLabel("オーバーヒート時の射撃スタミナ回復量")]private float overheatRecoveryPerSecond = 0.1f;
        [JapaneseLabel("現射撃スタミナ")]private float currentShotStamina;
        [JapaneseLabel("射撃スタミナ消費量")]private float shotStaminaDrainPerSecond = 0.25f;
        [JapaneseLabel("射撃クールタイム")]private float shotCoolTime = 0.2f;
        [JapaneseLabel("オーバーヒートしているか")] private bool Overheat = false;

        [JapaneseLabel("攻撃アニメーションに弾きのタイミングを合わせる")][SerializeField]
        private bool attackAnimationBestTime = true;
        
        private readonly System.Collections.Generic.Dictionary<float, WaitForSeconds> waitCache = new System.Collections.Generic.Dictionary<float, WaitForSeconds>();
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            staminaSlider = playerUI.GetComponentInChildren<Slider>();
            BulletUI = playerUI.GetComponentInChildren<Image>();
            if (playerMove == null)
            {
                playerMove = GetComponent<PlayerMove>();
            }
            PlayerParamReset();
        }

        private void PlayerParamReset()
        {
            MoveAction = characterParams.moveAction;
            Bullets = characterParams.bullets;
            reflectionSound = soundData.ReflectionSound;
            shotSound = soundData.ShotSound;
            damageSound = soundData.DamageSound;
            maxStamina = characterParams.maxStamina;
            staminaDrainPerSecond = characterParams.staminaDrainPerSecond;
            staminaRecoveryPerSecond = characterParams.staminaRecoveryPerSecond;
            shotStaminaRecoveryPerSecond = characterParams.shotStaminaRecoveryPerSecond;
            overheatRecoveryPerSecond = characterParams.overheatRecoveryPerSecond;
            shotStaminaDrainPerSecond = characterParams.shotStaminaDrainPerSecond;
            shotCoolTime = characterParams.shotCoolTime;
            collisionRadius = characterParams.collisionRadius;
            butEffectDuration = characterParams.butEffectDuration;
            deadZone = characterParams.deadZone;
        }
        
        [Obsolete("Obsolete")]
        private void Start()
        {
            animator = GetComponent<Animator>();
            Arrow.SetActive(false);
            
            mapManager = FindObjectOfType<MapManager>();
            currentStamina = maxStamina;
            staminaSlider.maxValue = currentStamina;
            sceneButtonManager = FindObjectOfType<SceneButtonManager>();
            currentShotStamina = maxShotStamina;
        }
        
        private void Update()
        {
           BulletUI.fillAmount = currentShotStamina;
           switch (Overheat)
           {
               case false when currentShotStamina < maxShotStamina:
                   currentShotStamina += shotStaminaRecoveryPerSecond * Time.deltaTime;
                   break;
               case true when currentShotStamina < maxShotStamina:
                   currentShotStamina += overheatRecoveryPerSecond * Time.deltaTime;
                   break;
           }
            
            Vector3 temp = transform.position;
            temp.z = 0f;
            transform.position = temp;
            
            if (!isMove)
            {
                return;
            }
            
            if (!IsAttacking && currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryPerSecond * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
            staminaSlider.value = currentStamina;

            if (currentShotStamina <= 0)
            {
                Overheat = true;
            }
            if (currentShotStamina >= maxShotStamina)
            {
                Overheat = false;
            }
            
            BulletUI.color = Overheat ? Color.red : Color.white;
            
            animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (animatorStateInfo.IsName("isShot") && animatorStateInfo.normalizedTime >= 1.0f)
            {
                IsShot = false;
            }

            // if (quickAttackDirection != Vector2.zero)
            // {
            //     // 入力方向から角度を計算
            //     float quickAngle = Mathf.Atan2(quickAttackDirection.y, quickAttackDirection.x) * Mathf.Rad2Deg;
            //     // 矢印の回転を設定
            //     quickAxis.transform.rotation = Quaternion.Euler(0f, 0f, quickAngle-90);
            //     quickAxis.SetActive(true);
            // }
            // else
            // {
            //     quickAxis.SetActive(false);
            // }
        }

        public void Ground(bool isGrounded)
        {
            playerMove.SetGroundState(isGrounded);
        }

        private void OnQuickAttackAim(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            
            if (input.sqrMagnitude > deadZone)
            {
                quickAttackDirection = input.normalized; 
                quickAngle = Mathf.Atan2(quickAttackDirection.y, quickAttackDirection.x) * Mathf.Rad2Deg;

                if (!IsAttacking && lastAimInput.sqrMagnitude <= deadZone)
                {
                    //Debug.Log(quickAngle);
                    OnQuickAttackTriggered(quickAngle);
                }
                quickAxis.transform.rotation = Quaternion.Euler(0f, 0f, quickAngle - 90);
                quickAxis.SetActive(true);
            }
            else
            {
                quickAxis.SetActive(false);
            }
            lastAimInput = input;
        }
        private void OnQuickAttackTriggered(float angle)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
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
                IsAttacking = true;

                QuickAttackCollision.gameObject.SetActive(true);
                StartCoroutine(DeactivateAttackCollisionAfterDelay(collisionRadius));
            }
        }

        public void Attacking()
        {
            if(!attackAnimationBestTime) return;
            IsAttacking = true;

            QuickAttackCollision.gameObject.SetActive(true);
            StartCoroutine(DeactivateAttackCollisionAfterDelay(collisionRadius));
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            if (animator == null)
            {
                Debug.LogWarning("Animatorがnullです。Playerオブジェクトが既に破棄されているか、適切に初期化されていません。");
                return;
            }
            InputMove = context.ReadValue<Vector2>();

            playerMove.SetMoveInput(InputMove); 
            
            if (InputMove != Vector2.zero)
            {
                var angle = Mathf.Atan2(InputMove.y, InputMove.x) * Mathf.Rad2Deg;
                Arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            playerMove.HandleJump(audioSource1); 
        }
        public void OffJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            playerMove.HandleJumpCanceled();
        }
        public void OnShot(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            if (Overheat == false && IsShot == false)
            {
                IsShot = true;
                currentShotStamina -= shotStaminaDrainPerSecond;
                animator.SetTrigger(IsShot1);
                StartCoroutine(ShootWithCoolDown(0.45f, shotCoolTime));
            }
        }
        private void ShotFinish()
        {
            IsShot=false;
        }

        public void OnQuickAttack(InputAction.CallbackContext context)
        {
            if(IsAttacking) return;
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            

            QuickAttackCollision.gameObject.SetActive(true);
            Invoke(nameof(AttackCollisionFalse), collisionRadius);
            //PlayAttackAnimation();
        }
        private void OffAttack(InputAction.CallbackContext context)
        {
            AttackFinish();
        }
        public void AttackFinish()
        {
            AttackCollision.gameObject.SetActive(false);
            QuickAttackCollision.gameObject.SetActive(false);
        }

        private void PlayAttackAnimation(int attackDirection)
        {
            //animator.ResetTrigger(IsAttack);
            animator.SetTrigger(Idle);
            animator.SetInteger(AttackDirection,attackDirection);
            animator.SetTrigger(IsAttack);
        }
        
        public void PlayEffect(int effectIndex)
        {
            // インデックスの有効性チェック
            if (effectPrefabs == null || effectIndex < 0 || effectIndex >= effectPrefabs.Length)
            {
                Debug.LogWarning($"PlayEffect: Invalid effect index {effectIndex} or effect list is null/empty.");
                return;
            }
            
            // 生存時間リストのインデックスチェック
            if (effectDurations == null || effectIndex >= effectDurations.Length)
            {
                Debug.LogWarning($"PlayEffect: Effect duration is not set for index {effectIndex}. Using default duration ({butEffectDuration}).");
            }
            
            VisualEffect effectToPlay = effectPrefabs[effectIndex];
            if (effectToPlay != null)
            {
                // 生存時間を取得（リストに設定がない場合はbutEffectDurationをデフォルト値として使用）
                float duration = (effectDurations != null && effectIndex < effectDurations.Length) 
                    ? effectDurations[effectIndex] 
                    : butEffectDuration; 

                // コルーチンでエフェクトを再生
                //effectToPlay.GetComponent<VisualEffect>().SendEvent("OnPlay");
                ShowEffectForDuration(effectToPlay, duration);
            }
        }
        private WaitForSeconds GetWait(float seconds)
        {
            if (!waitCache.TryGetValue(seconds, out var waitObject))
            {
                waitObject = new WaitForSeconds(seconds);
                waitCache.Add(seconds, waitObject);
            }
            return waitObject;
        }
        private void ShowEffectForDuration(VisualEffect effectObject, float duration)
        {
            effectObject.SendEvent("OnPlay");
        }
        public void PlayReflectionSound()
        {
            audioSource1.PlayOneShot(reflectionSound);
        }

        public void PlayDamageSound()
        {
            audioSource1.PlayOneShot(damageSound);
        }
        private void AttackCollisionFalse()
        {
            AttackCollision.gameObject.SetActive(false);
            QuickAttackCollision.gameObject.SetActive(false);
            IsAttacking = false;
        }
        private System.Collections.IEnumerator DeactivateAttackCollisionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            AttackCollision.gameObject.SetActive(false);
            QuickAttackCollision.gameObject.SetActive(false);
            IsAttacking = false;
        }
        
        private System.Collections.IEnumerator ShootWithCoolDown(float preShotDelay, float coolDown)
        {
            // 0.45秒待機（アニメーションに合わせる）
            yield return new WaitForSeconds(preShotDelay);
    
            // Shot() の処理
            audioSource1.PlayOneShot(shotSound);
            var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
            var bullet = bullets.GetComponent<Bullet>();
            bullet.PowerDirection = direction;
    
            // shotCoolTime 待機
            yield return new WaitForSeconds(coolDown);
    
            // ShotFinish() の処理
            IsShot = false;
        }
        
        private void OnEnable()
        {
            // OnEnable で購読を開始
            MoveAction.actions["Move"].performed += OnMove;
            MoveAction.actions["Move"].canceled += OnMove;
            MoveAction.actions["Jump"].started += OnJump;
            MoveAction.actions["Shot"].started += OnShot;
            MoveAction.actions["Attack"].canceled += OffAttack;
            MoveAction.actions["Jump"].canceled += OffJump;
            MoveAction.actions["Aim"].performed += OnQuickAttackAim;
            MoveAction.actions["Aim"].canceled += OnQuickAttackAim;
        }

        private void OnDisable()
        {
            // OnDisable で購読を解除
            if (MoveAction == null) return;
            MoveAction.actions["Move"].performed -= OnMove;
            MoveAction.actions["Move"].canceled -= OnMove;
            MoveAction.actions["Jump"].started -= OnJump;
            MoveAction.actions["Shot"].started -= OnShot;
            MoveAction.actions["Attack"].canceled -= OffAttack;
            MoveAction.actions["Jump"].canceled -= OffJump;
            MoveAction.actions["Aim"].performed -= OnQuickAttackAim;
            MoveAction.actions["Aim"].canceled -= OnQuickAttackAim;
        }

    }
}