using System;
using System.Collections;
using Scriptable;
using Component;
using Scripts.Scriptable;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Player
{

    public class PlayerStatus : MonoBehaviour
    {
        public static PlayerStatus Instance;
        private static readonly int IsGround = Animator.StringToHash("isGround");
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private CharacterData characterData;
        [SerializeField] private int playerHp;

        [FormerlySerializedAs("sceneManager")]
        private SceneButtonManager sceneButtonManager;

        [JapaneseLabel("地面レイヤー")] private LayerMask groundLayer;

        [SerializeField] private Animator animator;

        [JapaneseLabel("被弾時無敵時間")]
        private float invincibleDuration = 2.0f;

        //private string enemyBulletTag = "EnemyBullet";

        private bool invincible;
        private bool isGrounded;

        //private string playerBulletTag = "Bullet";

        public int PlayerHp => playerHp;

        private global::Player.Player player => global::Player.Player.Instance;
        private Coroutine invincibilityCoroutine;
        [JapaneseLabel("被弾エフェクト")] public GameObject hitEffect;
        VisualEffect effect;

        private void Awake()
        {
            SetScriptable();
            effect = hitEffect.GetComponent<VisualEffect>();
        }

        private void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
            StartSetUp();
            
            sceneButtonManager = SceneButtonManager.Instance;
            if (sceneButtonManager == null)
            {
                 // Maybe it's not ready yet or not in scene?
                 // But we avoided Find.
            }
        }

        private void SetScriptable()
        {
            invincibleDuration = characterParams.invincibleDuration;
            groundLayer = characterParams.groundLayer;
        }

        public bool IsGrounded()
        {
            return isGrounded;
        }

        public void Damage(int damage)
        {
            if (invincible) return; // 無敵時間中ならダメージを受けない

            if (hitEffect != null)
            {
                effect.SendEvent("OnPlay");
            }

            playerHp -= damage;
            if (UILife.Instance != null)
            {
                UILife.Instance.RemoveLife();
            }

            Debug.Log("PlayerHP:" + playerHp);
            player.PlayDamageSound();

            if (playerHp <= 0 && sceneButtonManager != null)
            {
                // プレイヤー死亡イベントをログ
                LudiscanManager.Instance.LogPlayerDeath(transform.position);
                sceneButtonManager.GameOver();
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
            for (var i = 0; i < characterData.InitialHp; i++) UILife.Instance.AddLife();
        }
    }
}