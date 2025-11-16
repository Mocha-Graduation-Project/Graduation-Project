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
        private Player player;
        private Animator animator;
        [SerializeField] private AudioSource audioSource1;
        [SerializeField] private AudioSource audioSource2;

        // 参照
        private CharacterParams characterParams;
        private SoundData soundData;
        private SceneButtonManager sceneButtonManager;
        private Slider staminaSlider;
        private Image bulletUI;

        // 状態
        [NonSerialized] public int direction = 1;
        [NonSerialized][JapaneseLabel("攻撃中か")] public bool isAttacking = false;
        [NonSerialized][JapaneseLabel("発射中か")] private bool isShot = false;
        [NonSerialized] public Vector2 quickAttackDirection = Vector2.zero;

        // オブジェクト
        private GameObject Bullets;
        [JapaneseLabel("弾発射位置")][SerializeField] private GameObject shotPosition;
        [JapaneseLabel("即弾き判定")][SerializeField] private GameObject quickAttackCollision;
        [JapaneseLabel("クイック軸")] public GameObject quickAxis;
        
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

        [Header("反射の固定化")] [SerializeField] [JapaneseLabel("反射の角度ステップ")] [Tooltip("反射の角度を何度ごとに固定化するか。0にすると固定化しない。")]
        private float angleStep = 30;
        
        // 射撃スタミナ
        private float maxShotStamina = 1f;
        private float shotStaminaRecoveryPerSecond;
        private float overheatRecoveryPerSecond;
        private float currentShotStamina;
        private float shotStaminaDrainPerSecond;
        private float shotCoolTime;
        private bool overheat = false;

        [SerializeField] private bool attackAnimationBestTime = true;
        
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
            bulletUI = player.BulletUI;

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

            // 初期化
            currentStamina = maxStamina;
            if (staminaSlider != null) staminaSlider.maxValue = currentStamina;
            currentShotStamina = maxShotStamina;
        }

        private void Start()
        {
            sceneButtonManager = FindObjectOfType<SceneButtonManager>();
        }

        private void Update()
        {
            // Player.csからisMoveとdirectionを同期
            bool isMove = player.isMove;
            direction = player.direction;
            
            // 射撃スタミナ/UI更新
            if (bulletUI != null) bulletUI.fillAmount = currentShotStamina;
            switch (overheat)
            {
                case false when currentShotStamina < maxShotStamina:
                    currentShotStamina += shotStaminaRecoveryPerSecond * Time.deltaTime;
                    break;
                case true when currentShotStamina < maxShotStamina:
                    currentShotStamina += overheatRecoveryPerSecond * Time.deltaTime;
                    break;
            }
            if (currentShotStamina <= 0) overheat = true;
            if (currentShotStamina >= maxShotStamina) overheat = false;
            if (bulletUI != null) bulletUI.color = overheat ? Color.red : Color.white;
            
            // 移動中かチェック
            if (!isMove)
            {
                return;
            }
            
            // 反射スタミナ/UI更新
            if (!isAttacking && currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryPerSecond * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
            if (staminaSlider != null) staminaSlider.value = currentStamina;

            // アニメーション状態チェック
            animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (animatorStateInfo.IsName("isShot") && animatorStateInfo.normalizedTime >= 1.0f)
            {
                isShot = false;
            }
        }
        
        // (InputHandlerから呼ばれる)メソッド 
        
        public void HandleQuickAttackAim(Vector2 input)
        {
            if (input.sqrMagnitude > deadZone)
            {
                quickAttackDirection = input.normalized; 
                float quickAngle = Mathf.Atan2(quickAttackDirection.y, quickAttackDirection.x) * Mathf.Rad2Deg;
                
                float snappedAngle = SnapAngle(quickAngle,angleStep);
                quickAxis.transform.rotation = Quaternion.Euler(0f, 0f, snappedAngle - 90);

                if (!isAttacking && lastAimInput.sqrMagnitude <= deadZone)
                {
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
        private float SnapAngle(float angle, float step)
        {
            // stepが0以下の場合は、固定化しないで元の角度を返す
            if (step <= 0)
            {
                return angle;
            }
            
            // (入力角度 / ステップ) を四捨五入し、それにステップを掛ける
            // 例: angle=16, step=30 -> Round(16/30) * 30 -> Round(0.53) * 30 -> 1 * 30 = 30
            // 例: angle=14, step=30 -> Round(14/30) * 30 -> Round(0.46) * 30 -> 0 * 30 = 0
            return Mathf.Round(angle / step) * step;
        }

        public void PerformShot()
        {
            if (overheat == false && isShot == false)
            {
                isShot = true;
                currentShotStamina -= shotStaminaDrainPerSecond;
                animator.SetTrigger(IsShot1);
                StartCoroutine(ShootWithCoolDown(0.45f, shotCoolTime));
            }
        }
        //弾き処理
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
                isAttacking = true;
                quickAttackCollision.gameObject.SetActive(true);
                StartCoroutine(DeactivateAttackCollisionAfterDelay(collisionRadius));
            }
        }
        
        public void Attacking() // Animation Event
        {
            if(!attackAnimationBestTime) return;
            isAttacking = true;
            quickAttackCollision.gameObject.SetActive(true);
            StartCoroutine(DeactivateAttackCollisionAfterDelay(collisionRadius));
        }
        
        private void ShotFinish() // Animation Event
        {
            isShot　=　false;
        }

        public void AttackFinish()
        {
            quickAttackCollision.gameObject.SetActive(false);
        }

        private void PlayAttackAnimation(int attackDirection)
        {
            animator.SetTrigger(Idle);
            animator.SetInteger(AttackDirection,attackDirection);
            animator.SetTrigger(IsAttack);
        }
        //弾き判定解除遅延
        private IEnumerator DeactivateAttackCollisionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            quickAttackCollision.gameObject.SetActive(false);
            isAttacking = false;
        }
        //射撃クールダウン
        private IEnumerator ShootWithCoolDown(float preShotDelay, float coolDown)
        {
            yield return new WaitForSeconds(preShotDelay);
    
            audioSource1.PlayOneShot(shotSound);
            var bullets = Instantiate(Bullets, shotPosition.transform.position, Quaternion.identity);
            var bullet = bullets.GetComponent<Bullet>();
            bullet.PowerDirection = direction;
    
            yield return new WaitForSeconds(coolDown);
    
            isShot = false;
        }
    }
}