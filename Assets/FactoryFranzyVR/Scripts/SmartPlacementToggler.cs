using UnityEngine;
using UnityEngine.InputSystem;

public class SmartPlacementToggler : MonoBehaviour
{
    // Global variable that all objects can check
    public static bool IsSmartModeActive = false;

    [SerializeField] private InputActionProperty toggleButton;

    private void OnEnable()
    {
        if (toggleButton.action != null)
        {
            toggleButton.action.Enable();
            toggleButton.action.performed += OnTogglePressed;
        }
    }

    private void OnDisable()
    {
        if (toggleButton.action != null)
        {
            toggleButton.action.performed -= OnTogglePressed;
            toggleButton.action.Disable();
        }
    }

    private void OnTogglePressed(InputAction.CallbackContext context)
    {
        IsSmartModeActive = !IsSmartModeActive;
        Debug.Log($"[Smart Placement] Mode active: {IsSmartModeActive}");
    }
}