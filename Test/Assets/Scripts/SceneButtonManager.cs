using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneButtonManager : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["Retry"].performed += OnRetry;
            playerInput.actions["Finish"].performed += OnFinished;
            playerInput.actions["Pause"].performed += OnPause;
        }
    }

    public void PauseGame()
    {
        //今は仮でメインメニューに飛びます
        SceneManager.LoadScene("MainMenu");
    }

    public void SceneChangeTitle()
    {
        SceneManager.LoadScene("Title");
    }
    
    public void SceneChangeGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Retry()
    {
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
        PauseGame();
    }
    public void OnRetry(InputAction.CallbackContext context)
    {
        Retry();
    }

    public void OnFinished(InputAction.CallbackContext context)
    {
        FinishGame();
    }
}
