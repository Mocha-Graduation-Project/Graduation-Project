using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Component;
using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.Timeline;
#endif
public class SceneButtonManager : MonoBehaviour
{
    public enum State
    {
        Gameplay,
        Pause,
        Clear,
    }
    
    public State currentState = State.Gameplay;
    [SerializeField] private GameObject player;
    [SerializeField]private Player.Player playerScript;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject pauseObj;
    [SerializeField] private GameObject clearObj;
    
    public State CurrentState { get { return currentState; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = State.Gameplay;
        
        player=GameObject.Find("Player");
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

    void ChangeState(State nextState)
    {
        currentState = nextState;
    }
    
    public void PauseGame()
    {
        if(currentState == State.Clear){return;}
        
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
        ChangeState(State.Clear);
        Time.timeScale = 0;
        if (clearObj != null) {clearObj.SetActive(true);}
        Debug.Log("Game Clear:" + currentState);
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

    public void InputReset()
    {
        Time.timeScale = 1;
        if (playerInput == null) return;
        Debug.Log("Reset Input:");
        playerScript.PlayerReset();
        playerInput.actions["Retry"].performed -= OnRetry;
        playerInput.actions["Finish"].performed -= OnFinished;
        playerInput.actions["Pause"].performed -= OnPause;
    }
}
