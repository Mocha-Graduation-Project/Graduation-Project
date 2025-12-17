using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class EnemyDataEntity
{
    public int id;
    public string enemyName;
    public int maxHP;
    public enum EnemyType
    {
        normal,
        shield,
        boss,
        humanoid,
    }
    public EnemyType  enemyType;
    public enum PlayerLookType
    {
        look,
        lookY,
        dontLook,
    }
    public PlayerLookType playerLookType;
    public float coolTime;
    public enum MoveType
    {
        dontMove,
        custom,
        vertical,
        horizontal,
    }
    public MoveType moveType;
    public float leftRenge;
    public float rightRenge;
    public float horizontalTime;
    public float upRenge;
    public float downRenge;
    public float verticalTime;
    public float waitTime;
    public enum EnemyAttackType
    {
        dontAttack,
        playerAim,
        straight,
    }
    public EnemyAttackType enemyAttackType;
    public float bulletRate;
    public float beforeAttackTime;
    public float brinkDuration;
    public bool isRotate;
    public float rotateTime;
    public bool isJump;
    public float jumpPower;
    public float jumpCoolTime;
}
