using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Scripts;
using Scripts.Scriptable;

namespace Scripts
{
    public class Player : MonoBehaviour
    {
        //コントローラー
        public static Player Instance;
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private GameObject playerUI;
        private PlayerInput MoveAction;
        private AudioSource audioSource;
        [NonSerialized] public Vector2 InputMove = Vector2.zero;
        private Rigidbody rb;
        private float startY;
        [SerializeField] MapManager mapManager;
        [SerializeField] SceneButtonManager sceneButtonManager;
        [NonSerialized]public Slider staminaSlider;
        [NonSerialized] public Vector2 quickAttackDirection = Vector2.zero;
        //プレイヤーのステータス
        [JapaneseLabel("移動スピード")]private float MoveSpeed;
        [JapaneseLabel("ジャンプ力")]private float jumpPower;
        [JapaneseLabel("最大ジャンプ数")]private int MaxJumpCount;
        [JapaneseLabel("2回目のジャンプまでのクールタイム")]
        private float jumpCooldown = 0.2f;
        [FormerlySerializedAs("limitSpeed")]
        [JapaneseLabel("最大落下速度")]private float maxFallSpeed = 5f;
        [JapaneseLabel("地面レイヤー")] private LayerMask groundLayer;
        [JapaneseLabel("判定消えるまでの時間")]　private float collisionRadius;
        
        //プレイヤーの状態
        [NonSerialized] public int direction = 1;
        [JapaneseLabel("")]private bool isfirst = true;
        [JapaneseLabel("地面についているか")]private bool isGround;
        [JapaneseLabel("ジャンプ中か")]private bool isJump;
        [SerializeField][JapaneseLabel("ジャンプ数")]private int jumpCount;
        [JapaneseLabel("最後にジャンプした時間")]private float lastJumpTime;
        [JapaneseLabel("攻撃中か")]public bool IsAttacking = false;
        [JapaneseLabel("発射中か")]private bool IsShot = false;
        [JapaneseLabel("移動中か")][NonSerialized] public bool isMove = true;
        
        //オブジェクト
        private GameObject Bullets;
        [JapaneseLabel("弾発射位置")][SerializeField] private GameObject ShotPosition;
        [JapaneseLabel("弾き判定")][SerializeField] private GameObject AttackCollision;
        [JapaneseLabel("即弾き判定")][SerializeField] private GameObject QuickAttackCollision;
        [JapaneseLabel("矢印")]public GameObject Arrow;
        [JapaneseLabel("クイック軸")] public GameObject quickAxis;

        [Header("<エフェクト>")] [SerializeField][JapaneseLabel("バットの斬撃")]
        private GameObject batSlash;
        [JapaneseLabel("バットのアニメーションからエフェクトがでるまでの時間")] private float butEffectDuration = 0.1f;
        //[SerializeField] private float MaxBulletTime;
        
        private Image BulletUI;
        
        //アニメーション関連
        private Animator animator;
         
        AnimatorStateInfo animatorStateInfo;
        
        //サウンド関連
        [JapaneseLabel("反射音")]private AudioClip ReflectionSound;
        [JapaneseLabel("発射音")]private AudioClip ShotSound;
        [JapaneseLabel("被ダメージ音")]private AudioClip DamageSound;

        //反射
        [JapaneseLabel("最大反射スタミナ")] private float maxStamina = 100f;
        [JapaneseLabel("反射スタミナ回復量")] private float staminaRecoveryPerSecond = 10f;
        [NonSerialized,JapaneseLabel("現反射スタミナ")] public float currentStamina;
        [NonSerialized,JapaneseLabel("反射スタミナ消費量")]public float staminaDrainPerSecond = 20f;
        //[JapaneseLabel("quick反射消費量")] private float quickStaminaDrainPerSecond = 20f;
        
        //射撃
        [JapaneseLabel(("最大射撃スタミナ"))]private float maxShotStamina = 1f;
        [JapaneseLabel("射撃スタミナ回復量")]private float shotStaminaRecoveryPerSecond = 0.1f;
        [JapaneseLabel("オーバーヒート時の射撃スタミナ回復量")]private float overheatRecoveryPerSecond = 0.1f;
        [JapaneseLabel("現射撃スタミナ")]private float currentShotStamina;
        [JapaneseLabel("射撃スタミナ消費量")]private float shotStaminaDrainPerSecond = 0.25f;
        [JapaneseLabel("射撃クールタイム")]private float shotCoolTime = 0.2f;
        [JapaneseLabel("オーバーヒートしているか")]bool Overheat = false;
        
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            staminaSlider = playerUI.GetComponentInChildren<Slider>();
            BulletUI = playerUI.GetComponentInChildren<Image>();
            
            PlayerParamReset();
        }

