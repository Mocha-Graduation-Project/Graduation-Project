using Scripts.Scriptable;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Component
{
    enum GameplayState
    {
        Title,
        Normal,
        MoveBoss,
        Depth,
        GameOver,
        GameClear
    }
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private MapData mapData;
        [SerializeField] private bool checkSkip;
        [SerializeField] private SoundData soundData;

        [SerializeField] private GameplayState gameplayState = GameplayState.Normal;
        private AudioSource audioSource;
        private AudioClip bgm;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            switch (gameplayState)
            {
                case GameplayState.Title:
                  bgm = soundData.title;
                    break;
                case GameplayState.Normal:
                    bgm = soundData.normal;
                    break;
                case GameplayState.MoveBoss:
                    bgm = soundData.moveBoss;
                    break;
                case GameplayState.Depth:
                    bgm = soundData.depth;
                    break;
                case GameplayState.GameOver:
                    bgm = soundData.gameOver;
                    break;
                case GameplayState.GameClear:
                    bgm = soundData.gameClear;
                    break;
            }

            audioSource.clip = bgm;
            audioSource.Play();

            // Ludiscanセッション開始（非同期処理を分離）
            if (gameplayState is GameplayState.Normal or GameplayState.MoveBoss or GameplayState.Depth)
            {
                StartLudiscanSessionAsync().Forget();
            }
        }

        /// <summary>
        /// Ludiscanセッションを非同期で開始（エラーハンドリング対応）
        /// </summary>
        private async UniTaskVoid StartLudiscanSessionAsync()
        {
            try
            {
                await LudiscanManager.Instance.StartStageSessionAsync(SceneManager.GetActiveScene().name);
                LudiscanManager.Instance.LogGamePhaseChanged(gameplayState.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MapManager] Failed to start Ludiscan session: {ex.Message}");
            }
        }

        public enum Side
        {
            Left = 0,
            Right = 1,
            Up = 2,
            Down = 3,
        }

        public void Clear()
        {
            if (audioSource == null || audioSource.gameObject == null)
                return;

            audioSource.Stop();
            bgm = soundData.gameClear;
            audioSource.clip = bgm; 
            audioSource.Play();

            // ゴールイベントをログ（非同期処理を分離）
            LogPlayerGoalAsync().Forget();
        }

        /// <summary>
        /// プレイヤーゴールをログ記録（エラーハンドリング対応）
        /// </summary>
        private async UniTaskVoid LogPlayerGoalAsync()
        {
            try
            {
                var player = FindObjectOfType<Player.Player>();
                if (player != null)
                {
                    LudiscanManager.Instance.LogPlayerGoal(player.transform.position);
                }

                // セッション終了
                await LudiscanManager.Instance.EndCurrentSessionAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MapManager] Failed to log player goal: {ex.Message}");
            }
        }

        public void GameOver()
        {
            audioSource.Stop();
            bgm = soundData.gameOver;
            audioSource.clip = bgm; 
            audioSource.Play();

            // セッション終了（非同期処理を分離）
            EndLudiscanSessionAsync().Forget();
        }

        /// <summary>
        /// Ludiscanセッションを非同期で終了（エラーハンドリング対応）
        /// </summary>
        private async UniTaskVoid EndLudiscanSessionAsync()
        {
            try
            {
                await LudiscanManager.Instance.EndCurrentSessionAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MapManager] Failed to end Ludiscan session: {ex.Message}");
            }
        }
        public bool CanLoop(Vector3 pos, Side side)
        {
            //Debug.Log(pos);
            if (checkSkip == true)
            {
                return true;
            }
        
            if (side == Side.Up && mapData.UpDownCantArea != null)
            {
                for (int i = 0; i < mapData.UpDownCantArea.Length; i += 2)
                {
                    //オブジェクトの左側よりも大きい且つ右側よりも小さい(オブジェクトの内部)の場合ループできない
                    if (pos.x > mapData.UpDownCantArea[i] && pos.x < mapData.UpDownCantArea[i + 1])
                    {
                        return false;
                    }
                }
            }
            else if (side == Side.Down && mapData.DownUpCantArea != null)
            {
                for (int i = 0; i < mapData.DownUpCantArea.Length; i += 2)
                {
                    //オブジェクトの左側よりも大きい且つ右側よりも小さい(オブジェクトの内部)の場合ループできない
                    if (pos.x > mapData.DownUpCantArea[i] && pos.x < mapData.DownUpCantArea[i + 1])
                    {
                        return false;
                    }
                }
            }
        
            return true;
        }
    }
}
