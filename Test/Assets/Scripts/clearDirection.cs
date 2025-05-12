using UnityEngine;

public class clearDirection : MonoBehaviour
{
    [SerializeField] SceneButtonManager sceneButtonManager;
    //クリア条件
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sceneButtonManager = GameObject.FindObjectOfType<SceneButtonManager>();
        //仮で10秒後にクリア表示
        //Invoke("Clear", 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Clear()
    {
        sceneButtonManager.GameClear();
    }
}
