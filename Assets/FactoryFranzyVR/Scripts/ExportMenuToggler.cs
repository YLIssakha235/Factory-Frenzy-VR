using UnityEngine;
using UnityEngine.InputSystem;

public class ExportMenuToggler : MonoBehaviour
{
    [Header("Menu Setup")]
    [SerializeField] private GameObject exportMenuCanvas;
    [SerializeField] private Transform vrCamera;
    [SerializeField] private float spawnDistance = 1.0f;

    // No input variables, no Update loop!

    // This method is now triggered externally by the Player Input component
    public void ToggleMenu(InputAction.CallbackContext context)
    {
        // Only fire once when the button is fully pressed down
        if (!context.performed) return;

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