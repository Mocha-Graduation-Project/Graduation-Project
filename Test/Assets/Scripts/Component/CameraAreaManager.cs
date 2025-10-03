using UnityEngine;

namespace Component
{
    public class CameraAreaManager : MonoBehaviour
    {
        private Vector3 leftDownPosition;
        private Vector3 rightUpPosition;

        private float leftMax;
        private float rightMax;
        private float upMax;
        private float downMax;

        private const float enemySize = 0.5f;
    
        public float LeftMax{get{return leftMax;}}
        public float RightMax{get{return rightMax;}}
        public float UpMax{get{return upMax;}}
        public float DownMax{get{return downMax;}}
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            leftDownPosition = Camera.main.ViewportToWorldPoint(Vector2.zero);
            rightUpPosition = Camera.main.ViewportToWorldPoint(Vector2.one);
            // Debug.Log("画面の左下の座標は " + leftDownPosition + " です");
            // Debug.Log("画面の右上の座標は " + rightUpPosition + " です");
        
            leftMax = Rounding(leftDownPosition.x + enemySize);
            rightMax = Rounding(rightUpPosition.x - enemySize);
            upMax = Rounding(rightUpPosition.y - enemySize);
            downMax = Rounding(leftDownPosition.y + enemySize);
            // Debug.Log("左:" + leftMax + "右:" + rightMax);
            // Debug.Log("上:" + upMax + "下:" + downMax);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        float Rounding(float value)
        {
            return float.Parse(value.ToString("F1"));
        }
    }
}
