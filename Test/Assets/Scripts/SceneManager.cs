using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string sceneName ;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //playerInput.actions["Retry"].performed += OnRetry;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Retry()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void OnRetry(InputAction.CallbackContext context)
    {
        Retry();
    }


}
