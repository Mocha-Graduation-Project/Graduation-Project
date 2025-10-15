using Scripts.Scriptable;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObject/EnemyData")]
public class EnemyData :  ScriptableObject
{
    public enum EnemyType
    {
        normal,
        shield,
        boss,
    }
    
    public enum MoveType
    {
        dontMove,
        custom,
        vertical,
        horizontal,
    }
    
    public enum EnemyAttackType
    {
        dontAttack,
        playerAim,
        straight,
    }
    
    [Header("敵の基礎ステータス")] 
    [JapaneseLabel("HPの最大値")] public int maxHP;

    [JapaneseLabel("エネミータイプ")] public EnemyType enemyType;
    [JapaneseLabel("動きのタイプ")]  public MoveType moveType;
    [FormerlySerializedAs("enemyAttckType")] [JapaneseLabel("攻撃タイプ")] public EnemyAttackType enemyAttackType;

    [JapaneseLabel("移動するか")] public bool isMove;
    [JapaneseLabel("回転するか")] public bool isRotate;
    [JapaneseLabel("ジャンプするか")] public bool isJump;
    [JapaneseLabel("パターンのクールタイム")] public float coolTime;
    [JapaneseLabel("音源")] public SoundData soundData;
    
    
    [Space(5)] 
    [Header("移動に関するステータス")] 
    [JapaneseLabel("左の最大値")] public float leftRenge;
    [JapaneseLabel("右の最大値")] public float rightRenge;
    [JapaneseLabel("真ん中から横端までにかかる時間")] public float moveHorizontalTime;
    [JapaneseLabel("上の最大値")] public float upRenge;
    [JapaneseLabel("下の最大値")] public float downRenge;
    [JapaneseLabel("真ん中から縦端までにかかる時間")] public float moveVerticalTime;
    [JapaneseLabel("移動完了から次の移動までのクールタイム")] public float moveWaitTime;

    [Space(5)]
    [Header("攻撃に関するステータス")] 
    [JapaneseLabel("発射する弾")] public GameObject bulletObj;
    [JapaneseLabel("弾を発射するレート")] public float bulletRate;
    [JapaneseLabel("攻撃前の表示")] public float beforeAttackTime;
    [JapaneseLabel("警告マークの点滅間隔")] public float blinkDuration;

    [Space(5)]
    [Header("回転に関するステータス")]
    [JapaneseLabel("回転にかける時間")] public float rotateTime;

    [Space(5)]
    [Header("ジャンプに関するステータス")]
    [JapaneseLabel("ジャンプのパワー")] public float jumpPower;
    [JapaneseLabel("ジャンプのクールタイム")] public float jumpCoolTime;
    [JapaneseLabel("地面のレイヤー")] public LayerMask groundLayer;
}
