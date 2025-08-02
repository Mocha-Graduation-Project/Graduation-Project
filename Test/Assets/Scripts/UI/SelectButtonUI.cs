using UnityEngine;
using UnityEngine.UI;

public class SelectButtonUI : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    [SerializeField] private GameObject cursol;
    [SerializeField] int currentButtonIndex = 0;
    bool isCoolTime = false;
    private float startTime;
    private float coolTime;
    private float inputValueV;
    private float inputValueVBefore;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.realtimeSinceStartup;
        coolTime = 0.2f;
        inputValueV = 0;
        inputValueVBefore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float inputValueV = Input.GetAxis("Vertical"); 
        Debug.Log(inputValueV+"//"+inputValueVBefore);
        Debug.Log("cooltime:"+isCoolTime);

        if (isCoolTime == true)
        {
            float diff = Time.realtimeSinceStartup - startTime;
            Debug.Log(diff + "=" + Time.time + "-" + startTime);
            if (diff > coolTime)
            {
                isCoolTime = false;
            }
        }
        else
        {
            if (inputValueV == 1 && inputValueV != inputValueVBefore)
            {
                NextButton(-1);
            }
            else if (inputValueV == -1 && inputValueV != inputValueVBefore)
            {
                NextButton(1);
            }
        }
        EnterButton();
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

    void EnterButton()
    {
        if (Input.GetKeyDown("joystick button 0"))
        {
            ButtonInvoke();
        }
    }

    void ButtonInvoke()
    {
        buttons[currentButtonIndex].onClick.Invoke();
    }
}
