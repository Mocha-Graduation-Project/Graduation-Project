using Cysharp.Threading.Tasks;
using UnityEngine;
using LudiscanApiClient.Runtime.ApiClient;
using LudiscanApiClient.Runtime.ApiClient.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Component
{
    /// <summary>
    /// Ludiscan API クライアント統合マネージャー
    /// ゲームのセッション、各種ロギング、データアップロードを一元管理します
    /// </summary>
    public class LudiscanManager : MonoBehaviour
    {
        private static LudiscanManager instance;
        private int currentProjectId = -1;
        private int currentSessionId = -1;
        private bool isInitialized = false;
        private float sessionStartTimeStamp = -1f;
        private SemaphoreSlim sessionEndLock = new SemaphoreSlim(1, 1);

        public static LudiscanManager Instance
        {
            get
            {
                return instance;
            }
        }



        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            if (isInitialized)
                return;

            try
            {
                // LudiscanConfig がない場合はデフォルト値を使用
                string apiBaseUrl = "http://localhost:3211";
                string xapiKey = "ludi_fb3e6386762dcf6692a317faae7418dc";
                int timeoutSeconds = 10;
                currentProjectId = 8;

                // Ludiscan APIクライアント初期化
                var config = new LudiscanClientConfig(
                    apiBaseUrl,
                    xapiKey
                )
                {
                    TimeoutSeconds = timeoutSeconds
                };

                LudiscanClient.Initialize(config);

                // 各ロガーを初期化
                GeneralEventLogger.Initialize(initialCapacity: 2000);
                FieldObjectLogger.Initialize(initialCapacity: 1000);
                PositionLogger.Initialize(10000);
                EventScreenshotCapture.Initialize(false);

                isInitialized = true;
                Debug.Log("[Ludiscan] Manager initialized successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Ludiscan] Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 新しいステージセッションを開始
        /// </summary>
        public async UniTask StartStageSessionAsync(string mapName, int playerId = 0)
        {
            if (!isInitialized)
            {
                Debug.LogError("[Ludiscan] Manager is not initialized!");
                return;
            }

            try
            {
                // プロジェクト取得（キャッシュ）
                if (currentProjectId == -1)
                {
                    var projects = await LudiscanClient.Instance.GetProjects();
                    if (projects == null || projects.Count == 0)
                    {
                        Debug.LogError("[Ludiscan] No projects available!");
                        return;
                    }
                    currentProjectId = decimal.ToInt32(projects[0].Id);
                }

                // セッション作成（マップ名をセッション名として使用）
                var session = await LudiscanClient.Instance.CreateSession(
                    currentProjectId,
                    mapName
                );

                if (session != null)
                {
                    currentSessionId = decimal.ToInt32(session.SessionId);
                    sessionStartTimeStamp = Time.time;
                    Debug.Log($"[Ludiscan] Session created: {currentSessionId} for map: {mapName}");

                    // メタデータを設定
                    await LudiscanClient.Instance.PutMapName(currentProjectId, currentSessionId, mapName);

                    // ロガーの初期化
                    GeneralEventLogger.Instance.Clear();
                    FieldObjectLogger.Instance.Clear();

                    // 位置情報ロギング開始
                    PositionLogger.Instance.OnLogPosition = () =>
                    {
                        var player = Object.FindFirstObjectByType<Player.Player>();
                        if (player == null)
                        {
                            Debug.LogWarning("[Ludiscan] Player not found!");
                            return new List<PositionEntry>();
                        }

                        return new List<PositionEntry>
                        {
                            new PositionEntry
                            {
                                PlayerId = playerId,
                                Position = player.transform.position,
                                OffsetTimeStamp = (ulong)((Time.time - sessionStartTimeStamp) * 1000)
                            }
                        };
                    };

                    PositionLogger.Instance.StartLogging((int)(0.25F * 1000));
                    EventScreenshotCapture.Instance.StartCapture();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Ludiscan] Failed to start session: {ex.Message}");
            }
        }

        /// <summary>
        /// 現在のセッションを終了（重複実行防止）
        /// </summary>
        public async UniTask EndCurrentSessionAsync()
        {
            // 初期チェック（ロック前）
            if (currentSessionId == -1)
                return;

            // セッション終了処理の排他制御
            await sessionEndLock.WaitAsync();
            try
            {
                // ダブルチェック（ロック後）
                if (currentSessionId == -1)
                    return;

                int sessionIdToFinish = currentSessionId;

                // 位置情報ロギング停止
                PositionLogger.Instance.StopLogging();

                // 残りのデータをアップロード
                await UploadAllLogsAsync();

                // セッション終了
                await LudiscanClient.Instance.FinishSession(currentProjectId, sessionIdToFinish);
                Debug.Log($"[Ludiscan] Session finished: {sessionIdToFinish}");

                currentSessionId = -1;
                sessionStartTimeStamp = -1f;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Ludiscan] Failed to end session: {ex.Message}");
            }
            finally
            {
                EventScreenshotCapture.Instance.StopCapture();
                sessionEndLock.Release();
            }
        }

        /// <summary>
        /// 全てのログデータをアップロード
        /// </summary>
        private async UniTask UploadAllLogsAsync()
        {
            if (currentSessionId == -1)
                return;

            try
            {
                // 一般イベントログアップロード
                var generalLogs = GeneralEventLogger.Instance.GetLogsAndClear();
                if (generalLogs != null && generalLogs.Length > 0)
                {
                    await LudiscanClient.Instance.UploadGeneralEventLogs(currentProjectId, currentSessionId, generalLogs);
                    Debug.Log($"[Ludiscan] Uploaded {generalLogs.Length} general event logs");
                }

                // フィールドオブジェクトログアップロード
                var fieldLogs = FieldObjectLogger.Instance.GetLogsAndClear();
                if (fieldLogs != null && fieldLogs.Length > 0)
                {
                    await LudiscanClient.Instance.UploadFieldObjectLogs(currentProjectId, currentSessionId, fieldLogs);
                    Debug.Log($"[Ludiscan] Uploaded {fieldLogs.Length} field object logs");
                }

                // 位置情報アップロード
                var positionBuffer = PositionLogger.Instance.Buffer.ToList();
                if (positionBuffer.Count > 0)
                {
                    positionBuffer.RemoveAll(item => item.Position == Vector3.zero || item.OffsetTimeStamp == 0);
                    await LudiscanClient.Instance.UploadPosition(
                        currentProjectId,
                        currentSessionId,
                        positionBuffer.ToArray()
                        );
                    Debug.Log($"[Ludiscan] Uploaded {positionBuffer.Count} position entries");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Ludiscan] Upload failed: {ex.Message}");
            }
        }

        // ========== ロギング用パブリックメソッド ==========

        public void LogPlayerSpawn(Vector3 position, int playerId = 0)
        {
            if (!isInitialized || currentSessionId == -1)
                return;

            var metadata = new
            {
                spawn_point = GetSpawnPointName(position),
                player_id = playerId
            };

            GeneralEventLogger.Instance.AddLog(
                eventType: "player_spawn",
                metadata: metadata,
                offsetTimestamp: (ulong)((Time.time - sessionStartTimeStamp) * 1000),
                position: position,
                player: playerId
            );
        }

        public void LogPlayerDeath(Vector3 position, int playerId = 0)
        {
            if (!isInitialized || currentSessionId == -1)
                return;

            GeneralEventLogger.Instance.AddLog(
                eventType: "player_death",
                metadata: new { player_id = playerId },
                offsetTimestamp: (ulong)((Time.time - sessionStartTimeStamp) * 1000),
                position: position,
                player: playerId
            );
        }

        public void LogPlayerGoal(Vector3 position, int playerId = 0)
        {
            if (!isInitialized || currentSessionId == -1)
                return;

            GeneralEventLogger.Instance.AddLog(
                eventType: "player_goal",
                metadata: new { player_id = playerId },
                offsetTimestamp: (ulong)((Time.time - sessionStartTimeStamp) * 1000),
                position: position,
                player: playerId
            );
        }

        public void LogItemCollection(string itemId, string itemType, Vector3 position, int playerId = 0)
        {
            if (!isInitialized || currentSessionId == -1)
                return;

            GeneralEventLogger.Instance.AddLog(
                eventType: "item_collected",
                metadata: new { item_id = itemId, item_type = itemType },
                offsetTimestamp: (ulong)((Time.time - sessionStartTimeStamp) * 1000),
                position: position,
                player: playerId
            );

            // フィールドオブジェクトログにも記録
            FieldObjectLogger.Instance.LogItemDespawn(
                itemId: itemId,
                itemType: itemType,
                position: position,
                offsetTimestamp: (uint)((Time.time - sessionStartTimeStamp) * 1000),
                pickedByPlayer: playerId
            );
        }

        public void LogEnemySpawn(string enemyId, string enemyType, Vector3 position)
        {
            if (!isInitialized || currentSessionId == -1)
                return;

            FieldObjectLogger.Instance.LogEnemySpawn(
                enemyId: enemyId,
                enemyType: enemyType,
                position: position,
                offsetTimestamp: (uint)((Time.time - sessionStartTimeStamp) * 1000)
            );
        }

        public void LogEnemyDeath(string enemyId, string enemyType, Vector3 position)
        {
            if (!isInitialized || currentSessionId == -1)
                return;

            FieldObjectLogger.Instance.LogEnemyDeath(
                enemyId: enemyId,
                enemyType: enemyType,
                position: position,
                offsetTimestamp: (uint)((Time.time - sessionStartTimeStamp) * 1000)
            );

            GeneralEventLogger.Instance.AddLog(
                eventType: "enemy_defeated",
                metadata: new { enemy_id = enemyId, enemy_type = enemyType },
                offsetTimestamp: (ulong)((Time.time - sessionStartTimeStamp) * 1000),
                position: position,
                player: 0
            );
        }

        public void LogGamePhaseChanged(string phaseName)
        {
            if (!isInitialized || currentSessionId == -1)
                return;

            GeneralEventLogger.Instance.AddLog(
                eventType: "game_phase_changed",
                metadata: new { phase_name = phaseName },
                offsetTimestamp: (ulong)((Time.time - sessionStartTimeStamp) * 1000),
                position: Vector3.zero,
                player: 0
            );
        }

        private string GetSpawnPointName(Vector3 position)
        {
            // スポーン地点の名前を取得（必要に応じて実装）
            return "checkpoint_1";
        }
    }
}
