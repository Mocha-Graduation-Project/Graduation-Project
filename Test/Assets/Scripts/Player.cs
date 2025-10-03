using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Scripts.Scriptable;

namespace Scripts
{
    public class Player : MonoBehaviour
    {
        //コントローラー
        public static Player Instance;
        private static readonly int IsMove = Animator.StringToHash("isMove");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int IsJump = Animator.StringToHash("isJump");
        private static readonly int IsShot1 = Animator.StringToHash("isShot");
        private static readonly int AttackDirection = Animator.StringToHash("AttackDirection");
        private static readonly int IsAttack = Animator.StringToHash("isAttack");
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private SoundData soundData;
        [SerializeField] private GameObject playerUI;
        private PlayerInput MoveAction;
        [SerializeField]private AudioSource audioSource1;
        [SerializeField]private AudioSource audioSource2;
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

        private float quickAngle;
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
        [JapaneseLabel("反射音")]private AudioClip reflectionSound;
        [JapaneseLabel("発射音")]private AudioClip shotSound;
        [JapaneseLabel("被ダメージ音")]private AudioClip damageSound;
        [JapaneseLabel("ジャンプ")] public AudioClip jumpSound;
        [JapaneseLabel("歩き")] public AudioClip walkSound;

        //反射
        [JapaneseLabel("最大反射スタミナ")] private float maxStamina = 100f;
        [JapaneseLabel("反射スタミナ回復量")] private float staminaRecoveryPerSecond = 10f;
        [NonSerialized,JapaneseLabel("現反射スタミナ")] public float currentStamina;
        [NonSerialized,JapaneseLabel("反射スタミナ消費量")]public float staminaDrainPerSecond = 20f;
        //[JapaneseLabel("quick反射消費量")] private float quickStaminaDrainPerSecond = 20f;
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
           // quickStaminaDrainPerSecond = characterParams.quickStaminaDrainPerSecond;
            groundLayer = characterParams.groundLayer;
            collisionRadius = characterParams.collisionRadius;
            butEffectDuration = characterParams.butEffectDuration;
            jumpSound = soundData.JumpSound;
            walkSound = soundData.WalkSound;
            deadZone = characterParams.deadZone;
        }
        
        [Obsolete("Obsolete")]
        private void Start()
        {
            MoveAction.actions["Move"].performed += OnMove;
            MoveAction.actions["Move"].canceled += OnMove;
            MoveAction.actions["Jump"].started += OnJump;
            MoveAction.actions["Shot"].started += OnShot;
            //MoveAction.actions["Attack"].performed += OnAttack;
            MoveAction.actions["Attack"].canceled += OffAttack;
            MoveAction.actions["Jump"].canceled += OffJump;
            // MoveAction.actions["QuickAttack"].performed += OnQuickAttack;
            // MoveAction.actions["QuickAttack"].canceled += OffAttack;
            MoveAction.actions["Aim"].performed += OnQuickAttackAim;
            MoveAction.actions["Aim"].canceled += OnQuickAttackAim;
            

            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            Arrow.SetActive(false);
            jumpCount = MaxJumpCount;
            
            mapManager = FindObjectOfType<MapManager>();
            currentStamina = maxStamina;
            staminaSlider.maxValue = currentStamina;
            sceneButtonManager = FindObjectOfType<SceneButtonManager>();
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
            
            //歩き音
            if (isGround && animator.GetBool(IsMove))
            {
                if (!audioSource2.isPlaying)
                {
                    audioSource2.loop = true;
                    audioSource2.clip = walkSound;
                    audioSource2.Play();
                }
            }
            else
            {
                // それ以外の場合は停止する
                if (audioSource2.isPlaying)
                {
                    audioSource2.Stop();
                }
            }
            
            if (InputMove.x < 0)
            {
                temp+=new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                //transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                //transform.localScale = new Vector3(1f, 1f, -1f);
                transform.rotation = Quaternion.Euler(0, -90, 0);
                direction = -1;
            }
            else if (InputMove.x > 0)
            {
                temp+=new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                //transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                //transform.localScale = new Vector3(1f, 1f, 1f);
                transform.rotation = Quaternion.Euler(0, 90, 0);
                direction = 1;
            }
            transform.position = temp;

            animator.SetFloat(Jump, rb.linearVelocity.magnitude);
            
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
            //落下速度制限
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
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
            
            isGround = isGrounded;
        }

