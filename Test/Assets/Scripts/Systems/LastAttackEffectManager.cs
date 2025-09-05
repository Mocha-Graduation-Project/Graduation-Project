using UnityEngine;
using System.Collections;

namespace Systems
{
    public class LastAttackEffectManager : MonoBehaviour
    {
        public static LastAttackEffectManager Instance { get; private set; }

        [Header("ズームイン設定")] [SerializeField, JapaneseLabel("ズーム時のFOV")]
        private float zoomFOV = 30f;

        [SerializeField, JapaneseLabel("ズームするまでの時間（秒）")]
        private float zoomDuration = 0.5f;

        [SerializeField, JapaneseLabel("ズーム時のカメラの最終位置（オフセット）")]
        private Vector3 zoomCameraOffset = new Vector3(0, 2, -5);

        [Header("ヒットストップ設定")] [SerializeField, JapaneseLabel("ヒットストップの持続時間（秒）")]
        private float hitStopDuration = 0.2f;

        private Camera mainCamera;
        private float initialFOV;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                initialFOV = mainCamera.fieldOfView;
                initialPosition = mainCamera.transform.position;
                initialRotation = mainCamera.transform.rotation;
            }
        }
        public void PlayLastAttackEffect(Transform target,GameObject lastEnemy)
        {
            StartCoroutine(LastAttackCoroutine(target,lastEnemy));
            Time.timeScale = 0.1f;
        }

        private IEnumerator LastAttackCoroutine(Transform target,GameObject lastEnemy)
        {
            StartCoroutine(ZoomAndMoveCamera(target));
            
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            
            Time.timeScale = 1f;
            
            Destroy(lastEnemy);

            StartCoroutine(ResetCameraPosition());
        }

        private IEnumerator ZoomAndMoveCamera(Transform target)
        {
            float timer = 0f;
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;
            
            Vector3 endPos = target.position + zoomCameraOffset;
            Quaternion endRot = Quaternion.LookRotation(target.position - endPos);

            while (timer < zoomDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / zoomDuration;

                // カメラ位置と回転を滑らかに
                mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                
                mainCamera.fieldOfView = Mathf.Lerp(initialFOV, zoomFOV, t);

                yield return null;
            }

            // ズームイン完了後の最終値を設定
            mainCamera.transform.position = endPos;
            mainCamera.transform.rotation = endRot;
            mainCamera.fieldOfView = zoomFOV;
        }

        private IEnumerator ResetCameraPosition()
        {
            float timer = 0f;
            float resetDuration = zoomDuration; // 戻る時間も同じにする
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;
            float startFOV = mainCamera.fieldOfView;

            while (timer < resetDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / resetDuration;

                mainCamera.transform.position = Vector3.Lerp(startPos, initialPosition, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, initialRotation, t);
                mainCamera.fieldOfView = Mathf.Lerp(startFOV, initialFOV, t);

                yield return null;
            }

            // 最終的な値をセット
            mainCamera.transform.position = initialPosition;
            mainCamera.transform.rotation = initialRotation;
            mainCamera.fieldOfView = initialFOV;
        }
    }
}