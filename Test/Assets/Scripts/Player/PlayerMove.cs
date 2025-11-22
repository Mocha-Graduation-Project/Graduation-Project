# region

using Scriptable;
using UnityEngine;
using Scripts.Scriptable;
using UnityEngine.InputSystem;
# endregion
namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMove : MonoBehaviour
    {
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private SoundData soundData;
        
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
            if (currentJumpCount > 0 && Time.time - lastJumpTime >= jumpCooldown)
            {
                if (currentJumpCount == MaxJumpCount)
                { 
                    animator.SetTrigger(JumpHash);
                }
                audioSource1.PlayOneShot(jumpSound);
                // Rigidbodyの速度を直接操作
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower); 

                currentJumpCount--;
                lastJumpTime = Time.time;

            }
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
            // 1. 水平移動の実行
            if (Player.Instance.isMove)
            {
                Vector3 newVelocity = new Vector3(currentMoveInput.x * MoveSpeed, rb.linearVelocity.y, 0);
                rb.linearVelocity = newVelocity;
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
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