using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectButtonUI : MonoBehaviour
{
    public enum ButtonType
    {
        both,
        vertical,
        horizontal,
    }

    [SerializeField] [JapaneseLabel("ボタンの配置")] private ButtonType buttonType;
    
    [SerializeField] Button[] buttons;
    [SerializeField] private GameObject cursol;
    [SerializeField] int currentButtonIndex = 0;

    [SerializeField] [JapaneseLabel("縦の個数")] private int verticalCount;
    [SerializeField] private bool isCoolTime = false;
    private float startTime;
    private float coolTime;
    private float inputValueV;
    private float inputValueH;
    private float inputValueVBefore;
    private float inputValueHBefore;
    float inputValue;

    private PlayerInput UIAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIAction = GetComponent<PlayerInput>();
        startTime = Time.realtimeSinceStartup;
        coolTime = 0.2f;
        inputValue = 0.8f;
        inputValueV = 0;
        inputValueH = 0;
        inputValueVBefore = 0;
        inputValueHBefore = 0;
        UIAction.actions["On"].started += EnterButton;
        UIAction.actions["Set"].performed += MoveSet;
    }

    private void OnEnable()
    {
        if (UIAction != null)
        {
            UIAction.actions["On"].started += EnterButton;
            UIAction.actions["Set"].performed += MoveSet;
        }
        startTime = Time.realtimeSinceStartup;
        MoveSetVertical(currentButtonIndex);
    }

    void OnDisable()
    {
        UIAction.actions["On"].started -= EnterButton;
        UIAction.actions["Set"].performed -= MoveSet;
        isCoolTime = false;
    }

    public void MoveSet(InputAction.CallbackContext context)
    {
        inputValueV = context.ReadValue<Vector2>().y;
        inputValueH = context.ReadValue<Vector2>().x;
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
            switch (buttonType)
            {
                case ButtonType.both:
                    //Exitに行く処理
                    if (currentButtonIndex == verticalCount && inputValueV <= (-1) * inputValue)
                    {
                        MoveSetVertical(buttons.Length - 1 - verticalCount);
                    }
                    //Exitから戻る処理
                    else if (currentButtonIndex == buttons.Length - 1 && inputValueV >= inputValue)
                    {
                        MoveSetVertical(buttons.Length - 1 - verticalCount);
                    }
                    //Titleから戻る処理
                    else if (currentButtonIndex == 0 && inputValueH >=  inputValue)
                    {
                        MoveSetHorizontal(1);
                    }
                    else
                    {
                        MoveSetVertical(1);
                        MoveSetHorizontal(verticalCount);
                    }
                    break;
                case ButtonType.vertical:
                    MoveSetVertical(1);
                    break;
                case ButtonType.horizontal:
                    //Exitに行く処理
                    if (currentButtonIndex == 0 && inputValueV <= (-1) * inputValue)
                    {
                        MoveSetVertical(buttons.Length - 1);
                    }
                    //Exitから戻る処理
                    else if (currentButtonIndex == buttons.Length - 1 && inputValueV >= inputValue)
                    {
                        MoveSetVertical(buttons.Length - 1 );
                    }
                    else
                    {
                        MoveSetHorizontal(verticalCount);
                    }
                    break;
            }
        }

        inputValueVBefore = inputValueV;
        inputValueHBefore = inputValueH;
    }

    void MoveSetVertical(int next)
    {
        if (inputValueV >= inputValue && inputValueV != inputValueVBefore)
        {
            NextButton((-1) * next);
        }
        else if (inputValueV <= (-1) * inputValue && inputValueV != inputValueVBefore)
        {
            NextButton(next);
        }
    }

    void MoveSetHorizontal(int next)
    {
        if (inputValueH >= inputValue && inputValueV != inputValueHBefore)
        {
            NextButton(next);
        }
        else if (inputValueH <= (-1) * inputValue && inputValueV != inputValueHBefore)
        {
            NextButton((-1) * next);
        }
    }
    void NextButton(int next)
    {
        if (next > 0)
        {
            if (currentButtonIndex + next < buttons.Length)
            {
                currentButtonIndex += next;
            }
            else
            {
                currentButtonIndex = buttons.Length - 1;
            }
            cursol.transform.localPosition = buttons[currentButtonIndex].transform.localPosition;
            isCoolTime = true;
            startTime = Time.realtimeSinceStartup;
        }
        else if (next < 0)
        {
            if (currentButtonIndex + next >= 0)
            {
                currentButtonIndex += next;
            }
            else
            {
                currentButtonIndex = 0;
            }
            cursol.transform.localPosition = buttons[currentButtonIndex].transform.localPosition;
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
