using System;
using Component;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UI
{
    public class SceneButtonManager : MonoBehaviour
    {
        private static readonly int Title = Animator.StringToHash("Title");

        public enum State
        {
            Title,
            Gameplay,
            Pause,
            Clear,
            GameOver 
        }
    
        public static SceneButtonManager Instance { get; private set; }
        public State currentState = State.Gameplay;
        [SerializeField] private GameObject player;
        [SerializeField] private Player.Player playerScript;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private GameObject pauseObj;
        [SerializeField] private GameObject clearObj;
        [SerializeField] private GameObject gameOverObj;
        [SerializeField] private MapManager mapManager; 
        
        [Header("Clear Animation")]
        [SerializeField] private List<RectTransform> clearAnimationUIList;
        [SerializeField] private float loopDuration = 1f; 

        [Header("Game Over Animation")]
        [SerializeField] private List<CanvasGroup> gameOverAnimationUIList;
        [SerializeField] private float gameOverDuration = 1.5f;
        
        [Header("Title Settings")]
        [SerializeField] private Volume globalVolume;
        [SerializeField] private CanvasGroup titleCanvas;
        [SerializeField][JapaneseLabel("フェード時間")] private float titleFadeDuration = 0.5f;
        [SerializeField][JapaneseLabel("タイトル画面のぼかし強度")] private float titleGlobalFocalLength = 300f;
        [SerializeField][JapaneseLabel("通常ぼかし")] private float globalFocalLength = 50f;
    
        public State CurrentState { get { return currentState; } }
        public bool IsTitle => currentState == State.Title;
        public MapManager MapManager => mapManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            
            // MapManagerの状態がTitleの場合はTitle状態で開始（Awakeで設定して他のStart()より先に確定させる）
            if (mapManager != null)
            {
                var mapField = typeof(Component.MapManager).GetField("gameplayState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (mapField != null)
                {
                    var mapState = mapField.GetValue(mapManager);
                    if (mapState != null && mapState.ToString() == "Title")
                    {
                        currentState = State.Title;
                        if (globalVolume != null && globalVolume.profile.TryGet(out DepthOfField dof))
                        {
                            dof.focalLength.value = titleGlobalFocalLength;
                        }
                    }
                    else
                    {
                        currentState = State.Gameplay;
                    }
                }
                else
                {
                    currentState = State.Gameplay;
                }
            }
            else
            {
                currentState = State.Gameplay;
            }
        }
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (playerScript == null && Player.Player.Instance != null)
            {
                playerScript = Player.Player.Instance;
                player = playerScript.gameObject;
                playerInput = player.GetComponent<PlayerInput>();
            }

            if (playerInput == null) return;
            playerInput.actions["Retry"].performed += OnRetry;
            playerInput.actions["Finish"].performed += OnFinished;
            playerInput.actions["Pause"].performed += OnPause;
        }
        
        public void ChangeState(State nextState)
        {
            currentState = nextState;
        }
        
        /// <summary>
        /// タイトル状態からゲームを開始する（ボタンのOnClickから呼び出す）
        /// </summary>
        public void StartGame()
        {
            if (currentState != State.Title) return;
            
            Debug.Log("Start Game:");
            
            // 状態をGameplayに変更
            currentState = State.Gameplay;
            playerScript.Animator.SetBool(Title,false);
            // タイトルCanvasをフェードアウト
            if (titleCanvas != null)
            {
                titleCanvas.DOFade(0f, titleFadeDuration).OnComplete(() =>
                {
                    titleCanvas.gameObject.SetActive(false);
                });
            }
            
            // GlobalVolumeのDepthOfFieldを設定
            if (globalVolume != null && globalVolume.profile.TryGet(out DepthOfField dof))
            {
                DOTween.To(() => dof.focalLength.value, x => dof.focalLength.value = x, globalFocalLength, titleFadeDuration);
            }
            
            // EnemySpawnManagerにスポーン開始を通知
            if (Systems.EnemySpawnManager.Instance != null)
            {
                Systems.EnemySpawnManager.Instance.StartSpawning();
            }
            
            Debug.Log("Game Started!");
        }
    
        public void PauseGame()
        {
            if (currentState == State.Clear || currentState == State.GameOver) { return; }
        
            if (currentState != State.Pause)
            {
                ChangeState(State.Pause);
                if (pauseObj != null){pauseObj.SetActive(true);}
                Time.timeScale = 0;
                Debug.Log("Pause Game:" + currentState);
            }
            else
            {
                ChangeState(State.Gameplay);
                if (pauseObj != null){pauseObj.SetActive(false);}
                Time.timeScale = 1;
                Debug.Log("Play Game:" + currentState);
            }
        }

        public void GameClear()
        {
            if (currentState == State.Pause)
            {
                pauseObj.SetActive(false);
            }
            else if (currentState != State.Clear) {return;}
            
            //ChangeState(State.Clear);
            Time.timeScale = 0;
            if (clearObj != null) 
            {
                clearObj.SetActive(true);
                PlayClearAnimation();
            }
            mapManager.Clear();
            Debug.Log("Game Clear:" + currentState);
        }

        [Obsolete("Obsolete")]
        public void GameOver()
        {
            if (currentState == State.Pause)
            {
                pauseObj.SetActive(false);
            }
            else if (currentState != State.Gameplay) {return;}
            
            ChangeState(State.GameOver);
            
            // スローモーション演出 (Slow Motion Effect)
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, gameOverDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    Time.timeScale = 0;
                    if (gameOverObj != null)
                    {
                        gameOverObj.SetActive(true);
                        PlayGameOverAnimation();
                    }
                    mapManager.GameOver();
                    DisableAll();
                });

            Debug.Log("Game Over Sequence Started:" + currentState);
        }
        
        public void SceneChangeTitle()
        {
            InputReset();
            RecordController.OBSRecordStop();
            RecordController.OBSDisconnect();
            FadeManager.Instance.LoadScene("Title");
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void SceneChangeMainMenu()
        {
            InputReset();
            FadeManager.Instance.LoadScene("MainMenu");
        }
    
        public void SceneChangeGame(string sceneName)
        {
            InputReset();
            FadeManager.Instance.LoadScene(sceneName);
        }

        public void Retry()
        {
            InputReset();
            FadeManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void FinishGame()
        {
            RecordController.OBSRecordStop();
            RecordController.OBSDisconnect();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed == true) return;
            PauseGame();
        }
        public void OnRetry(InputAction.CallbackContext context)
        {
            if (!context.performed == true|| currentState!=State.Gameplay) return;
            Retry();
        }

        public void OnFinished(InputAction.CallbackContext context)
        {
            if (!context.performed == true|| currentState!=State.Gameplay) return;
            FinishGame();
        }
        [Obsolete("Obsolete")]
        private void DisableAll()
        {
            // シーン内の全てのEnemyControllerスクリプトを取得
            var enemies = FindObjectsOfType<Scripts.Enemy>();
        
            foreach (var enemy in enemies)
            {
                enemy.gameObject.SetActive(false);
            }
            var bullets = FindObjectsOfType<Bullet>();
            foreach (var bullet in bullets)
            {
                bullet.gameObject.SetActive(false);
            }
            playerScript.gameObject.SetActive(false);
        }

        public void InputReset()
        {
            Debug.Log("Reset Input:");
            Time.timeScale = 1;
            if (playerInput == null) return;
            playerInput.actions["Retry"].performed -= OnRetry;
            playerInput.actions["Finish"].performed -= OnFinished;
            playerInput.actions["Pause"].performed -= OnPause;
        }

        private void PlayClearAnimation()
        {
            if (clearAnimationUIList == null) return;

            foreach (var rect in clearAnimationUIList)
            {
                if (rect == null) continue;

                float originalX = rect.anchoredPosition.x;
                
                // 親のCanvasを探して幅を取得
                Canvas canvas = rect.GetComponentInParent<Canvas>();
                float moveDist = 2000f; // デフォルト値
                if (canvas != null)
                {
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                    // Canvasの幅をワールド座標に変換し、それをrectの親のローカル座標に変換して正しい移動距離を出す
                    Vector3 worldWidth = canvas.transform.TransformVector(new Vector3(canvasRect.rect.width, 0, 0));
                    if (rect.parent != null)
                    {
                        Vector3 localWidth = rect.parent.InverseTransformVector(worldWidth);
                        moveDist = Mathf.Abs(localWidth.x);
                    }
                    else
                    {
                        moveDist = canvasRect.rect.width; // 親がない場合はそのまま
                    }
                }

                rect.DOKill();

                Sequence seq = DOTween.Sequence();
                seq.SetUpdate(true); // Time.timeScale = 0 でも動くようにする

                // 1. 左へ移動 (Move Left)
                seq.Append(rect.DOAnchorPosX(originalX - moveDist, loopDuration * 0.5f).SetEase(Ease.Linear));

                // 2. 右端へワープ (Teleport to Right)
                seq.AppendCallback(() => 
                {
                    Vector2 pos = rect.anchoredPosition;
                    pos.x = originalX + moveDist;
                    rect.anchoredPosition = pos;
                });

                // 3. 元の位置へ戻る (Move to Original)
                seq.Append(rect.DOAnchorPosX(originalX, loopDuration * 0.5f).SetEase(Ease.Linear));
                
                // 補正：アニメーション終了時に確実に元の位置に戻す
                seq.OnComplete(() => 
                {
                    Vector2 pos = rect.anchoredPosition;
                    pos.x = originalX;
                    rect.anchoredPosition = pos;
                });
            }
        }

        private void PlayGameOverAnimation()
        {
            if (gameOverAnimationUIList == null) return;

            foreach (var group in gameOverAnimationUIList)
            {
                if (group == null) continue;
                
                group.alpha = 0f;
                group.DOFade(1f, 1f).SetUpdate(true);
                
                // 軽くスケールアニメーションも入れるとリッチになる
                group.transform.localScale = Vector3.one * 1.2f;
                group.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    }
}
