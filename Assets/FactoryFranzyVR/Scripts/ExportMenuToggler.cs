using UnityEngine;
using UnityEngine.InputSystem;

public class ExportMenuToggler : MonoBehaviour
{
    [Header("Menu Setup")]
    [Tooltip("The Canvas GameObject that holds your Export UI")]
    [SerializeField] private GameObject exportMenuCanvas;

    [Tooltip("Your XR Main Camera (to know where the player is looking)")]
    [SerializeField] private Transform vrCamera;

    [Tooltip("How far in front of the player the menu should spawn")]
    [SerializeField] private float spawnDistance = 1.0f;

    [Header("Input Setup")]
    [Tooltip("The button used to open/close the menu (e.g., Left Controller Menu button)")]
    [SerializeField] private InputActionReference menuButton;

    private void OnEnable()
    {
        if (menuButton != null) menuButton.action.Enable();
    }

    private void OnDisable()
    {
        if (menuButton != null) menuButton.action.Disable();
    }

    private void Update()
    {
        // Toggle the menu exactly when the button is pressed down
        if (menuButton != null && menuButton.action.WasPressedThisFrame())
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (exportMenuCanvas == null) return;

        // Flip the active state
        bool isOpening = !exportMenuCanvas.activeSelf;
        exportMenuCanvas.SetActive(isOpening);

        // If we are opening the menu, move it in front of the player
        if (isOpening && vrCamera != null)
        {
            // Place it 1 meter in front of the camera's current rotation
            Vector3 spawnPosition = vrCamera.position + (vrCamera.forward * spawnDistance);
            // Keep the menu at eye level by ignoring the camera's vertical tilt
            spawnPosition.y = vrCamera.position.y;

            exportMenuCanvas.transform.position = spawnPosition;

            // Make the canvas look at the camera
            exportMenuCanvas.transform.LookAt(vrCamera);
            // Flip it 180 degrees so the UI doesn't render backwards
            exportMenuCanvas.transform.Rotate(0, 180, 0);
        }
    }
}