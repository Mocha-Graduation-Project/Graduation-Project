using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class Warp : MonoBehaviour
    {
        [SerializeField] [JapaneseLabel("ワープ先")]
        private Transform targetWarp;

        [SerializeField] [JapaneseLabel("ワープさせるタグ")] [Tag]
        private List<string> Tags;

        private bool isWarping;

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"触れた: {other.tag}");
            // タグがリストに含まれていて、現在ワープ中でないなら処理する
            if (!isWarping && Tags.Contains(other.tag))
            {
                Debug.Log($"タグ一致、ワープ開始: {other.name}");
                StartCoroutine(WarpPoint(other));
            }
        }

        private IEnumerator WarpPoint(Collider2D target)
        {
            isWarping = true;

            var targetWarpPoint = targetWarp.GetComponent<Warp>();
            if (targetWarpPoint != null) targetWarpPoint.isWarping = true;

            // 親（ルート）ごと移動
            var rootTransform = target.transform.root;

            // CharacterController対策
            var controller = rootTransform.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                rootTransform.position = targetWarp.position;
                controller.enabled = true;
            }
            else
            {
                rootTransform.position = targetWarp.position;
            }

            yield return new WaitForSeconds(0.2f);

            isWarping = false;
            if (targetWarpPoint != null) targetWarpPoint.isWarping = false;
        }
    }
}