        private void PlayerParamReset()
        {
            MoveAction = characterParams.moveAction;
            MoveSpeed = characterParams.moveSpeed;
            jumpPower = characterParams.jumpPower;
            MaxJumpCount = characterParams.MaxJumpCount;
            jumpCooldown = characterParams.jumpCooldown;
            maxFallSpeed = characterParams.maxFallSpeed;
            Bullets = characterParams.bullets;
            ReflectionSound = characterParams.ReflectionSound;
            ShotSound = characterParams.ShotSound;
            DamageSound = characterParams.DamageSound;
            maxStamina = characterParams.maxStamina;
            staminaDrainPerSecond = characterParams.staminaDrainPerSecond;
            staminaRecoveryPerSecond = characterParams.staminaRecoveryPerSecond;
            shotStaminaRecoveryPerSecond = characterParams.shotStaminaRecoveryPerSecond;
            overheatRecoveryPerSecond = characterParams.overheatRecoveryPerSecond;
            shotStaminaDrainPerSecond = characterParams.shotStaminaDrainPerSecond;
            shotCoolTime = characterParams.shotCoolTime;
           // quickStaminaDrainPerSecond = characterParams.quickStaminaDrainPerSecond;
            groundLayer = characterParams.groundLayer;
            collisionRadius = characterParams.collisionRadius;
            butEffectDuration = characterParams.butEffectDuration;
        }
        
        private void Start()
        {
            MoveAction.actions["Move"].performed += OnMove;
            MoveAction.actions["Move"].canceled += OnMove;
            MoveAction.actions["Jump"].started += OnJump;
            MoveAction.actions["Shot"].started += OnShot;
            //MoveAction.actions["Attack"].performed += OnAttack;
            MoveAction.actions["Attack"].canceled += OffAttack;
            MoveAction.actions["Jump"].canceled += OffJump;
            MoveAction.actions["QuickAttack"].performed += OnQuickAttack;
            MoveAction.actions["QuickAttack"].canceled += OffAttack;
            MoveAction.actions["Aim"].performed += OnQuickAttackAim;
            MoveAction.actions["Aim"].canceled += OnQuickAttackAim;

            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            Arrow.SetActive(false);
            jumpCount = MaxJumpCount;
            audioSource = GetComponent<AudioSource>();
            
            mapManager = GameObject.FindObjectOfType<MapManager>();
            currentStamina = maxStamina;
            staminaSlider.maxValue = currentStamina;
            sceneButtonManager = GameObject.FindObjectOfType<SceneButtonManager>();
            currentShotStamina = maxShotStamina;
        }
        
        private void Update()
        {
           // BulletUI.fillAmount = (MaxBulletTime - BulletTime) / MaxBulletTime;
           BulletUI.fillAmount = currentShotStamina;
            // if (BulletTime > 0)
            //     BulletTime -= Time.deltaTime;
            if (!Overheat && currentShotStamina < maxShotStamina)
            {
                currentShotStamina += shotStaminaRecoveryPerSecond * Time.deltaTime;
            }
            else if (Overheat && currentShotStamina < maxShotStamina) 
            {
                currentShotStamina += overheatRecoveryPerSecond * Time.deltaTime;
            }
            
            Vector3 temp = transform.position;
            temp.z = 0f;
            if (!isMove)
            {
                transform.position = temp;
                return;
            }
            
            if (InputMove.x < 0)
            {
                temp+=new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                //transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                transform.localScale = new Vector3(1f, 1f, -1f);
                direction = -1;
            }
            else if (InputMove.x > 0)
            {
                temp+=new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                //transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                transform.localScale = new Vector3(1f, 1f, 1f);
                direction = 1;
            }
            transform.position = temp;

            animator.SetFloat("Jump", rb.linearVelocity.magnitude);
            
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
            
            if (Overheat)
            {
                BulletUI.color = Color.red;
            }
            else
            {
                BulletUI.color = Color.white;
            }
            
            animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (animatorStateInfo.IsName("isShot") && animatorStateInfo.normalizedTime >= 1.0f)
            {
                IsShot = false;
            }
            //落下速度制限
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }

