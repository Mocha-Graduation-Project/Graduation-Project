using System;
using UnityEngine;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scripts.Scriptable
{
    [CreateAssetMenu(fileName = "CharacterParams.asset", menuName = "ScriptableObject/CharacterParams",order = 0)]
    public class CharacterParams : ScriptableObject
    {
        //コントローラー
        [Header("<コントローラー>")]
        [JapaneseLabel("プレイヤーコントローラー")] public PlayerInput moveAction;
        
        //プレイヤーのステータス
        [Space(5)]
        [Header("<プレイヤーのステータス>")]
        [JapaneseLabel("移動速度")]public float moveSpeed = 5f;
        [JapaneseLabel("ジャンプ力")]public float jumpPower = 8f;
        [JapaneseLabel("最大ジャンプ回数")] public int MaxJumpCount;
        [JapaneseLabel("2回目のジャンプまでのクールタイム")]
        public float jumpCooldown = 0.2f;
        [FormerlySerializedAs("limitSpeed")] [JapaneseLabel("最大降下速度")]
        public float maxFallSpeed = 5f;
        
        //オブジェクト
        [Space(5)]
        [Header("<オブジェクト>")]
        [JapaneseLabel("弾のオブジェクト")]public GameObject bullets;
        
        //サウンド関連
        [Space(5)]
        [Header("<サウンド関連>")]
        [JapaneseLabel("反射音")] public AudioClip ReflectionSound;
        [JapaneseLabel("射撃音")] public AudioClip ShotSound;
        [JapaneseLabel("被ダメ時音")] public AudioClip DamageSound;
        
        //反射
        [Space(5)]
        [Header("<反射>")]
        [JapaneseLabel("最大反射スタミナ")] public float maxStamina = 100f;
        [JapaneseLabel("反射スタミナ回復量")] public float staminaRecoveryPerSecond = 10f;
        [JapaneseLabel("反射スタミナ消費量")] public float staminaDrainPerSecond = 20f;
        [JapaneseLabel("quick反射消費量")] public float quickStaminaDrainPerSecond = 20f;
        //射撃
        [Space(5)]
        [Header("<射撃>")]
        [JapaneseLabel("射撃スタミナ回復量")]public float shotStaminaRecoveryPerSecond = 0.1f;
        [JapaneseLabel("オーバーヒート時の射撃スタミナ回復量")]public float overheatRecoveryPerSecond = 0.1f;
        [JapaneseLabel("射撃スタミナ消費量")]public float shotStaminaDrainPerSecond = 0.25f;
        [JapaneseLabel("射撃クールタイム")]public float shotCoolTime = 0.2f;
        
        //弾のステータス
        [Space(5)] [Header("<弾のステータス>")] [JapaneseLabel("初期スピード")]
        public UnityEngine.Vector3 power;
        [JapaneseLabel("最大スピード")] public float maxBulletSpeed;
        [JapaneseLabel("弾くたびに＋〇〇速度を追加")] public float addSpeed = 0.2f;
        [JapaneseLabel("初期ダメージ値")]public int damage = 1;
        [Header("反射時のダメージ")]
        public int[] damageByReflectionCount;
        
        //無敵時間
        [Space(5)][Header("<無敵時間>")]
        [JapaneseLabel("被弾時無敵時間")] public float invincibleDuration = 2.0f;
        [JapaneseLabel("反射後の無敵時間")]　public float reflectInvincible = 1;
        
    }
}