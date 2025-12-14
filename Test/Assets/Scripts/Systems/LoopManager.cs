using UnityEngine;

namespace Systems
{
    // シーンに1つだけ存在する前提の管理クラス
    [DefaultExecutionOrder(-10)]
    public class LoopManager : MonoBehaviour
    {
        // どこからでもアクセスできるインスタンス
        public static LoopManager Instance { get; private set; }

        [SerializeField] private Collider collider;
        public Collider AreaCollider => collider;

        private void Awake()
        {
            // インスタンスを登録
            if (Instance == null)
            {
                Instance = this;
                if (collider == null) collider = GetComponent<Collider>();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}