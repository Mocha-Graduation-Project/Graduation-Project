using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StartUpEnemyData : MonoBehaviour
{
    //[System.Serializable]
    public class EnemiesData
    {
        [JapaneseLabel("ID")] public int id;
        [JapaneseLabel("名称")] public string enemyName;
        [JapaneseLabel("最大HP")] public int maxHP;
        public EnemyType enemyType;
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
        normal,
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

    public List<EnemiesData> enemiesData = new();
    
    public static bool IsInitialized { get; private set; } = false; 
    public static StartUpEnemyData instance;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize() 
    {
        new GameObject("StartupInitializer", typeof(StartUpEnemyData));
    }
    
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        string dataPath = "CSVData/EnemyData";
        TextAsset dataAsset = (TextAsset)Resources.Load<TextAsset>(dataPath);
        StringReader reader = new StringReader(dataAsset.text);

        while (reader.Peek() != -1)
        {
            string lineData = reader.ReadLine();
            string[] lineSprit = lineData.Split(',');
            
            EnemiesData enemyData = new EnemiesData();

            int.TryParse(lineSprit[0], out enemyData.id);
            enemyData.enemyName = lineSprit[1];
            int.TryParse(lineSprit[2], out enemyData.maxHP);
            switch (lineSprit[3])
            {
                case "normal":
                    enemyData.enemyType = EnemyType.normal;
                    break;
                case "shield":
                    enemyData.enemyType = EnemyType.shield;
                    break;
                case "boss":
                    enemyData.enemyType = EnemyType.boss;
                    break;
                case "humanoid":
                    enemyData.enemyType = EnemyType.humanoid;
                    break;
            }
            switch (lineSprit[4])
            {
                case "look":
                    enemyData.playerLookType = PlayerLookType.look;
                    break;
                case "lookY":
                    enemyData.playerLookType = PlayerLookType.lookY;
                    break;
                case "dontLook":
                    enemyData.playerLookType = PlayerLookType.dontLook;
                    break;
            }
            float.TryParse(lineSprit[5], out enemyData.coolTime);
            switch (lineSprit[6])
            {
                case "dontMove":
                    enemyData.moveType = MoveType.dontMove;
                    break;
                case "custom":
                    enemyData.moveType = MoveType.custom;
                    break;
                case "vertical":
                    enemyData.moveType = MoveType.vertical;
                    break;
                case "horizontal":
                    enemyData.moveType = MoveType.horizontal;
                    break;
            }

            float.TryParse(lineSprit[7], out enemyData.leftRenge);
            float.TryParse(lineSprit[8], out enemyData.rightRenge);
            float.TryParse(lineSprit[9], out enemyData.horizontalTime);
            float.TryParse(lineSprit[10], out enemyData.upRenge);
            float.TryParse(lineSprit[11], out enemyData.downRenge);
            float.TryParse(lineSprit[12], out enemyData.verticalTime);
            float.TryParse(lineSprit[13], out enemyData.waitTime);
            switch (lineSprit[14])
            {
                case "dontAttack":
                    enemyData.enemyAttackType = EnemyAttackType.dontAttack;
                    break;
                case "playerAim":
                    enemyData.enemyAttackType = EnemyAttackType.playerAim;
                    break;
                case "straight":
                    enemyData.enemyAttackType = EnemyAttackType.straight;
                    break;
            }
            float.TryParse(lineSprit[15], out enemyData.bulletRate);
            float.TryParse(lineSprit[16], out enemyData.beforeAttackTime);
            float.TryParse(lineSprit[17], out enemyData.brinkDuration);
            switch (lineSprit[18])
            {
                case "TRUE":
                    enemyData.isRotate = true;
                    break;
                case "FALSE":
                    enemyData.isRotate = false;
                    break;
            }
            float.TryParse(lineSprit[19], out enemyData.rotateTime);
            switch (lineSprit[20])
            {
                case "TRUE":
                    enemyData.isJump = true;
                    break;
                case "FALSE":
                    enemyData.isJump = false;
                    break;
            }
            float.TryParse(lineSprit[21], out enemyData.jumpPower);
            float.TryParse(lineSprit[22], out enemyData.jumpCoolTime);

            enemiesData.Add(enemyData);
        }

        // for (int i = 1; i < typeEnemy; i++)
        // {
        //     Debug.Log("ID:" + enemiesData[i].id);
        // }
        
        IsInitialized = true;
    }
}