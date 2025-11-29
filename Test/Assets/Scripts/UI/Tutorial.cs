using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

namespace UI
{
    public class Tutorial : MonoBehaviour
    {
        [Header("Tutorial Settings")]
        [JapaneseLabel("再生するチュートリアル動画のクリップ配列")]
        [SerializeField]
        private VideoClip[] tutorialVideos;

        [JapaneseLabel("各チュートリアルに対応するボタン画像（RawImage用）")]
        [SerializeField]
        private Texture[] buttonImages;
        
        [JapaneseLabel("ボタン画像を表示するためのUI RawImageコンポーネント")]
        [SerializeField] 
        private RawImage buttonDisplayImage;
        
        [JapaneseLabel("動画を表示するためのUI RawImageコンポーネント")]
        [SerializeField]
        private RawImage videoDisplayImage;
        
        [JapaneseLabel("テキスト")]
        [SerializeField]
        private TextMeshProUGUI instructionText;

        [JapaneseLabel("動画再生のためのVideoPlayerコンポーネント")]
        [SerializeField]
        private VideoPlayer videoPlayer;

        [Header("Display Settings")]
        [JapaneseLabel("動画を表示するまでの待機時間（秒）。この時間内にクリアすれば動画は出ません。")]
        [SerializeField]
        private float showVideoDelay = 3.0f;

        [JapaneseLabel("フェードインにかかる時間（秒）")]
        [SerializeField]
        private float fadeInDuration = 1.0f;

        [Header("チュートリアル開始地点")]
        [SerializeField]
        private TutorialType currentTutorialType;
        private int currentVideoIndex;
        
        private Coroutine displayCoroutine;

        private enum TutorialType
        {
            Loop,
            Jump,
            Shoot,
            Reflect,
            None
        }

        private void Start()
        {
            if (videoPlayer == null || videoDisplayImage == null)
            {
                Debug.LogError("VideoPlayer または VideoDisplayImage が設定されていません。");
                enabled = false;
                return;
            }

            // 非表示
            videoDisplayImage.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(false);
            buttonDisplayImage.gameObject.SetActive(false);
            // 初期アルファ値を0
            Color c = videoDisplayImage.color;
            c.a = 0f;
            videoDisplayImage.color = c;
            Color btnC = buttonDisplayImage.color;
            btnC.a = 0f;
            buttonDisplayImage.color = btnC;

            StartTutorial();
        }

        private void StartTutorial()
        {
            SetCurrentTutorialType(currentTutorialType);
            
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
            videoDisplayImage.DOKill(); // 実行中のTween停止
            videoDisplayImage.DOFade(0f, fadeInDuration);
            instructionText.DOKill(); // 実行中のTween停止
            instructionText.DOFade(0f, fadeInDuration);
            buttonDisplayImage.DOKill(); // ボタン画像のTween停止
            buttonDisplayImage.DOFade(0f, fadeInDuration); // ボタン画像のフェードアウト
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
            instructionText.gameObject.SetActive(true);
            buttonDisplayImage.gameObject.SetActive(true);
            
            // フェードイン処理
            Color videoDisplayColor = videoDisplayImage.color;
            Color textColor = instructionText.color;
            Color buttonColor = buttonDisplayImage.color;
            
            videoDisplayColor.a = 0f;
            textColor.a = 0f;
            buttonColor.a = 0f;
            
            videoDisplayImage.color = videoDisplayColor;
            buttonDisplayImage.color = buttonColor;
            videoDisplayImage.DOFade(1f, fadeInDuration);
            instructionText.DOFade(1f, fadeInDuration);
            buttonDisplayImage.DOFade(1f, fadeInDuration);
            
            

            videoPlayer.clip = tutorialVideos[currentVideoIndex];
            videoPlayer.Prepare();
            
            // 準備完了イベントは一度だけ登録
            videoPlayer.prepareCompleted -= OnPrepareCompleted;
            videoPlayer.prepareCompleted += OnPrepareCompleted;
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            source.Play();
        }

        private void UpdateTutorialType()
        {
            if (currentVideoIndex == 0)
                SetCurrentTutorialType(TutorialType.Loop);
            else if (currentVideoIndex == 1)
                SetCurrentTutorialType(TutorialType.Jump);
            else if (currentVideoIndex == 2)
                SetCurrentTutorialType(TutorialType.Shoot);
            else if (currentVideoIndex == 3)
                SetCurrentTutorialType(TutorialType.Reflect);
            else
                SetCurrentTutorialType(TutorialType.None);
            
            SetButtonImageFromIndex(currentVideoIndex);

            Debug.Log($"現在のチュートリアル（待機中/再生中）: {currentTutorialType}");
        }

        private void SetCurrentTutorialType(TutorialType type)
        {
            switch (type)
            {
                case TutorialType.None:
                    break;
                case TutorialType.Loop:
                    currentVideoIndex = 0;
                    break;
                case TutorialType.Jump:
                    currentVideoIndex = 1;
                    break;
                case TutorialType.Shoot:
                    currentVideoIndex = 2;
                    break;
                case TutorialType.Reflect:
                    currentVideoIndex = 3;
                    break;
                default:
                    break;
            }
            currentTutorialType = type;
        }
        
        private void SetButtonImageFromIndex(int index)
        {
            if (buttonImages != null && index >= 0 && index < buttonImages.Length)
            {
                buttonDisplayImage.texture = buttonImages[index];
            }
            else
            {
                // 画像が設定されていない、またはインデックスが範囲外の場合はクリア（またはデフォルト画像を設定）
                buttonDisplayImage.texture = null;
                Debug.LogWarning($"インデックス {index} に対応するボタン画像がありません。");
            }
        }

        public void NextTutorial()
        {
            // 次へ進むので、現在再生中の動画や待機中のタイマーを停止
            if (displayCoroutine != null) StopCoroutine(displayCoroutine);
            
            videoDisplayImage.DOKill(); // Tween停止
            instructionText.DOKill();
            buttonDisplayImage.DOKill();
            videoPlayer.Stop();
            Sequence fadeOutSequence = DOTween.Sequence();
            
            fadeOutSequence.Join(videoDisplayImage.DOFade(0f, fadeInDuration));
            fadeOutSequence.Join(instructionText.DOFade(0f, fadeInDuration));
            fadeOutSequence.Join(buttonDisplayImage.DOFade(0f, fadeInDuration));

            fadeOutSequence.OnComplete(() =>
            {
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
            });

        }

        private void EndTutorial()
        {
            if (displayCoroutine != null) StopCoroutine(displayCoroutine);
            videoDisplayImage.DOKill(); // Tween停止
            instructionText.DOKill();
            buttonDisplayImage.DOKill();
            videoPlayer.Stop();
            videoDisplayImage.DOFade(0f, fadeInDuration);
            instructionText.DOFade(0f, fadeInDuration);
            buttonDisplayImage.DOFade(0f, fadeInDuration);
            Debug.Log("チュートリアルが終了しました。");
        }
    }
}