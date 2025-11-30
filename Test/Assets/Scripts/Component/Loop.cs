using UnityEngine;
using System;

namespace Component
{
    /*
     コライダー内で特定のオブジェクトをループするscript
     */
    public class Loop : MonoBehaviour
    {
        //軽量化の為エディタから設定すること
        [SerializeField]private Collider loopAreaCollider;

        public event Action OnLoop;

        private float minX, maxX, minY, maxY;

        private void Awake()
        {
            var loopAreaObj = GameObject.FindWithTag("LoopArea");
            if (loopAreaObj != null)
                loopAreaCollider = loopAreaObj.GetComponent<Collider>();
            else
                Debug.LogError("LoopAreaColliderが見つかりません。LoopAreaタグを持つGameObjectを配置してください。");
        }

        private void Start()
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