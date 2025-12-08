using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Scriptable
{
    [CreateAssetMenu(fileName = "CharacterParams.asset", menuName = "ScriptableObject/CharacterParams", order = 0)]
    public class CharacterParams : ScriptableObject
    {
        //コントローラー
        [Header("<コントローラー>")] [JapaneseLabel("プレイヤーコントローラー")]
        public PlayerInput moveAction;

        //プレイヤーのステータス
        [Space(5)] [Header("<プレイヤーのステータス>")] [JapaneseLabel("移動速度")]
        public float moveSpeed = 5f;

        [JapaneseLabel("ジャンプ力")] public float jumpPower = 8f;
        [JapaneseLabel("最大ジャンプ回数")] public int MaxJumpCount;
        [JapaneseLabel("ジャンプキャンセルするかどうか")] public bool jumpCanceled;

        [JapaneseLabel("2回目のジャンプまでのクールタイム")] public float jumpCooldown = 0.2f;

        [FormerlySerializedAs("limitSpeed")] [JapaneseLabel("最大降下速度")]
        public float maxFallSpeed = 5f;

        [JapaneseLabel("判定消えるまでの時間")]　public float collisionRadius;

        [Space(5)] [Header("<地面判定レイヤー>")] [JapaneseLabel("地面レイヤー")]
        public LayerMask groundLayer;

        //オブジェクト
        [Space(5)] [Header("<オブジェクト>")] [JapaneseLabel("弾のオブジェクト")]
        public GameObject bullets;

        //反射
        [Space(5)] [Header("<反射>")] [JapaneseLabel("最大反射スタミナ")]
        public float maxStamina = 100f;

        [JapaneseLabel("反射スタミナ回復量")] public float staminaRecoveryPerSecond = 10f;

        [JapaneseLabel("反射スタミナ消費量")] public float staminaDrainPerSecond = 20f;

        [JapaneseLabel("スティックで弾きが発動するデットゾーン")]　public float deadZone = 0.25f;
        [Header("弾き方向の調整")]
        [JapaneseLabel("スナップする角度の間隔 (45=8方向, 30=12方向)")]
        public float angleStep = 45f; 
        
        //射撃
        [Space(5)] [Header("<射撃>")] [JapaneseLabel("最大射撃スタミナ")]
        public float maxShotStamina = 1f;

        [JapaneseLabel("射撃スタミナ回復量")] public float shotStaminaRecoveryPerSecond = 0.1f;
        [JapaneseLabel("オーバーヒート時の射撃スタミナ回復量")] public float overheatRecoveryPerSecond = 0.1f;
        [JapaneseLabel("射撃スタミナ消費量")] public float shotStaminaDrainPerSecond = 0.25f;
        [JapaneseLabel("射撃クールタイム")] public float shotCoolTime = 0.2f;

        //弾のステータス
        [Space(5)] [Header("<弾のステータス>")] [JapaneseLabel("初期スピード")]
        public Vector3 power;

        [JapaneseLabel("最大スピード")] public float maxBulletSpeed;
        [JapaneseLabel("弾くたびに＋〇〇速度を追加")] public float addSpeed = 0.2f;
        [JapaneseLabel("初期ダメージ値")] public int damage = 1;
        [JapaneseLabel("ヒットストップ時間")] public float hitStopDuration;

        [Header("反射時のダメージ")] public int[] damageByReflectionCount;

        [JapaneseLabel("反射クールタイム時間")] public float reflectCooldown = 0.5f;


        //無敵時間
        [Space(5)] [Header("<無敵時間>")] [JapaneseLabel("被弾時無敵時間")]
        public float invincibleDuration = 2.0f;

        [JapaneseLabel("反射後の無敵時間")]　public float reflectInvincible = 1;

        [Space(5)] [Header("<エフェクト>")] [JapaneseLabel("バットのアニメーション開始からエフェクトがでるまでの時間")]
        public float butEffectDuration = 0.1f;
    }
}