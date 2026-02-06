using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class FadeManager : MonoBehaviour
    {
        public static FadeManager Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField][JapaneseLabel("時間")] private float fadeDuration;
        
        // シーン遷移中かどうかのフラグ
        private bool isTransitioning = false;
        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null, false); // RectTransformのローカルスケールを保持
            
            DontDestroyOnLoad(gameObject);
            
            if (fadeImage == null)
            {
                Debug.LogError("FadeImageがセットされていません");
            }
            else
            {
                fadeImage.gameObject.SetActive(true);
                fadeImage.enabled = true;
        
                fadeImage.color = Color.black; 
                fadeImage.raycastTarget = true;
                
                // Canvasの設定を確認・修正して、フェードが最前面に表示されるようにする
                Canvas canvas = fadeImage.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 999; // 最前面に表示
                    Debug.Log($"[FadeManager] Canvas sortingOrder set to {canvas.sortingOrder}");
                }
                else
                {
                    Debug.LogWarning("[FadeManager] Canvas not found in parent hierarchy!");
                }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 遷移完了したのでフラグをリセット
            isTransitioning = false;
            Time.timeScale = 1f;
            // シーンロード時はフェードイン（黒 -> 透明）
            FadeIn(fadeDuration, null);
        }

        public void LoadScene(string sceneName)
        {
            // 既に遷移中なら無視
            if (isTransitioning)
            {
                Debug.Log("[FadeManager] LoadScene ignored: already transitioning.");
                return;
            }
            
            isTransitioning = true;
            
            // シーン遷移時はフェードアウト（透明 -> 黒）してからロード
            FadeOut(fadeDuration, () =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }

        public void FadeOut(float duration, Action onComplete)
        {
            Debug.Log($"[FadeManager] FadeOut called. fadeImage: {fadeImage}, duration: {duration}");
            
            if (fadeImage == null)
            {
                Debug.LogError("[FadeManager] fadeImage is null! Invoking onComplete immediately.");
                onComplete?.Invoke();
                return;
            }
            
            // 既存のフェードアニメーションをキル
            fadeImage.DOKill();
            
            fadeImage.raycastTarget = true; // 操作をブロック
            
            // フェードアウト開始時に透明（alpha=0）から開始することを保証
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            
            Debug.Log($"[FadeManager] Starting DOFade. Current alpha: {fadeImage.color.a}, Target: 1, Duration: {duration}");
            
            fadeImage.DOFade(1f, duration)
                .SetUpdate(true) 
                .OnComplete(() =>
                {
                    Debug.Log("[FadeManager] FadeOut complete.");
                    onComplete?.Invoke();
                });
        }

        public void FadeIn(float duration, Action onComplete)
        {
            if (fadeImage == null) return;

            // 既存のフェードアニメーションをキル
            fadeImage.DOKill();

            fadeImage.DOFade(0f, duration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    fadeImage.raycastTarget = false; // 操作ブロック解除
                    onComplete?.Invoke();
                });
        }
    }
}