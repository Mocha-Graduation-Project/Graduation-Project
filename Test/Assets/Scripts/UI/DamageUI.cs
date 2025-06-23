using UnityEngine;
using DG.Tweening;
using TMPro;
namespace Scripts.UI
{
    public class DamageUI : MonoBehaviour
    {
        [JapaneseLabel("ダメージテキストUI")] private TextMeshProUGUI damageText;

        [JapaneseLabel("初期位置")]private Vector3 startPosition;
        [JapaneseLabel("初期カラー")]private Color originalColor;
        [JapaneseLabel("DoTween")]private Tween currentTween;
        [JapaneseLabel("初期サイズ")]private float startSize;
        [Header("<アニメーション>")]
        [SerializeField] [JapaneseLabel("上昇量")] private float moveY = 1.0f;
        [SerializeField] [JapaneseLabel("上がりきるまでの時間")] private float duration = 1.0f;
        [Space(10)]
        [Header("<条件>")]
        [SerializeField][JapaneseLabel("X")] private float smallDamage = 10;
        [SerializeField][JapaneseLabel("Y")] private float mediumDamage = 30;

        [Header("<サイズ>")]
        [SerializeField][JapaneseLabel("【小】　ダメージがX以下の時フォントサイズを+〇する")] private float smallDamageSize = 0;
        [SerializeField][JapaneseLabel("【中】　ダメージがY以下の時フォントサイズを+〇する")] private float mediumDamageSize = 5;
        [SerializeField][JapaneseLabel("【大】　ダメージがY以上の時フォントサイズを+〇する")] private float bigDamageSize = 20;
        [Header("<色>")]
        [SerializeField][JapaneseLabel("【小】　ダメージがX以下の時色を〇色にする")] private Color smallDamageColor = Color.white;
        [SerializeField][JapaneseLabel("【中】　ダメージがY以下の時色を〇色にする")] private Color mediumDamageColor = Color.yellow;
        [SerializeField][JapaneseLabel("【大】　ダメージがY以上の時色を〇色にする")] private Color bigDamageColor = Color.red;

        private void Awake()
        {
            damageText = GetComponent<TextMeshProUGUI>();
            startPosition = transform.localPosition;
            originalColor = damageText.color;
            startSize = damageText.fontSize;
        }

        public void ShowDamage(int damage)
        {
            currentTween?.Kill();
            transform.localPosition = startPosition;

            damageText.text = damage.ToString();
            damageText.color = originalColor;
            damageText.fontSize = startSize;
            
            if (damage < smallDamage)
            {
                damageText.color = smallDamageColor;
                damageText.fontSize += smallDamageSize;
            }
            else if (damage < mediumDamage)
            {
                damageText.color = mediumDamageColor;
                damageText.fontSize += mediumDamageSize;
            }
            else
            {
                damageText.color = bigDamageColor;
                damageText.fontSize += bigDamageSize;
            }
            // アニメーション
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMoveY(startPosition.y + moveY, duration).SetEase(Ease.OutCubic));
            seq.Join(damageText.DOFade(0, duration));
            seq.OnComplete(() =>
            {
                // 元の位置・状態に戻す
                transform.localPosition = startPosition;
                damageText.color = originalColor;
                damageText.fontSize = startSize;
                damageText.text = damage.ToString(" ");
            });

            currentTween = seq;
        }
    }
}