        public void JumpCount(bool isGrounded)
        {
            jumpCount = MaxJumpCount;
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
                    Debug.Log(quickAngle);
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
            int attackDirection;
            if (angle is >= 45 and < 135)
            {
                // 上方向
                attackDirection = 0;
            }
            else if (angle is >= -135 and < -45)
            {
                // 下方向
                attackDirection = 2;
            }
            else if (angle is >= -45 and < 45)
            {
                // 右方向
                attackDirection = (direction == 1) ? 1 : 3;
                
            }
            else
            { 
                // 左方向
                attackDirection = (direction == -1) ? 1 : 3;
            }
            
            
            IsAttacking = true;

            QuickAttackCollision.gameObject.SetActive(true);
            
            Debug.Log(attackDirection);

            PlayAttackAnimation(attackDirection);

            Invoke(nameof(AttackCollisionFalse), collisionRadius);
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            if (animator == null)
            {
                Debug.LogWarning("Animatorがnullです。Playerオブジェクトが既に破棄されているか、適切に初期化されていません。");
                return;
            }
            animator.SetBool(IsMove, true);
            InputMove = context.ReadValue<Vector2>();

            if (InputMove != Vector2.zero)
            {
                animator.SetBool(IsMove, true);
                var angle = Mathf.Atan2(InputMove.y, InputMove.x) * Mathf.Rad2Deg;
                Arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            else
            {
                animator.SetBool(IsMove, false); 
                audioSource2.Stop();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            if (jumpCount > 0 && Time.time - lastJumpTime >= jumpCooldown)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
                // ForceMode.Impulseで瞬間的に力を加える
                rb.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
                // rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
                jumpCount--;
                lastJumpTime = Time.time;
                animator.SetBool(IsJump,true);
                audioSource1.PlayOneShot(jumpSound);
            }
        }

        public void OffJump(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            isJump = false;
            animator.SetBool(IsJump, false);
        }

        public void OnShot(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            if (Overheat == false && isJump == false && IsShot == false)
            {
                IsShot = true;
                currentShotStamina -= shotStaminaDrainPerSecond;
                animator.SetTrigger(IsShot1);
                Invoke(nameof(Shot),0.45f);
            }
        }

        public void Shot()
        {
            audioSource1.PlayOneShot(shotSound);
            var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
            var bullet = bullets.GetComponent<Bullet>();
            bullet.PowerDirection = direction;
            Invoke(nameof(ShotFinish),shotCoolTime);
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
            animator.SetInteger(AttackDirection,attackDirection);
            animator.SetTrigger(IsAttack);
        }
        
        private void PlayEffect()
        {
            batSlash.SetActive(true);
            Invoke(nameof(EffectCancel), 0.2f);
        }

        private void EffectCancel()
        {
            batSlash.SetActive(false);
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

        public void PlayerReset()
        {
            MoveAction.actions["Move"].performed -= OnMove;
            MoveAction.actions["Move"].canceled -= OnMove;
            MoveAction.actions["Jump"].started -= OnJump;
            MoveAction.actions["Shot"].started -= OnShot;
            //MoveAction.actions["Attack"].performed -= OnAttack;
            MoveAction.actions["Attack"].canceled -= OffAttack;
            MoveAction.actions["Jump"].canceled -= OffJump;
            //MoveAction.actions["QuickAttack"].performed -= OnQuickAttack;
            //MoveAction.actions["QuickAttack"].canceled -= OffAttack;
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
            //MoveAction.actions["QuickAttack"].performed += OnQuickAttack;
            //MoveAction.actions["QuickAttack"].canceled += OffAttack;
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
            //MoveAction.actions["Attack"].performed -= OnAttack;
            MoveAction.actions["Attack"].canceled -= OffAttack;
            MoveAction.actions["Jump"].canceled -= OffJump;
            //MoveAction.actions["QuickAttack"].performed -= OnQuickAttack;
            //MoveAction.actions["QuickAttack"].canceled -= OffAttack;
            MoveAction.actions["Aim"].performed -= OnQuickAttackAim;
            MoveAction.actions["Aim"].canceled -= OnQuickAttackAim;
        }

    }
}