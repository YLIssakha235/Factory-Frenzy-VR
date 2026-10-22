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
        // 1. Smart Snap
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

        // 2. Delete Mode
        if (deleteModeText != null)
        {
            // REPLACE 'IsDeleteModeActive' with the actual variable name in your DeleteManager script
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

        // 3. Freeze Mode
        if (freezeModeText != null)
        {
            // REPLACE 'IsFreezeModeActive' with the actual variable name in your LockTool script
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