            if (quickAttackDirection != Vector2.zero)
            {
                // 入力方向から角度を計算
                float quickAngle = Mathf.Atan2(quickAttackDirection.y, quickAttackDirection.x) * Mathf.Rad2Deg;
                // 矢印の回転を設定
                quickAxis.transform.rotation = Quaternion.Euler(0f, 0f, quickAngle-90);
                quickAxis.SetActive(true);
            }
            else
            {
                quickAxis.SetActive(false);
            }
        }

        public void Ground()
        {
                    jumpCount = MaxJumpCount;
        }
        public void OnQuickAttackAim(InputAction.CallbackContext context)
        {
            quickAttackDirection = context.ReadValue<Vector2>();
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            if (animator == null)
            {
                Debug.LogWarning("Animatorがnullです。Playerオブジェクトが既に破棄されているか、適切に初期化されていません。");
                return;
            }
            
            animator.SetBool("isMove", true);
            InputMove = context.ReadValue<Vector2>();

            if (InputMove != Vector2.zero)
            {
                animator.SetBool("isMove", true);
                var Angle = Mathf.Atan2(InputMove.y, InputMove.x) * Mathf.Rad2Deg;
                Arrow.transform.rotation = Quaternion.Euler(0f, 0f, Angle);
            }
            else
            {
                animator.SetBool("isMove", false); 
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            if (jumpCount > 0 && Time.time - lastJumpTime >= jumpCooldown)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpCount--;
                lastJumpTime = Time.time;
                animator.SetBool("isJump",true);
            }
        }

        public void OffJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            isJump = false;
            animator.SetBool("isJump", false);
        }

        public void OnShot(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            if (Overheat == false && isJump == false && IsShot == false)
            {
                IsShot = true;
                currentShotStamina -= shotStaminaDrainPerSecond;
                animator.SetTrigger("isShot");
                Invoke("Shot",0.45f);
            }
        }

        public void Shot()
        {
            audioSource.PlayOneShot(ShotSound);
            var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
            var bullet = bullets.GetComponent<Bullet>();
            bullet.PowerDirection = direction;
            Invoke("ShotFinish",shotCoolTime);
        }

        private void ShotFinish()
        {
            IsShot=false;
        }
        // public void OnAttack(InputAction.CallbackContext context)
        // {
        //     if(IsAttacking) return;
        //     if (currentStamina <= staminaDrainPerSecond)
        //     {
        //         OnQuickAttack(context);
        //         return;
        //     }
        //     
        //     IsAttacking = true;
        //     if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
        //     
        //     AttackCollision.gameObject.SetActive(true);
        //     Invoke("AttackCollisionFalse", collisionRadius);
        //     
        // }

        public void OnQuickAttack(InputAction.CallbackContext context)
        {
            if(IsAttacking) return;
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            

            QuickAttackCollision.gameObject.SetActive(true);
            Invoke("AttackCollisionFalse", collisionRadius);
            PlayAttackAnimation();
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

        public void PlayAttackAnimation()
        {
            animator.SetTrigger("isAttack");
        }
        
        private void PlayEffect()
        {
            batSlash.SetActive(true);
            Invoke("EffectCancel", 0.2f);
        }

        private void EffectCancel()
        {
            batSlash.SetActive(false);
        }
        public void PlayReflectionSound()
        {
            audioSource.PlayOneShot(ReflectionSound);
        }

        public void PlayDamageSound()
        {
            audioSource.PlayOneShot(DamageSound);
        }
        private void AttackCollisionFalse()
        {
            AttackCollision.gameObject.SetActive(false);
            QuickAttackCollision.gameObject.SetActive(false);
            IsAttacking = false;
        }

        public void PlayerReset()
        {
            MoveAction.actions["Move"].performed -= OnMove;
            MoveAction.actions["Move"].canceled -= OnMove;
            MoveAction.actions["Jump"].started -= OnJump;
            MoveAction.actions["Shot"].started -= OnShot;
            //MoveAction.actions["Attack"].performed -= OnAttack;
            MoveAction.actions["Attack"].canceled -= OffAttack;
            MoveAction.actions["Jump"].canceled -= OffJump;
            MoveAction.actions["QuickAttack"].performed -= OnQuickAttack;
            MoveAction.actions["QuickAttack"].canceled -= OffAttack;
            MoveAction.actions["Aim"].performed -= OnQuickAttackAim;
            MoveAction.actions["Aim"].canceled -= OnQuickAttackAim;
        }
        private void OnEnable()
        {
            // OnEnable で購読を開始
            MoveAction.actions["Move"].performed += OnMove;
            MoveAction.actions["Move"].canceled += OnMove;
            MoveAction.actions["Jump"].started += OnJump;
            MoveAction.actions["Shot"].started += OnShot;
            //MoveAction.actions["Attack"].performed += OnAttack;
            MoveAction.actions["Attack"].canceled += OffAttack;
            MoveAction.actions["Jump"].canceled += OffJump;
            MoveAction.actions["QuickAttack"].performed += OnQuickAttack;
            MoveAction.actions["QuickAttack"].canceled += OffAttack;
            MoveAction.actions["Aim"].performed += OnQuickAttackAim;
            MoveAction.actions["Aim"].canceled += OnQuickAttackAim;
        }

        private void OnDisable()
        {
            // OnDisable で購読を解除
            if (MoveAction != null)
            {
                MoveAction.actions["Move"].performed -= OnMove;
                MoveAction.actions["Move"].canceled -= OnMove;
                MoveAction.actions["Jump"].started -= OnJump;
                MoveAction.actions["Shot"].started -= OnShot;
                //MoveAction.actions["Attack"].performed -= OnAttack;
                MoveAction.actions["Attack"].canceled -= OffAttack;
                MoveAction.actions["Jump"].canceled -= OffJump;
                MoveAction.actions["QuickAttack"].performed -= OnQuickAttack;
                MoveAction.actions["QuickAttack"].canceled -= OffAttack;
                MoveAction.actions["Aim"].performed -= OnQuickAttackAim;
                MoveAction.actions["Aim"].canceled -= OnQuickAttackAim;
            }
        }

    }
}