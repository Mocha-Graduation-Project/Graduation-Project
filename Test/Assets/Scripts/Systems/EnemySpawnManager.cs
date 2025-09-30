using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Cysharp.Threading.Tasks;
using Scripts.Scriptable;
using TMPro;

namespace Systems
{
    public class EnemySpawnManager :MonoBehaviour
    {
        
        [System.Serializable]
        public class EnemySpawnData
        {
            [JapaneseLabel("敵ID")] public string enemyId;
            [JapaneseLabel("出現させる敵のプレハブ")] public GameObject enemyPrefab;
            [JapaneseLabel("出現するまでの時間（秒）")] public float spawnDelay;
            [JapaneseLabel("出現位置")] public Transform spawnPoint;
        }
        
        [System.Serializable]
        public class ConditionEnemySpawn
        {
            [JapaneseLabel("敵ID")] public string conditionId;
            [JapaneseLabel("特定の敵を倒したら出現する敵")] public GameObject conditionGameObject;

            [JapaneseLabel("条件（敵ID）")]
            public List<string> conditionEnemyIds = new List<string>();

            [JapaneseLabel("倒した後出現までの時間")] public float conditionSpawnDelay;
            [JapaneseLabel("出現位置")] public Transform spawnPoint;
            [HideInInspector] public bool hasSpawned = false;
        }

        [SerializeField,JapaneseLabel("！マークのプレハブ")]private GameObject warningMarkerPrefab;
        [SerializeField,JapaneseLabel("！マークを表示する時間（秒）")]private float warningTime = 3f;
        [SerializeField,JapaneseLabel("敵を倒してからクリア演出までの時間")]private float gameClearDelay;
        [Header("<時間スポーン>")]
        public List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();
        [Header("<条件スポーン>")]
        public List<ConditionEnemySpawn> conditionToSpawn = new List<ConditionEnemySpawn>();
        [SerializeField]private List<GameObject> activeEnemies = new List<GameObject>();

        [SerializeField] SceneButtonManager sceneButtonManager;
        private int enemies;
        private int knockEnemies;
        private int remainnEnemies;
        [SerializeField] TextMeshProUGUI remainingEnemiesText;

        private HashSet<string> defeatedEnemyIds = new HashSet<string>();

        [SerializeField] private SoundData soundData;
        private AudioSource audioSource;
        private AudioClip EnemyDestorySound;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            sceneButtonManager = GameObject.FindObjectOfType<SceneButtonManager>();

            // enemyIdの自動設定
            for (int i = 0; i < enemiesToSpawn.Count; i++)
            {
                if (string.IsNullOrEmpty(enemiesToSpawn[i].enemyId))
                {
                    enemiesToSpawn[i].enemyId = "Enemy_" + i;
                }
            }
            for (int i = 0; i < conditionToSpawn.Count; i++)
            {
                if (string.IsNullOrEmpty(conditionToSpawn[i].conditionId))
                {
                    conditionToSpawn[i].conditionId = "CondEnemy_" + i;
                }
            }


            // 出現する敵の総数（時間スポーン + 条件スポーン）
            enemies = enemiesToSpawn.Count + conditionToSpawn.Count;

            knockEnemies = 0;
            
            remainnEnemies = enemiesToSpawn.Count + conditionToSpawn.Count;
            remainingEnemiesText = GameObject.Find("RemainEnemies").GetComponent<TextMeshProUGUI>();
            remainingEnemiesText.text = remainnEnemies.ToString();
            EnemyDestorySound = soundData.EnemyDestorySound;

            Debug.Log($"このマップの敵総数: {enemies}");

