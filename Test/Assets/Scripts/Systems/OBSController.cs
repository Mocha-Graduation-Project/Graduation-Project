using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Component;
using UI;
using UnityEngine;

public class OBSController : MonoBehaviour
{
    [SerializeField] private SceneButtonManager _sceneButtonManager;
    [SerializeField] private GameObject startUIObj;
    [SerializeField] private GameObject canTakeFhotoObj;
    [SerializeField] private GameObject connectingTextObj;
    [SerializeField] private bool isRecording;
    
    public void AskCanTakeOBS()
    {
        if (isRecording == true)
        {
            if(startUIObj != null){startUIObj.SetActive(false);}
            if(canTakeFhotoObj != null){canTakeFhotoObj.SetActive(true);}
            StartCoroutine(OBSConnection());
        }
        else{_sceneButtonManager.SceneChangeMainMenu();}
    }
    
    IEnumerator OBSConnection()
    {
        Debug.Log("Host:" + RecordController.Host + "/Port:" + RecordController.Port + "/Password:" +
                  RecordController.Password);
        RecordController.OBSConnect(CancellationToken.None);
        Debug.Log("OBS Conecting");
        yield return new WaitForSeconds (1.0f);
        Debug.unityLogger.Log("OBS Connected");
    }

    IEnumerator ChangeOBSScene(string sceneName)
    {
        if(canTakeFhotoObj != null){canTakeFhotoObj.SetActive(false);}
        if(connectingTextObj != null){connectingTextObj.SetActive(true);}
        RecordController.OBSSetCurrentProgromScene(sceneName);
        yield return new WaitForSeconds (0.2f);
        RecordController.OBSRecordStart();
        _sceneButtonManager.SceneChangeMainMenu();
    }

    public void SetSceneName(string sceneName)
    {
        StartCoroutine(ChangeOBSScene(sceneName));
    }
}
