using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class HoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Info")]
    [Tooltip("The name to display when hovering over this specific panel")]
    public string prefabName = "Platform";

    [Header("Menu Tooltip References")]
    public TextMeshProUGUI tooltipText;
    public GameObject tooltipBackground;

    [Header("Haptics")]
    [Tooltip("Strength of the vibration (0.0 to 1.0)")]
    public float vibrationIntensity = 0.5f;
    [Tooltip("How long the vibration lasts in seconds")]
    public float vibrationDuration = 0.1f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. Show the Tooltip
        if (tooltipText != null) tooltipText.text = prefabName;
        if (tooltipBackground != null) tooltipBackground.SetActive(true);

        // 2. Play Vibration on the controller that triggered the event
        if (eventData is TrackedDeviceEventData trackedEventData)
        {
            // Check if the thing interacting with the UI is an XR Ray Interactor
            if (trackedEventData.interactor is XRRayInteractor rayInteractor)
            {
                // Send the vibration to that specific controller
                if (rayInteractor.xrController != null)
                {
                    rayInteractor.xrController.SendHapticImpulse(vibrationIntensity, vibrationDuration);
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the Tooltip
        if (tooltipText != null) tooltipText.text = "";
        if (tooltipBackground != null) tooltipBackground.SetActive(false);
    }
}