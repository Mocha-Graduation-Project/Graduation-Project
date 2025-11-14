using System;
using Component;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class SceneButtonManager : MonoBehaviour
    {
        public enum State
        {
            Gameplay,
            Pause,
            Clear,
            GameOver 
        }
    
        public State currentState = State.Gameplay;
        [SerializeField] private GameObject player;
        [SerializeField] private Player.Player playerScript;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private GameObject pauseObj;
        [SerializeField] private GameObject clearObj;
        [SerializeField] private GameObject gameOverObj;
        [SerializeField] private MapManager mapManager; 
    
        public State CurrentState { get { return currentState; } }
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            currentState = State.Gameplay;
         
            player = GameObject.Find("PlayerGeneric");
            if (player != null)
            {
                playerScript = player.GetComponent<Player.Player>();
                playerInput = player.GetComponent<PlayerInput>();
            }

            if (playerInput == null) return;
            playerInput.actions["Retry"].performed += OnRetry;
            playerInput.actions["Finish"].performed += OnFinished;
            playerInput.actions["Pause"].performed += OnPause;
        }
        
        public void ChangeState(State nextState)
        {
            currentState = nextState;
        }
    
        public void PauseGame()
        {
            if (currentState == State.Clear || currentState == State.GameOver) { return; }
        
            if (currentState != State.Pause)
            {
                ChangeState(State.Pause);
                if (pauseObj != null){pauseObj.SetActive(true);}
                Time.timeScale = 0;
                Debug.Log("Pause Game:" + currentState);
            }
            else
            {
                ChangeState(State.Gameplay);
                if (pauseObj != null){pauseObj.SetActive(false);}
                Time.timeScale = 1;
                Debug.Log("Play Game:" + currentState);
            }
        }

        public void GameClear()
        {
            if (currentState == State.Pause)
            {
                pauseObj.SetActive(false);
            }
            else if (currentState != State.Clear) {return;}
            
            //ChangeState(State.Clear);
            Time.timeScale = 0;
            if (clearObj != null) {clearObj.SetActive(true);}
            mapManager.Clear();
            Debug.Log("Game Clear:" + currentState);
        }

        public void GameOver()
        {
            if (currentState == State.Pause)
            {
                pauseObj.SetActive(false);
            }
            else if (currentState != State.Gameplay) {return;}
            
            ChangeState(State.GameOver);
            Time.timeScale = 0;
            if (gameOverObj != null){gameOverObj.SetActive(true);}
            mapManager.GameOver();
            DisableAll();
            Debug.Log("Game Over:" + currentState);
        }
        
        public void SceneChangeTitle()
        {
            InputReset();
            RecordController.OBSRecordStop();
            RecordController.OBSDisconnect();
            SceneManager.LoadScene("Title");
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void SceneChangeMainMenu()
        {
            InputReset();
            SceneManager.LoadScene("MainMenu");
        }
    
        public void SceneChangeGame(string sceneName)
        {
            InputReset();
            SceneManager.LoadScene(sceneName);
        }

        public void Retry()
        {
            InputReset();
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

        public void FinishGame()
        {
            RecordController.OBSRecordStop();
            RecordController.OBSDisconnect();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed == true) return;
            PauseGame();
        }
        public void OnRetry(InputAction.CallbackContext context)
        {
            if (!context.performed == true|| currentState!=State.Gameplay) return;
            Retry();
        }

        public void OnFinished(InputAction.CallbackContext context)
        {
            if (!context.performed == true|| currentState!=State.Gameplay) return;
            FinishGame();
        }
        [Obsolete("Obsolete")]
        private void DisableAll()
        {
            // シーン内の全てのEnemyControllerスクリプトを取得
            var enemies = FindObjectsOfType<Scripts.Enemy>();
        
            foreach (var enemy in enemies)
            {
                enemy.gameObject.SetActive(false);
            }
            var bullets = FindObjectsOfType<Bullet>();
            foreach (var bullet in bullets)
            {
                bullet.gameObject.SetActive(false);
            }
            playerScript.gameObject.SetActive(false);
        }

        public void InputReset()
        {
            Debug.Log("Reset Input:");
            Time.timeScale = 1;
            if (playerInput == null) return;
            playerInput.actions["Retry"].performed -= OnRetry;
            playerInput.actions["Finish"].performed -= OnFinished;
            playerInput.actions["Pause"].performed -= OnPause;
        }
    }
}
