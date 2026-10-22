using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GetKeyCode : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    public Color32 mNormalColor = Color.white;
    public Color32 mHoverColor = Color.gray;
    public Color32 mDownColor = Color.red;

    private OpenVirtualKeyboard keyboardController;
    private Image buttonImage;
    private string buttonString;
    private Text showString;
    private InputField inputTarget;

    private bool toLowLetterCase;
    private readonly CultureInfo cult =
        new CultureInfo("en-US", false);

    private void OnEnable()
    {
        if (keyboardController == null)
        {
            GameObject controller =
                GameObject.Find("Virtual Keyboard Controller");

            if (controller != null)
                keyboardController =
                    controller.GetComponent<OpenVirtualKeyboard>();
        }

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (buttonString == null)
            buttonString = transform.name;

        if (showString == null)
        {
            Transform textObject = transform.Find("Text");

            if (textObject != null)
                showString = textObject.GetComponent<Text>();
        }
    }

    private void Update()
    {
        if (toLowLetterCase == LetterCaseDetection.Lowercase)
            return;

        toLowLetterCase = LetterCaseDetection.Lowercase;

        if (Regex.IsMatch(buttonString, "^[a-zA-Z0-9]*$") &&
            !IsSpecialKey(buttonString))
        {
            buttonString = toLowLetterCase
                ? buttonString.ToLower(cult)
                : buttonString.ToUpper(cult);

            if (showString != null)
                showString.text = buttonString;
        }
    }

    private bool IsSpecialKey(string key)
    {
        return key == "delete" ||
               key == "clear" ||
               key == "backward" ||
               key == "forward" ||
               key == "Letter case" ||
               key == "To0" ||
               key == "ToLast";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (keyboardController != null)
            keyboardController.onExitKeyboardArea = false;

        if (buttonImage != null)
            buttonImage.color = mHoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (keyboardController != null)
            keyboardController.onExitKeyboardArea = false;

        if (buttonImage != null)
            buttonImage.color = mNormalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.color = mDownColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.color = mHoverColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (keyboardController != null)
            keyboardController.onExitKeyboardArea = false;

        if (buttonImage != null)
            buttonImage.color = mHoverColor;

        string target = GetInputFieldTarget.SelectInputFieldName;

        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("No input field selected.");
            return;
        }

        GameObject targetObject = GameObject.Find(target);

        if (targetObject == null)
        {
            Debug.LogWarning("Input field not found: " + target);
            return;
        }

        inputTarget = targetObject.GetComponent<InputField>();

        if (inputTarget == null)
        {
            Debug.LogWarning(
                "No standard InputField component on: " + target);
            return;
        }

        string targetText = inputTarget.text;

        int index = Mathf.Clamp(
            GetInputFieldTarget.Index,
            0,
            targetText.Length
        );

        GetInputFieldTarget.Index = index;

        if (!IsSpecialKey(buttonString))
        {
            inputTarget.text =
                targetText.Insert(index, buttonString);

            GetInputFieldTarget.Index =
                index + buttonString.Length;

            return;
        }

        switch (buttonString)
        {
            case "delete":
                if (index > 0)
                {
                    inputTarget.text =
                        targetText.Remove(index - 1, 1);

                    GetInputFieldTarget.Index = index - 1;
                }
                break;

            case "clear":
                inputTarget.text = "";
                GetInputFieldTarget.Index = 0;
                break;

            case "backward":
                GetInputFieldTarget.Index =
                    Mathf.Max(0, index - 1);
                break;

            case "forward":
                GetInputFieldTarget.Index =
                    Mathf.Min(targetText.Length, index + 1);
                break;

            case "Letter case":
                LetterCaseDetection.Lowercase =
                    !LetterCaseDetection.Lowercase;
                break;

            case "ToLast":
                GetInputFieldTarget.Index =
                    inputTarget.text.Length;
                break;

            case "To0":
                GetInputFieldTarget.Index = 0;
                break;
        }
    }
}