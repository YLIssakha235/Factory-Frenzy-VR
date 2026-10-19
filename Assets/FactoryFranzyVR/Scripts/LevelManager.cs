using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private readonly List<EditableObject> registeredObjects = new List<EditableObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterObject(EditableObject obj)
    {
        if (!registeredObjects.Contains(obj)) registeredObjects.Add(obj);
    }

    public void UnregisterObject(EditableObject obj)
    {
        if (registeredObjects.Contains(obj)) registeredObjects.Remove(obj);
    }

    public string GetFullFilePath(string levelName)
    {
        // CHANGED: Now saves to the StreamingAssets folder
        string dir = Path.Combine(Application.streamingAssetsPath, "ExportedLevels");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string sanitized = string.IsNullOrWhiteSpace(levelName) ? "Level_Default" : levelName.Trim();
        return Path.Combine(dir, $"{sanitized}.json");
    }

    // Used by the UI to check for the overwrite warning
    public bool DoesLevelFileExist(string levelName)
    {
        return File.Exists(GetFullFilePath(levelName));
    }

    public void SaveLevel(string levelName)
    {
        string filePath = GetFullFilePath(levelName);
        LevelData levelData = new LevelData
        {
            levelName = Path.GetFileNameWithoutExtension(filePath),
            exportedAt = DateTime.UtcNow.ToString("o")
        };

        registeredObjects.RemoveAll(item => item == null);
        foreach (var editable in registeredObjects)
        {
            levelData.objects.Add(editable.ToData());
        }

        // Fulfills the JsonUtility requirement
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"[LevelManager] Exported JSON to: {filePath}");

        // CHANGED: Forces Unity to refresh the Project window so the file appears instantly
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    [ContextMenu("Test Save Level")]
    public void TestSaveLevel()
    {
        // This will automatically save a file named "Test_Level_01.json"
        SaveLevel("Test_Level_01");
    }
}