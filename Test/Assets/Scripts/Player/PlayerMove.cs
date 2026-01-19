# region

using Scriptable;
using UnityEngine;
using Scripts.Scriptable;
using UnityEngine.InputSystem;
using System;
# endregion
namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMove : MonoBehaviour
    {
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private SoundData soundData;

        [Header("Movement Settings")]
        [SerializeField][JapaneseLabel("加速")] private float acceleration = 60f;
        [SerializeField][JapaneseLabel("減速")] private float deceleration = 60f;
        
        [Header("Jump Settings")]
        [SerializeField][JapaneseLabel("足場から踏み外しても、一瞬だけジャンプ可能")] private float coyoteTime = 0.1f;
        [SerializeField][JapaneseLabel("着地と同時にジャンプ")] private float jumpBufferTime = 0.1f;

        
        public event Action OnJump;

        private Rigidbody rb;
        private Animator animator;
        
        private float MoveSpeed;
        private float jumpPower;
        private int MaxJumpCount;
        private float jumpCooldown;
        private float maxFallSpeed;
        private LayerMask groundLayer;
        private AudioClip jumpSound;
        private AudioClip walkSound;
        private bool jumpCanceled;

        // 状態
        private Vector2 currentMoveInput = Vector2.zero;
        private int direction = 1;
        private bool isGround = false;
        private int currentJumpCount;
        private float lastJumpTime;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;

        
        // Animatorハッシュ
        private static readonly int IsMoveHash = Animator.StringToHash("isMove");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int IsGround = Animator.StringToHash("isGround");

        // 音
        [SerializeField] private AudioSource walkAudioSource; 
        
        
        [SerializeField] [JapaneseLabel("足元")] private Transform groundCheck;
        private readonly float checkDistance = 0.08f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            LoadCharacterParams();
        }

        private void LoadCharacterParams()
        {
            MoveSpeed = characterParams.moveSpeed;
            jumpPower = characterParams.jumpPower;
            MaxJumpCount = characterParams.MaxJumpCount;
            jumpCooldown = characterParams.jumpCooldown;
            maxFallSpeed = characterParams.maxFallSpeed;
            groundLayer = characterParams.groundLayer;
            jumpSound = soundData.jumpSound;
            walkSound = soundData.walkSound;
            jumpCanceled = characterParams.jumpCanceled;
            
            currentJumpCount = MaxJumpCount;
        }

        public void SetMoveInput(Vector2 input)
        {
            currentMoveInput = input;
            
            // アニメーション制御と方向転換
            
            animator.SetBool(IsMoveHash, Mathf.Abs(currentMoveInput.x) > 0.01f);

            if (currentMoveInput.x != 0)
            {
                direction = currentMoveInput.x > 0 ? 1 : -1;
                transform.rotation = Quaternion.Euler(0, direction == 1 ? 90 : -90, 0);
                if (Player.Instance != null)
                {
                    Player.Instance.direction = direction;
                }
            }
        }

        public void HandleJump(AudioSource audioSource1)
        {
            jumpBufferCounter = jumpBufferTime;
        }

        private void ExecuteJump()
        {
             if (Time.time - lastJumpTime >= jumpCooldown)
            {
                // 通常ジャンプ (Coyote Time有効)
                if (coyoteTimeCounter > 0f)
                {
                     PerformJump();
                     // ダブルジャンプ用にカウントを減らす（地上ジャンプなので残り回数はMAX-1になるはずだが、
                     // 現在の実装ではcurrentJumpCountを減らす方式なのでそれに合わせる）
                     currentJumpCount--; 
                }
                // 空中ジャンプ (ダブルジャンプ)
                else if (currentJumpCount > 0 && currentJumpCount < MaxJumpCount)
                {
                    PerformJump();
                    OnJump?.Invoke();
                    currentJumpCount--;
                }
            }
        }
        
        private void PerformJump()
        {
            if (currentJumpCount == MaxJumpCount)
            { 
                 animator.SetTrigger(JumpHash);
            }
            walkAudioSource.PlayOneShot(jumpSound); // Using walkAudioSource or passed audioSource? 
            // NOTE: The original code passed audioSource1, but here we can use the one checking walk or just play on a specific one.
            // For safety, let's play on the component's audio source or we need to change the signature.
            // Getting the audio source from HandleJump logic is tricky because we moved it to FixedUpdate.
            // Let's use the local walkAudioSource (AudioSource component) for simplicity or GetComponent<AudioSource>()
            
            // Re-using the logic from original code approximately. 
            // Ideally we should have a dedicated SFX source.
            if(walkAudioSource != null) walkAudioSource.PlayOneShot(jumpSound);

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower); 
            //OnJump?.Invoke();
            
            lastJumpTime = Time.time;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        public void HandleJumpCanceled()
        {
            if(!jumpCanceled) return;
            // 上昇中のときのみ
            if (rb.linearVelocity.y > 0)
            {
                // ジャンプの高さを制限
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
        
        private void FixedUpdate()
        {
            CheckGround();
            
            // ジャンプ処理
            if (jumpBufferCounter > 0f)
            {
                ExecuteJump();
                jumpBufferCounter -= Time.deltaTime;
            }

            // 1. 水平移動の実行 (加減速の適用)
            if (Player.Instance.isMove)
            {
                float targetSpeed = currentMoveInput.x * MoveSpeed;
                
                // 加速・減速の選択
                float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
                
                float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.deltaTime);
                
                rb.linearVelocity = new Vector3(newSpeed, rb.linearVelocity.y, 0);
            }
            else
            {
                // 入力がない場合（または動けない場合）は減速
                 float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, 0, deceleration * Time.deltaTime);
                 rb.linearVelocity = new Vector3(newSpeed, rb.linearVelocity.y, 0);
            }
            
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
        }
        
        private void Update()
        {
            Vector3 temp = transform.position;
            temp.z = 0f;
            transform.position = temp;

            bool isMovingOnGround = isGround && animator.GetBool(IsMoveHash) && Player.Instance.isMove;
            
            if (isMovingOnGround)
            {
                if (walkAudioSource.isPlaying) return;
                walkAudioSource.loop = true;
                walkAudioSource.clip = walkSound;
                walkAudioSource.Play();
            }
            else
            {
                if (walkAudioSource.isPlaying)
                {
                    walkAudioSource.Stop();
                }
            }
        }
        private void CheckGround()
        {
            bool wasGrounded = isGround; // 前フレームの接地状態
            isGround = Physics.Raycast(groundCheck.position, Vector2.down, checkDistance, groundLayer);

            // Coyote Timeの更新
            if (isGround)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
    
            // アニメーターへの通知
            animator.SetBool(IsGround, isGround);

            // 地面に着いた瞬間にジャンプ回数をリセット
            if (isGround && !wasGrounded)
            {
                currentJumpCount = MaxJumpCount;
            }
        }
    }
}