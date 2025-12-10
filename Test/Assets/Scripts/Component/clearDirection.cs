using System;
using UI;
using UnityEngine;

namespace Component
{
    public class ClearDirection : MonoBehaviour
    {
        private SceneButtonManager sceneButtonManager;
        //クリア条件
        private global::Player.Player player => global::Player.Player.Instance;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            sceneButtonManager = player.sceneButtonManager;
            //仮で10秒後にクリア表示
            //Invoke("Clear", 10);
        }

        private void Clear()
        {
            sceneButtonManager.GameClear();
        }
    }
}
