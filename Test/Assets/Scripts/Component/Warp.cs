using System.Collections;
using System.Collections.Generic;
using Scripts.Scriptable;
using UnityEngine;

/*
 特定のオブジェクトを別のワープ場所に移動させるscript
 */
namespace Component
{
    public class Warp : MonoBehaviour
    {
        [Header("ワープ先設定")]
        [SerializeField] [JapaneseLabel("ワープ先")]
        private Transform targetWarp;
        
        [Header("ターゲット設定")]
        [SerializeField] [JapaneseLabel("ワープさせるタグ")] [Tag]
        private List<string> tags;
        
        [Header("サウンド設定")]
        [SerializeField] private SoundData soundData;
        private AudioSource audioSource;

        private Warp targetWarpComponent;
        private bool isWarping;
        private AudioClip warpSound;

        private void Awake()
        {
            // AudioSourceがアタッチされていることを前提
            audioSource = GetComponent<AudioSource>();
            
            // Nullチェック
            if (soundData != null)
            {
                warpSound = soundData.warpSound;
            }
            else
            {
                Debug.LogWarning($"SoundDataが設定されていません。ワープ音は再生されません。", this);
            }
            
            // ワープ先のWarpコンポーネントを事前に取得
            if (targetWarp != null)
            {
                targetWarpComponent = targetWarp.GetComponent<Warp>();
                if (targetWarpComponent == null)
                {
                    Debug.LogError($"ワープ先 ({targetWarp.name}) に Warp コンポーネントが見つかりません。", this);
                }
            }
            else
            {
                Debug.LogError($"ワープ先 (Target Warp) が設定されていません。", this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // タグがリストに含まれていて、現在ワープ中でないなら処理する
            if (isWarping || !tags.Contains(other.tag)) return;
            
            if (warpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(warpSound);
            }
            
            //StartCoroutine(WarpPoint(other));
            StartCoroutine(WarpObject(other.transform.root));
        }
        // 共通のワープ処理ロジック
        private IEnumerator WarpObject(Transform rootTransform)
        {
            isWarping = true;
            if (targetWarpComponent != null) targetWarpComponent.isWarping = true;

            // CharacterController対策
            var controller = rootTransform.GetComponent<CharacterController>();
            
            // ワープ実行
            if (controller != null)
            {
                controller.enabled = false;
                rootTransform.position = targetWarp.position;
                controller.enabled = true;
            }
            else
            {
                // Rigidbodyの速度・角速度をリセットすると、ワープ後に挙動が安定することがあります
                var rb = rootTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                rootTransform.position = targetWarp.position;
            }

            // ワープ無効化期間
            yield return new WaitForSeconds(0.2f);

            // フラグをリセット
            isWarping = false;
            if (targetWarpComponent != null) targetWarpComponent.isWarping = false;
        }
    }
}