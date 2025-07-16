using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Scripts.Scriptable;

namespace Scripts
{

    public class PlayerStatus : MonoBehaviour
    {
        public static PlayerStatus Instance;
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private CharacterData characterData;
        [SerializeField] private int playerHp;

        [SerializeField] private UILife uiLife;

        [FormerlySerializedAs("sceneManager")] [SerializeField]
        private SceneButtonManager sceneButtonManager;

        [SerializeField] [JapaneseLabel("地面レイヤー")]
        private LayerMask[] groundLayer;

        [SerializeField] [JapaneseLabel("足元")] private Transform groundCheck;

        [SerializeField] private Animator animator;

        [JapaneseLabel("被弾時無敵時間")]
        private float invincibleDuration = 2.0f;

        private readonly float checkDistance = 0.05f; // Raycastの長さ
        private string enemyBulletTag = "EnemyBullet";

        private bool invincible;
        private bool isGrounded;

        private string playerBulletTag = "Bullet";

        public int PlayerHp => playerHp;

        private Player player => Player.Instance;
        private Coroutine invincibilityCoroutine;
        

        private void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
            StartSetUp();
            uiLife = uiLife.GetComponent<UILife>();
            sceneButtonManager = GameObject.FindObjectOfType<SceneButtonManager>();
            //sceneButtonManager = GameObject.Find("SceneManager").GetComponent<SceneButtonManager>();
        }

        private void Update()
        {
            CheckGround();
        }

        private void SetScriptable()
        {
            invincibleDuration = characterParams.invincibleDuration;
        }

        private void CheckGround()
        {
            foreach (var ground in groundLayer)
            {
                isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, checkDistance, ground);
                
                animator.SetBool("isGround", isGrounded);
                
                Debug.DrawRay(groundCheck.position, Vector2.down * checkDistance, Color.red);
            }
        }

        public bool IsGrounded()
        {
            return isGrounded;
        }

        // void OnTriggerEnter2D(Collider2D other)
        // {
        //     if (other.CompareTag(playerBulletTag) || other.CompareTag(enemyBulletTag))
        //     {
        //         Damage(1);
        //     }
        // }
        public void Damage(int damage)
        {
            if (invincible) return; // 無敵時間中ならダメージを受けない

            playerHp -= damage;
            uiLife.RemoveLife();
            Debug.Log("PlayerHP:" + playerHp);
            player.PlayDamageSound();

            if (playerHp <= 0 && sceneButtonManager != null)
            {
                player.PlayerReset();
                sceneButtonManager.Retry();
            }
            else
                StartCoroutine(InvincibilityCoroutine()); // 無敵時間開始
        }

        private IEnumerator InvincibilityCoroutine()
        {
            invincible = true;
            yield return new WaitForSeconds(invincibleDuration);
            invincible = false;
        }

        public IEnumerator ReflectInvincibilityCoroutine(float reflectInvincible)
        {
            invincible = true;
            yield return new WaitForSeconds(reflectInvincible);
            invincible = false;
        }
        
        public void StartReflectInvincibility(float duration)
        {
            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
                invincibilityCoroutine = null;
            }
            invincibilityCoroutine = StartCoroutine(ReflectInvincibilityCoroutine(duration));
        }

        public void StartSetUp()
        {
            Debug.Log("StartSetUp");
            playerHp = characterData.InitialHp;
            for (var i = 0; i < characterData.InitialHp; i++) uiLife.AddLife();
        }
    }
}