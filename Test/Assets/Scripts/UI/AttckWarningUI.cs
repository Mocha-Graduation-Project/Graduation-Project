using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AttckWarningUI : MonoBehaviour
{
    public enum AttckType
    {
        rightTackle,
        leftTackle,
        fallingAttck,
    }
    //AttckType attckType;
    
    [SerializeField] private GameObject rightTackleUI;
    [SerializeField] private GameObject leftTackleUI;
    [SerializeField] private GameObject fallingAttckUI;

    [SerializeField] private GameObject fallingAttckEnemy;
    [SerializeField] private DepthBoss depthBoss;
    RectTransform fallingUIRectTransform;
    [SerializeField] Camera mainCamera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fallingUIRectTransform = fallingAttckUI.GetComponent<RectTransform>();
        Debug.Log(fallingUIRectTransform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetWarning(AttckType attckType, float displayTime)
    {
        switch (attckType)
        {
            case AttckType.rightTackle:
                rightTackleUI.SetActive(true);
                StartCoroutine(HiddenUI(rightTackleUI, displayTime));
                break;
            case AttckType.leftTackle:
                leftTackleUI.SetActive(true);
                StartCoroutine(HiddenUI(leftTackleUI, displayTime));
                break;
            case AttckType.fallingAttck:
                if (fallingAttckEnemy != null)
                {
                    fallingUIRectTransform.localPosition = new Vector3(0, fallingUIRectTransform.localPosition.y, 0);
                    // Debug.Log("Before:" + fallingUIRectTransform.localPosition + "FallPos:" +
                    //           depthBoss.FallingAttckPos);
                    Vector3 screenPosition =mainCamera.WorldToScreenPoint(depthBoss.FallingAttckPos);
                    //Debug.Log("screenPos:" + screenPosition);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(fallingUIRectTransform, screenPosition, mainCamera,out Vector2 localPosition);
                    Vector3 uipos = fallingUIRectTransform.localPosition;
                    uipos.x = localPosition.x;
                    fallingUIRectTransform.localPosition = new Vector3(uipos.x, uipos.y, 0);
                    //Debug.Log("After:"+fallingUIRectTransform.localPosition);
                }
                fallingAttckUI.SetActive(true);
                StartCoroutine(HiddenUI(fallingAttckUI, displayTime));
                break;
            default:
                break;
        }
    }

    private IEnumerator HiddenUI(GameObject displayUI, float displayTime)
    {
        yield return new WaitForSeconds(displayTime);
        displayUI.SetActive(false);
    }
    

    public void SetFallingAttckEnemy(GameObject enemy)
    {
        fallingAttckEnemy = enemy;
        if (enemy.TryGetComponent<DepthBoss>(out DepthBoss boss))
        {
            depthBoss = boss;
        }
    }
}
