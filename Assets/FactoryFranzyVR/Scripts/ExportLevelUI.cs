using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExportLevelUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField levelNameInput;
    [SerializeField] private Button exportButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        exportButton.onClick.AddListener(OnExportClicked);
    }

    private void OnDestroy()
    {
        exportButton.onClick.RemoveListener(OnExportClicked);
    }

    private void OnExportClicked()
    {
        string levelName = levelNameInput.text;

        if (string.IsNullOrWhiteSpace(levelName))
        {
            if (statusText != null)
            {
                statusText.text = "Please enter a level name!";
                statusText.color = Color.yellow;
            }
            return;
        }

        // Save directly via the LevelManager
        LevelManager.Instance.SaveLevel(levelName);

        if (statusText != null)
        {
            statusText.text = $"Exported: {levelName}.json";
            statusText.color = Color.green;
        }
    }
}