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
        if (tooltipText != null) tooltipText.text = prefabName;
        if (tooltipBackground != null) tooltipBackground.SetActive(true);

        if (eventData is TrackedDeviceEventData trackedEventData)
        {
            if (trackedEventData.interactor is XRRayInteractor rayInteractor)
            {
                if (rayInteractor.xrController != null)
                {
                    rayInteractor.xrController.SendHapticImpulse(vibrationIntensity, vibrationDuration);
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipText != null) tooltipText.text = "";
        if (tooltipBackground != null) tooltipBackground.SetActive(false);
    }
}