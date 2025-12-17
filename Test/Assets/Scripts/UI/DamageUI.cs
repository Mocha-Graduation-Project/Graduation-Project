using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

namespace Scripts.UI
{
    public class DamageUI : MonoBehaviour
    {
        //[JapaneseLabel("ダメージテキストUI")] private TextMeshProUGUI damageText;
        [Header("ダメージ表記")] 
        [SerializeField][JapaneseLabel("ダメージ1桁")] Image damageImageSingleDigit;
        
        [SerializeField][JapaneseLabel("ダメージ2桁")] private Image[] damageImageTwoDigit;

        [SerializeField] [JapaneseLabel("ダメージ素材")] private Sprite[] damageImageSources;
        
        private Vector3 startScale;

        [JapaneseLabel("初期位置1桁")] private Vector3 damageImageSinglePosition;
        [JapaneseLabel("初期位置2桁")] private Vector3[] damageImageTwoPositions;
        
        [JapaneseLabel("初期位置")]private Vector3 startPosition;
        [JapaneseLabel("初期カラー")]private Color originalColor;
        [JapaneseLabel("ダメージカラー")] private Color damageColor;
        [JapaneseLabel("DoTween")]private Tween currentTween;
        private Tween[] currentTweens;
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
        // [SerializeField][JapaneseLabel("【中】　ダメージがY以下の時フォントサイズを+〇する")] private float mediumDamageSize = 5;
        // [SerializeField][JapaneseLabel("【大】　ダメージがY以上の時フォントサイズを+〇する")] private float bigDamageSize = 20;
        [Header("<色>")]
        [SerializeField][JapaneseLabel("【小】　ダメージがX以下の時色を〇色にする")] private Color smallDamageColor = Color.white;
        [SerializeField][JapaneseLabel("【中】　ダメージがY以下の時色を〇色にする")] private Color mediumDamageColor = Color.yellow;
        [SerializeField][JapaneseLabel("【大】　ダメージがY以上の時色を〇色にする")] private Color bigDamageColor = Color.red;
        
        private void Awake()
        {
            // damageText = GetComponent<TextMeshProUGUI>();
            // startPosition = transform.localPosition;
            // originalColor = damageText.color;
            // startSize = damageText.fontSize;
            Cursor.visible = false;
        }