            foreach (var enemy in enemiesToSpawn)
            {
                StartCoroutine(SpawnEnemy(enemy));
            }
        }

        
        IEnumerator SpawnEnemy(EnemySpawnData enemyData)
        {
            float adjustedWarningTime = Mathf.Min(warningTime, enemyData.spawnDelay);
            yield return new WaitForSeconds(enemyData.spawnDelay - adjustedWarningTime);
            GameObject warningMarker = Instantiate(warningMarkerPrefab, enemyData.spawnPoint.position, warningMarkerPrefab.transform.rotation);
            StartCoroutine(BlinkWarningMarker(warningMarker));
            yield return new WaitForSeconds(adjustedWarningTime);
            Destroy(warningMarker);
            GameObject spawnedEnemy = Instantiate(enemyData.enemyPrefab, enemyData.spawnPoint.position, Quaternion.identity);
            activeEnemies.Add(spawnedEnemy);
            spawnedEnemy.name = $"Enemy_{enemyData.enemyId}";
        }
        IEnumerator ConditionSpawnEnemy(ConditionEnemySpawn enemyData)
        {
            float adjustedWarningTime = Mathf.Min(warningTime, enemyData.conditionSpawnDelay);
            yield return new WaitForSeconds(enemyData.conditionSpawnDelay - adjustedWarningTime);
            GameObject warningMarker = Instantiate(warningMarkerPrefab, enemyData.spawnPoint.position, warningMarkerPrefab.transform.rotation);
            StartCoroutine(BlinkWarningMarker(warningMarker));
            yield return new WaitForSeconds(adjustedWarningTime);
            Destroy(warningMarker);
            GameObject spawnedEnemy = Instantiate(enemyData.conditionGameObject, enemyData.spawnPoint.position, Quaternion.identity);
            activeEnemies.Add(spawnedEnemy);
            spawnedEnemy.name = $"ConditionEnemy_{enemyData.conditionId}";
        }
        IEnumerator BlinkWarningMarker(GameObject marker)
        {
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer == null) yield break;

            while (marker != null)
            {
                markerRenderer.enabled = !markerRenderer.enabled;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        public void  RemoveEnemy(GameObject enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                audioSource.PlayOneShot(EnemyDestorySound);
                string defeatedId = GetEnemyIdByObject(enemy);
                activeEnemies.Remove(enemy);
                //Destroy(enemy);
                knockEnemies++;
                remainnEnemies--;
                remainingEnemiesText.text = remainnEnemies.ToString();

                // 撃破ID記録
                defeatedEnemyIds.Add(defeatedId);

                // 条件チェックしてスポーン
                foreach (var condition in conditionToSpawn)
                {
                    if (!condition.hasSpawned &&
                        condition.conditionEnemyIds.TrueForAll(id => defeatedEnemyIds.Contains(id)))
                    {
                        condition.hasSpawned = true;
                        StartCoroutine(ConditionSpawnEnemy(condition));
                    }
                }

                if (activeEnemies.Count == 0 && remainnEnemies == 0)
                {
                    if (LastAttackEffectManager.Instance != null)
                    {
                        LastAttackEffectManager.Instance.PlayLastAttackEffect(enemy.transform,enemy);
                    }
                    GameClearDelayed().Forget();
                }
                else
                {
                    Destroy(enemy);
                }
            }
        }
        private async UniTaskVoid GameClearDelayed()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(gameClearDelay));
            sceneButtonManager.GameClear();
        }
        private string GetEnemyIdByObject(GameObject enemy)
        {
            string name = enemy.name;

            if (name.StartsWith("Enemy_"))
                return name.Replace("Enemy_", "").Trim();
            if (name.StartsWith("ConditionEnemy_"))
                return name.Replace("ConditionEnemy_", "").Trim();

            foreach (var data in enemiesToSpawn)
            {
                if (data.enemyPrefab.name == enemy.name.Replace("(Clone)", "").Trim())
                    return data.enemyId;
            }

            foreach (var condition in conditionToSpawn)
            {
                if (condition.conditionGameObject.name == enemy.name.Replace("(Clone)", "").Trim())
                    return condition.conditionId;
            }

            return "";
        }

    }
}