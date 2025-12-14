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

        private void Awake()
        {
            if (LoopManager.Instance != null)
            {
                loopAreaCollider = LoopManager.Instance.AreaCollider;
                
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
                pos.x = minX;
                OnLoop?.Invoke();
            }
            else if (pos.x < minX)
            {
                pos.x = maxX;
                OnLoop?.Invoke();
            }

            if (pos.y > maxY)
            {
                pos.y = minY;
                OnLoop?.Invoke();
            }
            else if (pos.y < minY)
            {
                pos.y = maxY;
                OnLoop?.Invoke();
            }

            transform.position = pos;
        }
    }
}