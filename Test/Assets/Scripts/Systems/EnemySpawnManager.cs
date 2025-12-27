using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Enemy.Basic;
using Scripts.Scriptable;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.VFX;

namespace Systems
{
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] [JapaneseLabel("出現エフェクトのプレハブ")]
        private GameObject warningMarkerPrefab;

        [SerializeField] [JapaneseLabel("出現エフェクトを表示する時間（秒）")]
        private float warningTime = 4f;

        [JapaneseLabel("エネミーを出現させる時間")]
        private float enemySpawnTime = 3f;

        [SerializeField] [JapaneseLabel("敵を倒してからクリア演出までの時間")]
        private float gameClearDelay;

        [Header("<時間スポーン>")] public List<EnemySpawnData> enemiesToSpawn = new();

        [Header("<条件スポーン>")] public List<ConditionEnemySpawn> conditionToSpawn = new();

        [SerializeField] private List<GameObject> activeEnemies = new();

        [SerializeField] private SceneButtonManager sceneButtonManager;
        [SerializeField] private TextMeshProUGUI remainingEnemiesText;

        [SerializeField] private SoundData soundData;

        [JapaneseLabel("死亡演出にかける時間")] [SerializeField]
        private float deathDuration = 1.5f;

        [JapaneseLabel("浮いてる敵の死亡演出の回転数(360*X)")] [SerializeField]
        private float knockDuration = 3f;

        [SerializeField] [JapaneseLabel("地面レイヤー")]
        public LayerMask groundLayer;
        
        private readonly Dictionary<int, string> enemyIdCache = new();
        
        private readonly HashSet<string> defeatedEnemyIds = new();

        private AudioSource audioSource;
        private int enemies;
        private AudioClip EnemyDestorySound;
        private int knockEnemies;
        private int remainnEnemies;

        private int shieldEnemyID = 400;
        private int bossID = 10;
        
        public static EnemySpawnManager Instance { get; private set; }
        public enum EnemyID
        {
            test=0,
            moveBoss=1,
            depthBoss=2,
            humanoidBoss=3,
            nomalEnemyBase=101,
            horizontalNomalEnemy=102,
            verticalNomalEnemy=103,
            noBulletEnemyBase=201,
            horizontalNoBulletEnemy=202,
            verticalNoBulletEnemy=203,
            reflectionEnemy=301,
            jumpEnemy=401,
            shieldEnemy=501,
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            
            audioSource = GetComponent<AudioSource>();

            // enemyIdの自動設定
            for (var i = 0; i < enemiesToSpawn.Count; i++)
                if (string.IsNullOrEmpty(enemiesToSpawn[i].enemyId))
                    enemiesToSpawn[i].enemyId = "Enemy_" + i;

            for (var i = 0; i < conditionToSpawn.Count; i++)
                if (string.IsNullOrEmpty(conditionToSpawn[i].conditionId))
                    conditionToSpawn[i].conditionId = "CondEnemy_" + i;


            // 出現する敵の総数（時間スポーン + 条件スポーン）
            enemies = enemiesToSpawn.Count + conditionToSpawn.Count;

            knockEnemies = 0;

            remainnEnemies = enemiesToSpawn.Count + conditionToSpawn.Count;
            remainingEnemiesText.text = remainnEnemies.ToString();
            EnemyDestorySound = soundData.enemyDestroySound;

            Debug.Log($"このマップの敵総数: {enemies}");

            foreach (var enemyData in enemiesToSpawn) 
            {
                StartCoroutine(SpawnEnemyCoroutine(
                    enemyData.enemyPrefab, 
                    enemyData.spawnPoint, 
                    enemyData.spawnDelay, 
                    $"Enemy_{enemyData.enemyId}",
                    enemyData.excelDataId,
                    enemyData.enemyId // Pass raw ID
                ));
            }
        }
        private IEnumerator SpawnEnemyCoroutine(GameObject prefab, Transform spawnPoint, float spawnDelay, string instanceName,EnemyID excelEnemyID, string rawEnemyId)
        {
            var adjustedWarningTime = Mathf.Min(enemySpawnTime, spawnDelay);
            if (adjustedWarningTime > 0)
            {
                Vector3 warpPos = spawnPoint.position;
                warpPos.z += 0.2f;
                yield return new WaitForSeconds(spawnDelay - adjustedWarningTime);
                var warningMarker = Instantiate(warningMarkerPrefab, warpPos,
                    warningMarkerPrefab.transform.rotation);
                warningMarker.GetComponent<VisualEffect>().SendEvent("OnPlay");
                StartCoroutine(DestroyObjectCoroutine(warningMarker, warningTime));
                //StartCoroutine(BlinkWarningMarker(warningMarker));
                yield return new WaitForSeconds(adjustedWarningTime);
            }
            else
            {
                // スポーン遅延が0か、警告時間より短い場合
                yield return new WaitForSeconds(spawnDelay);
            }

            var spawnedEnemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            EnemyAI enemyAI;
            if ((int)excelEnemyID < bossID || (int)excelEnemyID > shieldEnemyID)
            {
                enemyAI = spawnedEnemy.GetComponentInChildren<EnemyAI>();
            }
            else
            {
                enemyAI = spawnedEnemy.GetComponent<EnemyAI>();
            }
            enemyAI.SetNumber((int)excelEnemyID);
            
            activeEnemies.Add(spawnedEnemy);
            spawnedEnemy.name = instanceName;

            // Note: Cache the ID
            if (spawnedEnemy != null)
            {
                enemyIdCache[spawnedEnemy.GetInstanceID()] = rawEnemyId;
            }
        }

        private IEnumerator DestroyObjectCoroutine(GameObject obj, float destroyTime)
        {
            yield return new WaitForSeconds(destroyTime);
            
            Destroy(obj);
        }
        private IEnumerator BlinkWarningMarker(GameObject marker)
        {
            var markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer == null) yield break;

            while (marker != null)
            {
                markerRenderer.enabled = !markerRenderer.enabled;
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void RemoveEnemy(GameObject enemy, GameObject deathEffectPrefab)
        {
            if (!activeEnemies.Contains(enemy)) return;
            audioSource.PlayOneShot(EnemyDestorySound);
            var defeatedId = GetEnemyIdByObject(enemy);
            activeEnemies.Remove(enemy);
            knockEnemies++;
            remainnEnemies--;
            remainingEnemiesText.text = remainnEnemies.ToString();

            // 撃破ID記録
            defeatedEnemyIds.Add(defeatedId);

            // 条件チェックしてスポーン
            foreach (var condition in conditionToSpawn)
            {
                if (condition.hasSpawned ||
                    !condition.conditionEnemyIds.TrueForAll(id => defeatedEnemyIds.Contains(id))) continue;
                condition.hasSpawned = true;
                StartCoroutine(SpawnEnemyCoroutine(
                    condition.conditionGameObject,
                    condition.spawnPoint,
                    condition.conditionSpawnDelay,
                    $"ConditionEnemy_{condition.conditionId}"
                    ,condition.excelDataId
                    ,condition.conditionId // Pass raw ID
                ));
            }

            if (activeEnemies.Count == 0 && remainnEnemies == 0)
            {
                if (LastAttackEffectManager.Instance != null)
                {
                    sceneButtonManager.ChangeState(SceneButtonManager.State.Clear);
                    LastAttackEffectManager.Instance.PlayLastAttackEffect(enemy.transform, enemy);

                    if (enemy.GetComponent<NomalEnemy>())
                    {
                        var normalEnemy = enemy.GetComponent<NomalEnemy>();
                        normalEnemy.enabled = false;
                        deathEffectPrefab.GetComponent<VisualEffect>().SendEvent("OnPlay");
                        if (enemy.CompareTag("FryEnemy"))
                        {
                            StartCoroutine(FryEnemyDeathAnimation(enemy,true));
                        }
                        else
                        {
                            StartCoroutine(NormalEnemyDeathAnimation(enemy,true));
                        }
                    }

                    if (enemy.GetComponentInChildren<DepthBoss>())
                    {
                        var bossEnemy = enemy.GetComponentInChildren<DepthBoss>();
                        bossEnemy.enabled = false;
                        deathEffectPrefab.GetComponent<VisualEffect>().SendEvent("OnPlay");
                        StartCoroutine(DepthBoss(enemy,true));
                    }
                    if (enemy.GetComponentInChildren<MoveBoss>())
                    {
                        deathEffectPrefab.GetComponent<VisualEffect>().SendEvent("OnPlay");
                        StartCoroutine(MoveBoss(enemy,true));
                    }
                    else
                    {
                        deathEffectPrefab.GetComponent<VisualEffect>().SendEvent("OnPlay");
                        StartCoroutine(NormalEnemyDeathAnimation(enemy,true));
                    }
                }
            }
            else
            {
                if (enemy.GetComponent<NomalEnemy>())
                {
                    var normalEnemy = enemy.GetComponent<NomalEnemy>();
                    normalEnemy.enabled = false;
                    deathEffectPrefab.GetComponent<VisualEffect>().SendEvent("OnPlay");
                    if (enemy.CompareTag("FryEnemy"))
                        StartCoroutine(FryEnemyDeathAnimation(enemy,false));
                    else
                        StartCoroutine(NormalEnemyDeathAnimation(enemy,false));
                }
                else
                {
                    deathEffectPrefab.GetComponent<VisualEffect>().SendEvent("OnPlay");
                    StartCoroutine(NormalEnemyDeathAnimation(enemy,false));
                }
            }
        }

        private IEnumerator FryEnemyDeathAnimation(GameObject enemy,bool clear)
        {
            var duration = 1.5f; // 演出にかける時間
            var startTime = Time.time;
            var startPosition = enemy.transform.position;

            var currentRotationSpeed = 360f * 3f; // 3秒で3回転

            var targetY = startPosition.y - 100f;
            var rayDistance = 200f;
            const float OFFSET_TO_BOTTOM = 0.5f;

            RaycastHit hit;
            if (Physics.Raycast(startPosition, Vector3.down, out hit, rayDistance, groundLayer))
                targetY = hit.point.y + OFFSET_TO_BOTTOM;
            var enemyCollider = enemy.GetComponent<Collider>();
            if (enemyCollider != null) enemyCollider.enabled = false;
            var isGrounded = false;

            while (Time.time < startTime + duration)
            {
                var elapsed = Time.time - startTime;
                var progress = elapsed / duration;

                // 回転
                enemy.transform.Rotate(0, 0, currentRotationSpeed * Time.deltaTime, Space.Self);

                // 落下位置
                var dropAmount = 5f;
                var newPosition = startPosition + new Vector3(
                    0,
                    Mathf.Lerp(0, -dropAmount, progress * progress),
                    0
                );

                // 地面到達チェックと制限
                if (newPosition.y <= targetY)
                {
                    newPosition.y = targetY; // 地面より下にいかないように固定

                    if (!isGrounded)
                    {
                        currentRotationSpeed = 0f;
                        isGrounded = true;
                    }
                }

                enemy.transform.position = newPosition;

                // 演出時間の調整
                if (isGrounded && Time.time > startTime + 0.5f) break;

                yield return null;
            }

            // 演出終了後、最終的な位置を地面に固定
            enemy.transform.position = new Vector3(
                enemy.transform.position.x,
                targetY,
                enemy.transform.position.z
            );

            Destroy(enemy);
            if (clear)
            { 
                GameClearDelayed().Forget();
            }
        }

        private IEnumerator NormalEnemyDeathAnimation(GameObject enemy,bool clear)
        {
            var duration = 1.5f; // 演出にかける時間
            var startTime = Time.time;
            yield return new WaitForSeconds(duration);
            Destroy(enemy);
            if (clear)
            { 
                GameClearDelayed().Forget();
            }
        }

        private IEnumerator MoveBoss(GameObject boss,bool clear)
        {
            //Script無効化
            var bossEnemy = boss.GetComponentInChildren<MoveBoss>();
            bossEnemy.Defeat();
            bossEnemy.enabled = false;
            
            var duration = 3.0f; // 演出にかける時間
            var startTime = Time.time;
            yield return new WaitForSeconds(duration);
            Destroy(boss);
            if (clear)
            { 
                GameClearDelayed().Forget();
            }
        }
        private IEnumerator DepthBoss(GameObject boss,bool clear)
        {
            var bossEnemy= boss.GetComponentInChildren<DepthBoss>();
            bossEnemy.DepthBossDeath();
            bossEnemy.enabled = false;
            
            var duration = 3.0f; // 演出にかける時間
            var startTime = Time.time;
            yield return new WaitForSeconds(duration);
            Destroy(boss);
            if (clear)
            { 
                GameClearDelayed().Forget();
            }
        }
        private async UniTaskVoid GameClearDelayed()
        {
            float duration = gameClearDelay;
            float elapsed = 0f;
            float startScale = Time.timeScale;
            float targetScale = 0.2f; // Slow down to 20% speed
            
            var mapManager = sceneButtonManager.MapManager;
            AudioSource bgmSource = mapManager != null ? mapManager.AudioSource : null;
            float startPitch = bgmSource != null ? bgmSource.pitch : 1f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                float currentScale = Mathf.Lerp(startScale, targetScale, t);
                Time.timeScale = currentScale;
                
                if (bgmSource != null)
                {
                    bgmSource.pitch = Mathf.Lerp(startPitch, targetScale, t);
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            
            Time.timeScale = targetScale;
            if (bgmSource != null)
            {
                bgmSource.pitch = targetScale;
            }

            if (this == null) return;
            if (sceneButtonManager == null) return;

            sceneButtonManager.GameClear();
        }

        private string GetEnemyIdByObject(GameObject enemy)
        {
            if (enemy == null) return "";
            
            // Optimization: Try cache first
            if (enemyIdCache.TryGetValue(enemy.GetInstanceID(), out var id))
            {
                // Remove from cache as it's being removed
                enemyIdCache.Remove(enemy.GetInstanceID());
                return id;
            }

            var name = enemy.name;

            if (name.StartsWith("Enemy_"))
                return name.Replace("Enemy_", "").Trim();
            if (name.StartsWith("ConditionEnemy_"))
                return name.Replace("ConditionEnemy_", "").Trim();

            foreach (var data in enemiesToSpawn)
                if (data.enemyPrefab.name == enemy.name.Replace("(Clone)", "").Trim())
                    return data.enemyId;

            foreach (var condition in conditionToSpawn)
                if (condition.conditionGameObject.name == enemy.name.Replace("(Clone)", "").Trim())
                    return condition.conditionId;

            return "";
        }

        [Serializable]
        public class EnemySpawnData
        {
            [JapaneseLabel("敵ID")] public string enemyId;
            [JapaneseLabel("出現させる敵のプレハブ")] public GameObject enemyPrefab;
            [JapaneseLabel("出現するまでの時間（秒）")] public float spawnDelay;
            [JapaneseLabel("出現位置")] public Transform spawnPoint;
            [JapaneseLabel("ExcelDataのID")] public EnemyID excelDataId;
        }

        [Serializable]
        public class ConditionEnemySpawn
        {
            [JapaneseLabel("敵ID")] public string conditionId;
            [JapaneseLabel("特定の敵を倒したら出現する敵")] public GameObject conditionGameObject;

            [JapaneseLabel("条件（敵ID）")] public List<string> conditionEnemyIds = new();

            [JapaneseLabel("倒した後出現までの時間")] public float conditionSpawnDelay;
            [JapaneseLabel("出現位置")] public Transform spawnPoint;
            [JapaneseLabel("ExcelDataのID")] public EnemyID excelDataId;
            [HideInInspector] public bool hasSpawned;
        }
    }
}