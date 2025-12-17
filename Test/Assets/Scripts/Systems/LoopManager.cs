using UnityEngine;
using UnityEngine.Serialization;

namespace Systems
{
    // シーンに1つだけ存在する前提の管理クラス
    [DefaultExecutionOrder(-10)]
    public class LoopManager : MonoBehaviour
    {
        // どこからでもアクセスできるインスタンス
        public static LoopManager Instance { get; private set; }

        [SerializeField] private Collider loopCollider;
        public Collider AreaLoopCollider => loopCollider;

        private void Awake()
        {
            // インスタンスを登録
            if (Instance == null)
            {
                Instance = this;
                if (loopCollider == null) loopCollider = GetComponent<Collider>();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}