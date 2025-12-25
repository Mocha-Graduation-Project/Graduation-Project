using UnityEngine;
using System;
using Systems;

namespace Component
{
    /*
     コライダー内で特定のオブジェクトをループするscript
     */
    public class Loop : MonoBehaviour
    {
        //軽量化の為エディタから設定すること
        private Collider loopAreaCollider;

        public event Action OnLoop;

        private float minX, maxX, minY, maxY;

        [SerializeField] private float loopOffset = 1.0f;

        private void Awake()
        {
            if (LoopManager.Instance != null)
            {
                loopAreaCollider = LoopManager.Instance.AreaLoopCollider;
                
                // バウンズ計算
                CalculateBounds();
            }
            else
            {
                Debug.LogError("LoopManagerが見つかりません。シーンに配置してください。");
            }
        }

        private void CalculateBounds()
        {
            var bounds = loopAreaCollider.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
            minY = bounds.min.y;
            maxY = bounds.max.y;
        }

        private void FixedUpdate()
        {
            var pos = transform.position;

            if (pos.x > maxX)
            {
                pos.x = minX + loopOffset;
                OnLoop?.Invoke();
            }
            else if (pos.x < minX)
            {
                pos.x = maxX - loopOffset;
                OnLoop?.Invoke();
            }

            if (pos.y > maxY)
            {
                pos.y = minY + loopOffset;
                OnLoop?.Invoke();
            }
            else if (pos.y < minY)
            {
                pos.y = maxY - loopOffset;
                OnLoop?.Invoke();
            }

            transform.position = pos;
        }
    }
}