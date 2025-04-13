using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneButtonManager : MonoBehaviour
{
    public enum State
    {
        Gameplay,
        Pause,
    }
    
    public State currentState = State.Gameplay;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject pauseObj;
    
    public State CurrentState { get { return currentState; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = State.Gameplay;
        
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["Retry"].performed += OnRetry;
            playerInput.actions["Finish"].performed += OnFinished;
            playerInput.actions["Pause"].performed += OnPause;
        }
    }

    void ChangeState(State nextState)
    {
        currentState = nextState;
    }
    
    public void PauseGame()
    {
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
        //今は仮でメインメニューに飛びます
        //SceneManager.LoadScene("MainMenu");
    }

    public void SceneChangeTitle()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Title");
    }

    public void SceneChangeMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void SceneChangeGame(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    public void Retry()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void FinishGame()
    {
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
}
