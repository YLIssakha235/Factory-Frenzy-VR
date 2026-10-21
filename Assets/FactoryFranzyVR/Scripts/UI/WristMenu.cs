using UnityEngine;

public class WristMenu : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your Menu Canvas here")]
    public GameObject menuCanvas;

    [Tooltip("Leave empty to auto-find the Main Camera (Player Head)")]
    public Transform playerCamera;

    [Header("Settings")]
    [Tooltip("How precisely you must look at the watch (lower = stricter, 45 is a good default)")]
    public float activationAngle = 45f;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }

        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false); // Hide on start
        }
    }

    private void Update()
    {
        if (menuCanvas == null || playerCamera == null) return;

        // 1. Get the direction from the wrist to the player's head
        Vector3 directionToHead = (playerCamera.position - transform.position).normalized;

        // 2. Check if the wrist is tilted toward the face
        // Unity UI Canvases face the -Z direction by default, so we use -transform.forward
        float wristAngle = Vector3.Angle(-transform.forward, directionToHead);

        // 3. Check if the player's head is actually looking down at the wrist
        float lookAngle = Vector3.Angle(playerCamera.forward, -directionToHead);

        // 4. If both angles are within the threshold, show the menu
        bool isLookingAtWatch = (wristAngle < activationAngle) && (lookAngle < activationAngle);

        if (menuCanvas.activeSelf != isLookingAtWatch)
        {
            menuCanvas.SetActive(isLookingAtWatch);
        }
    }
}