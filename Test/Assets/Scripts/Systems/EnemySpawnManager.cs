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
            yield return new WaitForSeconds(enemyData.spawnDelay);
            GameObject spawnedEnemy = Instantiate(enemyData.enemyPrefab, enemyData.spawnPoint.position, Quaternion.identity);
            activeEnemies.Add(spawnedEnemy);
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