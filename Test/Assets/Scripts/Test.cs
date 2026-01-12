using System;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [Serializable]
    public class EnemiesData
    {
        [JapaneseLabel("ID")] public int id;
        [JapaneseLabel("名称")] public string enemyName;
        [JapaneseLabel("最大HP")] public int maxHP;
        public EnemyType enmyType;
        public PlayerLookType playerLookType;
        public float coolTime;
        public MoveType moveType;
        public float leftRenge;
        public float rightRenge;
        public float horizontalTime;
        public float upRenge;
        public float downRenge;
        public float verticalTime;
        public float waitTime;
        public EnemyAttackType enemyAttackType;
        [JapaneseLabel("発射レート")] public float bulletRate;
        public float beforeAttackTime;
        public float brinkDuration;
        public bool isRotate;
        public float rotateTime;
        public bool isJump;
        public float jumpPower;
        public float jumpCoolTime;
    }
    
    public enum EnemyType
    {
        nomal,
        shield,
        boss,
        humanoid,
    }
    public enum PlayerLookType
    {
        look,
        lookY,
        dontLook,
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
    
    private static int typeEnemy = 13 + 1;

    [SerializeField]public List<EnemiesData> enemiesData = new (typeEnemy);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < typeEnemy; i++)
        {
            EnemiesData enemydata=new EnemiesData();
            enemydata.id = i;
            enemydata.enemyName = "aaa";
            enemydata.maxHP = 8;
            enemiesData.Add(enemydata);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
