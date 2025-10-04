using UnityEngine;
using Scripts.Scriptable;
using UnityEngine.InputSystem;

namespace Player
{
    // PlayerMovementはPlayerからの入力を受け取り、物理的な移動を制御する
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMove : MonoBehaviour
    {
        // 外部から設定されるパラメータはScriptableObjectから取得する
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private SoundData soundData;
        
        // RigidbodyはFixedUpdateで操作するためプライベートにする
        private Rigidbody rb;
        private Animator animator; // アニメーション制御はPlayerMovementに一時的に残す

        // CharacterParamsから取得する値
        private float MoveSpeed;
        private float jumpPower;
        private int MaxJumpCount;
        private float jumpCooldown;
        private float maxFallSpeed;
        private LayerMask groundLayer;
        private AudioClip jumpSound;
        private AudioClip walkSound;

        // 状態
        private Vector2 currentMoveInput = Vector2.zero;
        private int direction = 1; // 1:右, -1:左
        private bool isGround = false;
        private int currentJumpCount;
        private float lastJumpTime;
        
        // Animatorハッシュ (private static readonlyが望ましい)
        private static readonly int IsMoveHash = Animator.StringToHash("isMove");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        // ウォークサウンド再生用 (AudioSource2はPlayer.csから引き継ぎ)
        [SerializeField] private AudioSource walkAudioSource; 

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
            jumpSound = soundData.JumpSound;
            walkSound = soundData.WalkSound;
            
            currentJumpCount = MaxJumpCount;
        }

        // --- 外部からの入力設定 ---

        // PlayerInputHandler (元Player.OnMove) から呼ばれることを想定
        public void SetMoveInput(Vector2 input)
        {
            currentMoveInput = input;
            
            // アニメーション制御と方向転換
            animator.SetBool(IsMoveHash, currentMoveInput != Vector2.zero);

            if (currentMoveInput.x != 0)
            {
                direction = currentMoveInput.x > 0 ? 1 : -1;
                transform.rotation = Quaternion.Euler(0, direction == 1 ? 90 : -90, 0);
            }
        }

        // PlayerInputHandler (元Player.OnJump) から呼ばれることを想定
        public void HandleJump(AudioSource audioSource1)
        {
            if (currentJumpCount > 0 && Time.time - lastJumpTime >= jumpCooldown)
            {
                // Rigidbodyの速度を直接操作
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower); 

                currentJumpCount--;
                lastJumpTime = Time.time;
                animator.SetTrigger(JumpHash);
                audioSource1.PlayOneShot(jumpSound);
            }
        }

        // PlayerInputHandler (元Player.OffJump) から呼ばれることを想定
        public void HandleJumpCanceled()
        {
            // 上昇中（垂直速度が正）のときのみ
            if (rb.linearVelocity.y > 0)
            {
                // ジャンプの高さを制限
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
        
        // --- 接地とリセット ---
        
        // 接地判定 (Player.Groundメソッドと統合)
        public void SetGroundState(bool isGrounded)
        {
            if (isGrounded && !isGround)
            {
                // 地面に着いた瞬間にジャンプ回数をリセット
                currentJumpCount = MaxJumpCount;
            }
            isGround = isGrounded;
        }

        // --- 物理更新 (FixedUpdate) ---

        private void FixedUpdate()
        {
            // 1. 水平移動の実行
            if (Player.Instance.isMove) // Playerクラスに依存するのを避け、独自のフラグを持つことが理想
            {
                Vector3 newVelocity = new Vector3(currentMoveInput.x * MoveSpeed, rb.linearVelocity.y, 0);
                rb.linearVelocity = newVelocity;
            }
            else
            {
                // 移動入力がない場合は水平速度をゼロに（空中ではそのまま）
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            
            // 2. 落下速度制限 (FixedUpdateで実行)
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
        }
        
        // --- フレーム更新 (Update) ---
        
        private void Update()
        {
            // 3. Z座標の固定 (Unityの2D設定が望ましいが、コードで行う場合)
            Vector3 temp = transform.position;
            temp.z = 0f;
            transform.position = temp;

            // 4. 歩き音の制御 (Updateで行う)
            bool isMovingOnGround = isGround && animator.GetBool(IsMoveHash) && Player.Instance.isMove;
            
            if (isMovingOnGround)
            {
                if (!walkAudioSource.isPlaying)
                {
                    walkAudioSource.loop = true;
                    walkAudioSource.clip = walkSound;
                    walkAudioSource.Play();
                }
            }
            else
            {
                if (walkAudioSource.isPlaying)
                {
                    walkAudioSource.Stop();
                }
            }
        }
    }
}