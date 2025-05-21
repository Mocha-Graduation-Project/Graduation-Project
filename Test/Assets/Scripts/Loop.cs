using System;
using UnityEngine;

namespace Scripts
{
    public class Loop : MonoBehaviour
    {
        private Collider2D loopAreaCollider;

        private float minX, maxX, minY, maxY;

        private void Awake()
        {
            GameObject loopAreaObj = GameObject.FindWithTag("LoopArea");
            if (loopAreaObj != null)
            {
                loopAreaCollider = loopAreaObj.GetComponent<Collider2D>();
            }
            else
            {
                Debug.LogError("LoopAreaColliderが見つかりません。LoopAreaタグを持つGameObjectを配置してください。");
                return;
            }
        }

        void Start()
        {
            Bounds bounds = loopAreaCollider.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
            minY = bounds.min.y;
            maxY = bounds.max.y;
        }

        void Update()
        {
            Vector3 pos = transform.position;

            if (pos.x > maxX) pos.x = minX;
            else if (pos.x < minX) pos.x = maxX;

            if (pos.y > maxY) pos.y = minY;
            else if (pos.y < minY) pos.y = maxY;

            transform.position = pos;
        }
    }
}