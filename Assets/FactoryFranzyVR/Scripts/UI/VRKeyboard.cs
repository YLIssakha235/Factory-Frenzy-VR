using UnityEngine;
using TMPro;

public class VRKeyboard : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag your TMP_InputField here")]
    public TMP_InputField targetInputField;

    // Called by letter buttons (A, B, C, etc.)
    public void TypeKey(string character)
    {
        if (targetInputField != null)
        {
            targetInputField.text += character;
        }
    }

    // Called by the Backspace button
    public void Backspace()
    {
        if (targetInputField != null && targetInputField.text.Length > 0)
        {
            targetInputField.text = targetInputField.text.Substring(0, targetInputField.text.Length - 1);
        }
    }

    // Called by a Clear button
    public void ClearText()
    {
        if (targetInputField != null)
        {
            targetInputField.text = "";
        }
    }
}