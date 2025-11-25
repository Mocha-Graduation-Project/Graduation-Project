using UnityEngine;

namespace Component
{
    public class StartupInitializer : MonoBehaviour
    {
        public static bool IsInitialized { get; private set; } = false; 
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize() {
            new GameObject("StartupInitializer", typeof(StartupInitializer));
        }
    
        private void Awake()
        {
            RecordController.Initialize("localhost", "4455", "S0cTYDJosuQjDjAX");
            Debug.Log("Initialized startup initializer");
            Debug.Log("Host:" + RecordController.Host + "/Port:" + RecordController.Port + "/Password:" +
                      RecordController.Password);

            // LudiscanManagerを動的に生成
            InitializeLudiscanManager();

            //初期化が済んだら自分を消す
            Destroy(gameObject);
            IsInitialized = true;
        }

        private void InitializeLudiscanManager()
        {
            // LudiscanManager用のGameObjectを作成
            GameObject ludiscanObj = new GameObject("LudiscanManager");
            ludiscanObj.AddComponent<LudiscanManager>();

            Debug.Log("[Ludiscan] Manager created and initialized");
        }
    }
}