        void Start()
        {
            damageImageSingleDigit=gameObject.transform.Find("damageSingleDigit").GetComponent<Image>();
            damageImageTwoDigit[0] = gameObject.transform.Find("damageTwoDigit10").GetComponent<Image>();
            damageImageTwoDigit[1] = gameObject.transform.Find("damageTwoDigit1").GetComponent<Image>();

            damageImageSinglePosition = damageImageSingleDigit.transform.localPosition;
            damageImageTwoPositions = new Vector3[damageImageTwoDigit.Length];
            for (int i = 0; i < damageImageTwoDigit.Length; i++)
            {
                damageImageTwoPositions[i] = damageImageTwoDigit[i].transform.localPosition;
            }

            currentTweens = new Tween[damageImageTwoDigit.Length];
            startPosition = damageImageSingleDigit.transform.localPosition;
            originalColor = damageImageSingleDigit.color;
            damageColor = new Color(255, 255, 255, 255);
            startScale=new Vector3(damageImageSingleDigit.transform.localScale.x,damageImageSingleDigit.transform.localScale.y,1);
        }
        public void ShowDamage(int damage)
        {
            currentTween?.Kill();
            for (int i = 0; i < currentTweens.Length; i++)
            {
                currentTweens[i]?.Kill();
            }
            // transform.localPosition = startPosition;
            //
            // damageText.text = damage.ToString();
            // damageText.color = originalColor;
            // damageText.fontSize = startSize;

            Debug.Log("Damage:" + damage);
            
            if (damage < smallDamage)
            {
                // damageText.color = smallDamageColor;
                // damageText.fontSize += smallDamageSize;
            }
            else if (damage < mediumDamage)
            {
                // damageText.color = mediumDamageColor;
                // damageText.fontSize += mediumDamageSize;
            }
            else
            {
                // damageText.color = bigDamageColor;
                // damageText.fontSize += bigDamageSize;
            }
            // アニメーション
            // Sequence seq = DOTween.Sequence();
            // seq.Append(transform.DOLocalMoveY(startPosition.y + moveY, duration).SetEase(Ease.OutCubic));
            // seq.Join(damageText.DOFade(0, duration));
            
            if (damage < 10)
            {
                switch (damage)
                {
                    case 1:
                        damageImageSingleDigit.sprite = damageImageSources[0];
                        break;
                    case 2:
                        damageImageSingleDigit.sprite = damageImageSources[1];
                        break;
                    case 3:
                        damageImageSingleDigit.sprite = damageImageSources[2];
                        break;
                    case 4:
                        damageImageSingleDigit.sprite = damageImageSources[3];
                        break;
                    case 6:
                        damageImageSingleDigit.sprite = damageImageSources[4];
                        break;
                    case 8:
                        damageImageSingleDigit.sprite = damageImageSources[5];
                        break;
                    default:
                        break;
                }
                damageImageSingleDigit.color = damageColor;
                
                Sequence seq = DOTween.Sequence();
                seq.Append(damageImageSingleDigit.transform.DOLocalMoveY(startPosition.y + moveY, duration)
                    .SetEase(Ease.OutCubic));
                seq.Join(damageImageSingleDigit.DOFade(0, duration));
                seq.OnComplete(() =>
                {
                    ResetUI();
                });
                
                currentTween = seq;
            }
            else
            {
                int degit10 = damage / 10;
                int degit1 = damage % 10;
                switch (degit10)
                {
                    case 1:
                        damageImageTwoDigit[0].sprite = damageImageSources[0];
                        break;
                    default:
                        break;
                }

                switch (degit1)
                {
                    case 1:
                        damageImageTwoDigit[1].sprite = damageImageSources[0];
                        break;
                    case 2:
                        damageImageTwoDigit[1].sprite = damageImageSources[1];
                        break;
                    case 3:
                        damageImageTwoDigit[1].sprite = damageImageSources[2];
                        break;
                    case 4:
                        damageImageTwoDigit[1].sprite = damageImageSources[3];
                        break;
                    case 6:
                        damageImageTwoDigit[1].sprite = damageImageSources[4];
                        break;
                    case 8:
                        damageImageTwoDigit[1].sprite = damageImageSources[5];
                        break;
                    default:
                        break;
                }

                Sequence[] seq = new Sequence[2];
                for (int i = 0; i < damageImageTwoDigit.Length; i++)
                {
                    damageImageTwoDigit[i].color = damageColor;
                    seq[i] = DOTween.Sequence();
                    seq[i].Append(damageImageTwoDigit[i].transform
                        .DOLocalMoveY(startPosition.y + moveY, duration).SetEase(Ease.OutCubic));
                    seq[i].Join(damageImageTwoDigit[i].DOFade(0, duration));
                }

                for (int i = 0; i < damageImageTwoDigit.Length; i++)
                {
                    seq[i].OnComplete(() =>
                    {
                        ResetUI();
                    });

                    currentTweens[i] = seq[i];
                }
            }
            
            // seq.OnComplete(() =>
            // {
            //     // 元の位置・状態に戻す
            //     transform.localPosition = startPosition;
            //     damageText.color = originalColor;
            //     damageText.fontSize = startSize;
            //     damageText.text = damage.ToString(" ");
            // });
            //
            // currentTween = seq;
        }
        
        void ResetUI()
        {
            // 元の位置・状態に戻す
            damageImageSingleDigit.color = originalColor;
            damageImageSingleDigit.sprite = null;
            damageImageSingleDigit.transform.localPosition = damageImageSinglePosition;
            damageImageSingleDigit.transform.localScale = startScale;
            
            for (int i = 0; i < damageImageTwoDigit.Length; i++)
            {
                damageImageTwoDigit[i].color = originalColor;
                damageImageTwoDigit[i].sprite = null;
                damageImageTwoDigit[i].transform.localPosition = damageImageTwoPositions[i];
                damageImageTwoDigit[i].transform.localScale = startScale;
            }
        }
    }
}