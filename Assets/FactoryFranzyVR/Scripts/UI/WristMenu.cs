using UnityEngine;

public class WristMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject menuCanvas;
    public Transform playerCamera;

    [Header("Settings")]
    public float activationAngle = 45f;

    private Vector3 originalScale;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }

        if (menuCanvas != null)
        {
            // Remember the tiny scale you set (e.g., 0.001)
            originalScale = menuCanvas.transform.localScale;

            // Hide it on start by shrinking it to 0
            menuCanvas.transform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        if (menuCanvas == null || playerCamera == null) return;

        Vector3 directionToHead = (playerCamera.position - transform.position).normalized;

        // Note: Using transform.forward assuming your UI faces outward
        float wristAngle = Vector3.Angle(transform.forward, directionToHead);
        float lookAngle = Vector3.Angle(playerCamera.forward, -directionToHead);

        bool isLookingAtWatch = (wristAngle < activationAngle) && (lookAngle < activationAngle);

        // Pop the menu in and out by changing its scale instead of turning it off
        menuCanvas.transform.localScale = isLookingAtWatch ? originalScale : Vector3.zero;
    }
}