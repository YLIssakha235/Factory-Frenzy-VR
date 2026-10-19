using UnityEngine;
using UnityEngine.InputSystem;

public class ExportMenuToggler : MonoBehaviour
{
    [Header("Menu Setup")]
    [SerializeField] private GameObject exportMenuCanvas;
    [SerializeField] private Transform vrCamera;
    [SerializeField] private float spawnDistance = 1.0f;

    [Header("Input Binding")]
    [SerializeField] private InputActionProperty menuButtonAction;

    private void OnEnable()
    {
        if (menuButtonAction.action != null)
        {
            menuButtonAction.action.Enable();
            menuButtonAction.action.performed += OnMenuButtonPressed;
        }
    }

    private void OnDisable()
    {
        if (menuButtonAction.action != null)
        {
            menuButtonAction.action.performed -= OnMenuButtonPressed;
            menuButtonAction.action.Disable();
        }
    }

    private void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        if (exportMenuCanvas == null) return;

        bool isOpening = !exportMenuCanvas.activeSelf;
        exportMenuCanvas.SetActive(isOpening);

        if (isOpening && vrCamera != null)
        {
            Vector3 spawnPosition = vrCamera.position + (vrCamera.forward * spawnDistance);
            spawnPosition.y = vrCamera.position.y;

            exportMenuCanvas.transform.position = spawnPosition;
            exportMenuCanvas.transform.LookAt(vrCamera);
            exportMenuCanvas.transform.Rotate(0, 180, 0);
        }
    }
}