using UnityEngine;

public class OpenVirtualKeyboard : MonoBehaviour
{
    private GameObject virtualKeyboard;
    private RectTransform keyboardBackground;

    [HideInInspector]
    public bool onExitKeyboardArea;

    private void Awake()
    {
        virtualKeyboard = GameObject.Find("Virtual Keyboard");

        if (virtualKeyboard == null)
        {
            Debug.LogWarning("Virtual Keyboard not found in the scene.");
            enabled = false;
            return;
        }

        Transform background = virtualKeyboard.transform.Find("Background");

        if (background == null)
        {
            Debug.LogWarning("Background not found in Virtual Keyboard.");
            enabled = false;
            return;
        }

        keyboardBackground = background.GetComponent<RectTransform>();

        if (keyboardBackground == null)
        {
            Debug.LogWarning("Background has no RectTransform.");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (onExitKeyboardArea)
        {
            OnCloseVirtualKeyboard();
        }
        else
        {
            OnOpenVirtualKeyboard();
        }
    }

    public void OnOpenVirtualKeyboard()
    {
        if (virtualKeyboard == null || keyboardBackground == null)
            return;

        if (virtualKeyboard.activeSelf)
            return;

        SetupKeyboardSize();
        virtualKeyboard.SetActive(true);
    }

    public void OnCloseVirtualKeyboard()
    {
        if (virtualKeyboard == null)
            return;

        if (!virtualKeyboard.activeSelf)
            return;

        virtualKeyboard.SetActive(false);
    }

    private void SetupKeyboardSize()
    {
        float keyboardWidth = 485f;
        float keyboardHeight = 485f;

        keyboardBackground.sizeDelta =
            new Vector2(keyboardWidth, keyboardHeight);
    }
}