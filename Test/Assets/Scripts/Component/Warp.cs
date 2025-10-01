using System.Collections;
using System.Collections.Generic;
using Scripts.Scriptable;
using UnityEngine;

namespace Component
{
    public class Warp : MonoBehaviour
    {
        [SerializeField] [JapaneseLabel("ワープ先")]
        private Transform targetWarp;

        [SerializeField] [JapaneseLabel("ワープさせるタグ")] [Tag]
        private List<string> Tags;

        private bool isWarping;
        
        [SerializeField] private SoundData soundData;
        private AudioSource audioSource;
        private AudioClip WarpSound;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            WarpSound = soundData.WarpSound;
        }

        private void OnTriggerEnter(Collider other)
        {
            // タグがリストに含まれていて、現在ワープ中でないなら処理する
            if (isWarping || !Tags.Contains(other.tag)) return;
            audioSource.PlayOneShot(WarpSound);
            StartCoroutine(WarpPoint(other));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator WarpPoint(Collider target)
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