using UnityEngine;

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
		
        //初期化が済んだら自分を消す
        Destroy(gameObject);
        IsInitialized = true;
    }
}
