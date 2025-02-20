using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace DefaultNamespace
{
    public class EnemySpawnManager :MonoBehaviour
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            [JapaneseLabel("出現させる敵のプレハブ")] public GameObject enemyPrefab;
            [JapaneseLabel("出現するまでの時間（秒）")] public float spawnDelay;
            [JapaneseLabel("出現位置")] public Transform spawnPoint;
        }
        [SerializeField,JapaneseLabel("！マークのプレハブ")]private GameObject warningMarkerPrefab;
        [SerializeField,JapaneseLabel("！マークを表示する時間（秒）")]private float warningTime = 3f;
        
        public List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();
        private List<GameObject> activeEnemies = new List<GameObject>();

        private void Awake()
        {
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
        public void RemoveEnemy(GameObject enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                Destroy(enemy);
            }
        }
    }
}