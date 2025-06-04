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
        [JapaneseLabel("プレイヤーコントローラー")] public PlayerInput moveAction;
        
        //プレイヤーのステータス
        [JapaneseLabel("移動速度")]public float moveSpeed = 5f;
        [JapaneseLabel("ジャンプ力")]public float jumpPower = 8f;
        [JapaneseLabel("最大ジャンプ回数")] public int MaxJumpCount;
        [JapaneseLabel("2回目のジャンプまでのクールタイム")]
        public float jumpCooldown = 0.2f;
        [FormerlySerializedAs("limitSpeed")] [JapaneseLabel("最大降下速度")]
        public float maxFallSpeed = 5f;
        
        //オブジェクト
        [JapaneseLabel("弾のオブジェクト")]public GameObject bullets;
        
        //サウンド関連
        [JapaneseLabel("反射音")] public AudioClip ReflectionSound;
        [JapaneseLabel("射撃音")] public AudioClip ShotSound;
        [JapaneseLabel("被ダメ時音")] public AudioClip DamageSound;
        
        //反射
        [JapaneseLabel("最大反射スタミナ")] public float maxStamina = 100f;
        [JapaneseLabel("反射スタミナ回復量")] public float staminaRecoveryPerSecond = 10f;
        [JapaneseLabel("反射スタミナ消費量")] public float staminaDrainPerSecond = 20f;
        
        //射撃
        [JapaneseLabel("射撃スタミナ回復量")]public float shotStaminaRecoveryPerSecond = 0.1f;
        [JapaneseLabel("オーバーヒート時の射撃スタミナ回復量")]public float overheatRecoveryPerSecond = 0.1f;
        [JapaneseLabel("射撃スタミナ消費量")]public float shotStaminaDrainPerSecond = 0.25f;
        [JapaneseLabel("射撃クールタイム")]public float shotCoolTime = 0.2f;
    }
}