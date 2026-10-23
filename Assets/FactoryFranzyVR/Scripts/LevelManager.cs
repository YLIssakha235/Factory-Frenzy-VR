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
        string dir = Path.Combine(Application.streamingAssetsPath, "ExportedLevels");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        string sanitized = string.IsNullOrWhiteSpace(levelName) ? "Level_Default" : levelName.Trim();
        return Path.Combine(dir, $"{sanitized}.json");
    }

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

        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"[LevelManager] Exported JSON to: {filePath}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    [ContextMenu("Test Save Level")]
    public void TestSaveLevel()
    {
        SaveLevel("Test_Level_01");
    }
}