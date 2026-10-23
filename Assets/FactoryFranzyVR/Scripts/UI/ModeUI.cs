using UnityEngine;
using TMPro;

public class ModesUI : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI smartModeText;
    public TextMeshProUGUI deleteModeText;
    public TextMeshProUGUI freezeModeText;

    void Update()
    {
        if (smartModeText != null)
        {
            if (SmartPlacementToggler.IsSmartModeActive)
            {
                smartModeText.text = "Smart: ON";
                smartModeText.color = Color.green;
            }
            else
            {
                smartModeText.text = "Smart Snap: OFF";
                smartModeText.color = Color.red;
            }
        }

        if (deleteModeText != null)
        {
            if (DeleteManager.isDeleteModeActive)
            {
                deleteModeText.text = "Delete: ON";
                deleteModeText.color = Color.green;
            }
            else
            {
                deleteModeText.text = "Delete: OFF";
                deleteModeText.color = Color.red;
            }
        }

        if (freezeModeText != null)
        {
            if (LockTool.isLockModeActive)
            {
                freezeModeText.text = "Freeze: ON";
                freezeModeText.color = Color.green;
            }
            else
            {
                freezeModeText.text = "Freeze: OFF";
                freezeModeText.color = Color.red;
            }
        }
    }
}