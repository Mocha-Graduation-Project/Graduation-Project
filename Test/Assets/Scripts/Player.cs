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
        private PlayerInput MoveAction;
        private AudioSource audioSource;
        [NonSerialized] public Vector2 InputMove = Vector2.zero;
        private Rigidbody2D rb;
        private float startY;
        [SerializeField] MapManager mapManager;
        [SerializeField] SceneButtonManager sceneButtonManager;
        public Slider staminaSlider;
        
        //プレイヤーのステータス
        private float MoveSpeed;
        private float jumpPower;
        private int MaxJumpCount;
        [JapaneseLabel("2回目のジャンプまでのクールタイム")]
        private float jumpCooldown = 0.2f;
        [FormerlySerializedAs("limitSpeed")]
        private float maxFallSpeed = 5f;
        
        //プレイヤーの状態
        [NonSerialized] public int direction = 1;
        private bool isfirst = true;
        private bool isGround;
        private bool isJump;
        private int jumpCount;
        private float lastJumpTime; // 最後にジャンプした時間
        private bool IsAttacking = false;
        private bool IsShot = false;
        [NonSerialized] public bool isMove = true;
        
        //オブジェクト
        private GameObject Bullets;
        [SerializeField] private GameObject ShotPosition;
        [SerializeField] private GameObject AttackCollision;
        [SerializeField] private GameObject QuickAttackCollision;
        public GameObject Arrow;
        
        //[SerializeField] private float MaxBulletTime;
        
        [SerializeField] private Image BulletUI;
        
        //アニメーション関連
        private Animator animator;
         
        AnimatorStateInfo animatorStateInfo;
        
        //サウンド関連
        private AudioClip ReflectionSound;
        private AudioClip ShotSound;
        private AudioClip DamageSound;

        //反射
        [JapaneseLabel("最大反射スタミナ")] private float maxStamina = 100f;
        [JapaneseLabel("反射スタミナ回復量")] private float staminaRecoveryPerSecond = 10f;
        [NonSerialized,JapaneseLabel("現反射スタミナ")] public float currentStamina;
        [NonSerialized,JapaneseLabel("反射スタミナ消費量")]public float staminaDrainPerSecond = 20f;
        [JapaneseLabel("quick反射消費量")] private float quickStaminaDrainPerSecond = 20f;
        
        //射撃
        [JapaneseLabel(("最大射撃スタミナ"))]private float maxShotStamina = 1f;
        [JapaneseLabel("射撃スタミナ回復量")]private float shotStaminaRecoveryPerSecond = 0.1f;
        [JapaneseLabel("オーバーヒート時の射撃スタミナ回復量")]private float overheatRecoveryPerSecond = 0.1f;
        [JapaneseLabel("現射撃スタミナ")]private float currentShotStamina;
        [JapaneseLabel("射撃スタミナ消費量")]private float shotStaminaDrainPerSecond = 0.25f;
        [JapaneseLabel("射撃クールタイム")]private float shotCoolTime = 0.2f;
        bool Overheat = false;
        
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
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
            quickStaminaDrainPerSecond = characterParams.quickStaminaDrainPerSecond;

        }
        
        private void Start()
        {
            MoveAction.actions["Move"].performed += OnMove;
            MoveAction.actions["Move"].canceled += OnMove;
            MoveAction.actions["Jump"].started += OnJump;
            MoveAction.actions["Shot"].started += OnShot;
            MoveAction.actions["Attack"].performed += OnAttack;
            MoveAction.actions["Attack"].canceled += OffAttack;
            MoveAction.actions["Jump"].canceled += OffJump;
            MoveAction.actions["QuickAttack"].performed += OnQuickAttack;
            MoveAction.actions["QuickAttack"].canceled += OffAttack;

            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
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
            if (!isMove)
                return;
            if (InputMove.x < 0)
            {
                transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                transform.localScale = new Vector3(1f, 1f, -1f);
                direction = -1;
            }
            else if (InputMove.x > 0)
            {
                transform.position += new Vector3(MoveSpeed * InputMove.x, 0, 0) * Time.deltaTime;
                transform.localScale = new Vector3(1f, 1f, 1f);
                direction = 1;
            }

            animator.SetFloat("Jump", rb.linearVelocityY);
            
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
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Ground")
                jumpCount = MaxJumpCount;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
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
                // audioSource.PlayOneShot(ShotSound);
                // var bullets = Instantiate(Bullets, ShotPosition.transform.position, Quaternion.identity);
                // var bullet = bullets.GetComponent<Bullet>();
                // bullet.PowerDirection = direction;
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
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (currentStamina <= 0) return;

            IsAttacking = true;
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            
            AttackCollision.gameObject.SetActive(true);
            Invoke("AttackCollisionFalse", 0.1f);
            //Invoke("AttackFinish", 0.3f);
            //animator.SetTrigger("isAttack");
        }

        public void OnQuickAttack(InputAction.CallbackContext context)
        {
            if (currentStamina <= 0) return;

            IsAttacking = true;
            if (sceneButtonManager.currentState != SceneButtonManager.State.Gameplay) return;
            

            QuickAttackCollision.gameObject.SetActive(true);
            animator.SetTrigger("isAttack");
            Invoke("AttackCollisionFalse", 0.1f);
            //Invoke("AttackFinish", 0.3f);
            //animator.SetTrigger("isAttack");
        }
        private void OffAttack(InputAction.CallbackContext context)
        {
            AttackFinish();
        }
        public void AttackFinish()
        {
            IsAttacking = false;
            AttackCollision.gameObject.SetActive(false);
            QuickAttackCollision.gameObject.SetActive(false);
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
        }

        public void PlayerReset()
        {
            MoveAction.actions["Move"].performed -= OnMove;
            MoveAction.actions["Move"].canceled -= OnMove;
            MoveAction.actions["Jump"].started -= OnJump;
            MoveAction.actions["Shot"].started -= OnShot;
            MoveAction.actions["Attack"].performed -= OnAttack;
            MoveAction.actions["Attack"].canceled -= OffAttack;
            MoveAction.actions["Jump"].canceled -= OffJump;
            MoveAction.actions["QuickAttack"].performed -= OnQuickAttack;
            MoveAction.actions["QuickAttack"].canceled -= OffAttack;
        }
    }
}