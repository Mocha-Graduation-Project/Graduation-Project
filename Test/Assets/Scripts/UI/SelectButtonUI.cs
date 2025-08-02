using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectButtonUI : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    [SerializeField] private GameObject cursol;
    [SerializeField] int currentButtonIndex = 0;
    bool isCoolTime = false;
    private float startTime;
    private float coolTime;
    private float inputValueVBefore;

    private PlayerInput UIAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIAction = GetComponent<PlayerInput>();
        startTime = Time.realtimeSinceStartup;
        coolTime = 0.2f;
        inputValueVBefore = 0;
        UIAction.actions["On"].started += EnterButton;
        UIAction.actions["Set"].performed += MoveSet;
    }

    // Update is called once per frame
   
    public void MoveSet(InputAction.CallbackContext context)
    {
        float inputValueV = context.ReadValue<Vector2>().y;
       //Debug.Log(inputValueV);
       // Debug.Log(inputValueV + "//" + inputValueVBefore);
       // Debug.Log("cooltime:" + isCoolTime);

        if (isCoolTime == true)
        {
            float diff = Time.realtimeSinceStartup - startTime;
           // Debug.Log(diff + "=" + Time.time + "-" + startTime);
            if (diff > coolTime)
            {
                isCoolTime = false;
            }
        }
        else
        {
            if (inputValueV >= 0.8f && inputValueV != inputValueVBefore)
            {
                NextButton(-1);
            }
            else if (inputValueV <= -0.8f && inputValueV != inputValueVBefore)
            {
                NextButton(1);
            }
          
        }

        inputValueVBefore = inputValueV;
    }
    void NextButton(int next)
    {
        if (next == 1 && currentButtonIndex + 1 < buttons.Length)
        {
            currentButtonIndex++;
            cursol.transform.localPosition=buttons[currentButtonIndex].transform.localPosition;
            isCoolTime = true;
            startTime = Time.realtimeSinceStartup;
        }
        else if (next == -1 && currentButtonIndex - 1 >= 0)
        {
            currentButtonIndex--;
            cursol.transform.localPosition=buttons[currentButtonIndex].transform.localPosition;
            isCoolTime = true;
            startTime = Time.realtimeSinceStartup;
        }
    }

    public void EnterButton(InputAction.CallbackContext context)
    {
        ButtonInvoke();
    }

    void ButtonInvoke()
    {
        buttons[currentButtonIndex].onClick.Invoke();
    }
}
