using UnityEngine;
using TMPro;
using DG.Tweening;
namespace Scripts.UI
{
    public class BeforeAttack : MonoBehaviour
    {
        private TextMeshProUGUI text;

        private Tween blinkTween;
        
        private void Start()
        {
            text = GetComponent<TextMeshProUGUI>();
        }
        public void Warning(float blinkDuration)
        {
            text.text = "!";
            text.alpha = 1f;

            // 既に再生中なら止める
            blinkTween?.Kill();

            // 点滅アニメーション
            blinkTween = text.DOFade(0f, blinkDuration)
                .From(1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear);
        }

        public void After()
        {
            blinkTween?.Kill();
            blinkTween = null;
            text.text = " ";
            text.alpha = 1f;
        }
    }
}