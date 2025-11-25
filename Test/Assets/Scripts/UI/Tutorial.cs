using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace UI
{
    public class Tutorial : MonoBehaviour
    {
        [Header("Tutorial Settings")]
        [JapaneseLabel("再生するチュートリアル動画のクリップ配列")]
        public VideoClip[] tutorialVideos;

        [JapaneseLabel("動画を表示するためのUI RawImageコンポーネント")]
        public RawImage videoDisplayImage;

        [JapaneseLabel("動画再生のためのVideoPlayerコンポーネント")]
        public VideoPlayer videoPlayer;

        [Header("Display Settings")]
        [JapaneseLabel("動画を表示するまでの待機時間（秒）。この時間内にクリアすれば動画は出ません。")]
        public float showVideoDelay = 3.0f;

        [JapaneseLabel("フェードインにかかる時間（秒）")]
        public float fadeInDuration = 1.0f;

        [Header("Current Tutorial State")]
        public TutorialType currentTutorialType = TutorialType.None;
        private int currentVideoIndex = 0;
        
        private Coroutine displayCoroutine;

        public enum TutorialType
        {
            None,
            ScreenLoop,
            Jump,
            Shoot,
            Reflect
        }

        private void Start()
        {
            if (videoPlayer == null || videoDisplayImage == null)
            {
                Debug.LogError("VideoPlayer または VideoDisplayImage が設定されていません。");
                enabled = false;
                return;
            }

            // 最初は非表示にしておく
            videoDisplayImage.gameObject.SetActive(false);
            // 初期アルファ値を0にしておく（念のため）
            Color c = videoDisplayImage.color;
            c.a = 0f;
            videoDisplayImage.color = c;

            StartTutorial();
        }

        public void StartTutorial()
        {
            currentVideoIndex = 0;
            SetCurrentTutorialType(TutorialType.ScreenLoop);
            
            // 動画再生（遅延処理付き）を開始
            StartDisplaySequence();
        }

        /*
         動画表示の準備と遅延タイマーを開始します。
        */
        private void StartDisplaySequence()
        {
            if (tutorialVideos.Length == 0 || currentVideoIndex >= tutorialVideos.Length)
            {
                EndTutorial();
                return;
            }

            // 前のタイマーが動いていたら停止
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            // 動画UIを一旦非表示にする（まだ見せない）
            videoDisplayImage.DOKill(); // 実行中のTweenがあれば停止
            videoDisplayImage.gameObject.SetActive(false);
            videoPlayer.Stop();

            // 現在のインデックスに対応するTutorialTypeを設定
            UpdateTutorialType();

            // 指定秒数待ってから再生するコルーチンを開始
            displayCoroutine = StartCoroutine(WaitAndPlayVideo());
        }

        /*
        / 設定された時間待機してから動画を表示・再生するコルーチン
        */
        private IEnumerator WaitAndPlayVideo()
        {
            // 設定された秒数待機
            yield return new WaitForSeconds(showVideoDelay);

            // 待機時間が終わったら、UIを表示して動画を再生
            videoDisplayImage.gameObject.SetActive(true);
            
            // フェードイン処理
            Color c = videoDisplayImage.color;
            c.a = 0f;
            videoDisplayImage.color = c;
            videoDisplayImage.DOFade(1f, fadeInDuration);

            videoPlayer.clip = tutorialVideos[currentVideoIndex];
            videoPlayer.Prepare();
            
            // 準備完了イベントは一度だけ登録（念のため以前のものを削除してから）
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.prepareCompleted += OnPrepareCompleted;
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            source.Play();
        }

        private void UpdateTutorialType()
        {
            if (currentVideoIndex == 0) SetCurrentTutorialType(TutorialType.ScreenLoop);
            else if (currentVideoIndex == 1) SetCurrentTutorialType(TutorialType.Jump);
            else if (currentVideoIndex == 2) SetCurrentTutorialType(TutorialType.Shoot);
            else if (currentVideoIndex == 3) SetCurrentTutorialType(TutorialType.Reflect);
            else SetCurrentTutorialType(TutorialType.None);

            Debug.Log($"現在のチュートリアル（待機中/再生中）: {currentTutorialType}");
        }

        private void SetCurrentTutorialType(TutorialType type)
        {
            currentTutorialType = type;
        }

        public void NextTutorial()
        {
            // 次へ進むので、現在再生中の動画や待機中のタイマーを停止
            if (displayCoroutine != null) StopCoroutine(displayCoroutine);
            
            videoDisplayImage.DOKill(); // Tween停止
            videoPlayer.Stop();
            videoDisplayImage.gameObject.SetActive(false); // 即座に消す

            currentVideoIndex++; 
            
            if (currentVideoIndex < tutorialVideos.Length)
            {
                // 次のステップのタイマー開始
                StartDisplaySequence();
            }
            else
            {
                EndTutorial();
            }
        }

        // 外部呼び出し用イベント
        public void OnScreenLoopCompleted() { /*...*/ } // 必要ならNextTutorialを呼ぶロジック

        public void EndTutorial()
        {
            if (displayCoroutine != null) StopCoroutine(displayCoroutine);
            videoDisplayImage.DOKill(); // Tween停止
            videoPlayer.Stop();
            videoDisplayImage.gameObject.SetActive(false);
            Debug.Log("チュートリアルが終了しました。");
        }
    }
}