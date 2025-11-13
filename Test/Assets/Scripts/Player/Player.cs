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
    // 必須コンポーネントを明記
    [RequireComponent(typeof(PlayerMove))]
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class Player : MonoBehaviour
    {
        //コンポーネント
        public static Player Instance;
        [NonSerialized] public PlayerMove PlayerMove;
        [NonSerialized] public PlayerCombat PlayerCombat;
        [NonSerialized] public PlayerInputHandler PlayerInputHandler;
        [NonSerialized] public PlayerStatus PlayerStatus;
        [NonSerialized] public Animator Animator;

        //参照
        [SerializeField] private CharacterParams characterParams;
        [SerializeField] private SoundData soundData;
        [SerializeField] private GameObject playerUI;
        [NonSerialized] public PlayerInput MoveAction;
        [SerializeField] private AudioSource audioSource1;

        [NonSerialized] public Vector2 InputMove = Vector2.zero;
        [NonSerialized] public Slider staminaSlider;
        [NonSerialized] public Image BulletUI;
        
        //プレイヤーの状態
        [NonSerialized] public int direction = 1;
        [NonSerialized][JapaneseLabel("移動中か")] public bool isMove = true;

        //オブジェクト
        [JapaneseLabel("矢印")]public GameObject Arrow;

        [Header("<エフェクトリスト>")]
        [SerializeField]
        private GameObject[] effectPrefabs; 
        [Header("<エフェクト生存時間リスト>")]
        [SerializeField]
        private float[] effectDurations;
        [JapaneseLabel("バットのアニメーションからエフェクトがでるまでの時間")] private float butEffectDuration = 0.1f;
        
        //サウンド関連
        [NonSerialized] public AudioClip reflectionSound;
        [NonSerialized] public AudioClip damageSound;

        private readonly System.Collections.Generic.Dictionary<float, WaitForSeconds> waitCache = new System.Collections.Generic.Dictionary<float, WaitForSeconds>();
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            
            // コンポーネントをキャッシュ
            PlayerMove = GetComponent<PlayerMove>();
            PlayerCombat = GetComponent<PlayerCombat>();
            PlayerInputHandler = GetComponent<PlayerInputHandler>();
            PlayerStatus = GetComponentInChildren<PlayerStatus>();
            Animator = GetComponent<Animator>();

            staminaSlider = playerUI.GetComponentInChildren<Slider>();
            BulletUI = playerUI.GetComponentInChildren<Image>();
            
            PlayerParamReset();
        }

        private void PlayerParamReset()
        {
            MoveAction = characterParams.moveAction;
            reflectionSound = soundData.reflectionSound;
            damageSound = soundData.damageSound;
            butEffectDuration = characterParams.butEffectDuration;
            
            // 各コンポーネントの初期化メソッドを呼ぶ
            PlayerCombat.Initialize(characterParams, soundData);
        }
        
        [Obsolete("Obsolete")]
        private void Start()
        {
            Arrow.SetActive(false);
        }
        
        private void Update()
        {
           // プレイヤー自身のUpdateはZ軸補正のみ
            Vector3 temp = transform.position;
            temp.z = 0f;
            transform.position = temp;
        }

        public void Ground(bool isGrounded)
        {
            PlayerMove.SetGroundState(isGrounded);
        }
        
        public void PlayEffect(int effectIndex)
        {
            // (PlayEffectの中身は変更なし)
            if (effectPrefabs == null || effectIndex < 0 || effectIndex >= effectPrefabs.Length) return;
            if (effectDurations == null || effectIndex >= effectDurations.Length) { }
            
            GameObject effectToPlay = effectPrefabs[effectIndex];
            if (effectToPlay != null)
            {
                float duration = (effectDurations != null && effectIndex < effectDurations.Length) 
                    ? effectDurations[effectIndex] 
                    : butEffectDuration; 
                ShowEffectForDuration(effectToPlay, duration);
            }
        }
        
        private void ShowEffectForDuration(GameObject effectObject, float duration)
        {
            VisualEffect effect = effectObject.GetComponent<VisualEffect>();
            effect.SendEvent("OnPlay");
        }

        public void PlayDamageSound()
        {
            audioSource1.PlayOneShot(damageSound);
        }
    }
}