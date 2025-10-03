using System;
using UnityEngine;

namespace Component
{
    public class ClearDirection : MonoBehaviour
    {
        [SerializeField] SceneButtonManager sceneButtonManager;
        //クリア条件
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Obsolete("Obsolete")]
        private void Start()
        {
            sceneButtonManager = FindObjectOfType<SceneButtonManager>();
            //仮で10秒後にクリア表示
            //Invoke("Clear", 10);
        }

        private void Clear()
        {
            sceneButtonManager.GameClear();
        }
    }
}
