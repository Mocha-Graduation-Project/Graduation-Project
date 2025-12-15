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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
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
            // シーンロード時はフェードイン（黒 -> 透明）
            FadeIn(fadeDuration, null);
        }

        public void LoadScene(string sceneName)
        {
            // シーン遷移時はフェードアウト（透明 -> 黒）してからロード
            FadeOut(fadeDuration, () =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }

        public void FadeOut(float duration, Action onComplete)
        {
            if (fadeImage == null) return;
            
            fadeImage.raycastTarget = true; // 操作をブロック
            
            fadeImage.DOFade(1f, duration)
                .SetUpdate(true) 
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        public void FadeIn(float duration, Action onComplete)
        {
            if (fadeImage == null) return;

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