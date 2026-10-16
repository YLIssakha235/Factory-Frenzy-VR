using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class LevelObjectData
{
    public string type;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

[Serializable]
public class LevelData
{
    public List<LevelObjectData> objects = new List<LevelObjectData>();
}

public class LevelExporter : MonoBehaviour
{
    [Tooltip("Optional. If set, positions are saved relative to this transform.")]
    [SerializeField] Transform levelOrigin;

    string LevelsDir
    {
        get
        {
            string baseDir =
#if UNITY_EDITOR || UNITY_STANDALONE
                Application.streamingAssetsPath;
#else
                Application.persistentDataPath;
#endif
            return Path.Combine(baseDir, "levels");
        }
    }

    // Hook this to your button
    [ContextMenu("Save Level")]
    public void SaveNewLevel()
    {
        Directory.CreateDirectory(LevelsDir);

        int index = 1;
        while (File.Exists(Path.Combine(LevelsDir, $"level{index}.json")))
            index++;

        Export($"level{index}");
    }

    void Export(string levelName)
    {
        var data = new LevelData();

        foreach (var obj in FindObjectsByType<EditableObject>(FindObjectsSortMode.None))
        {
            Transform t = obj.transform;

            Vector3 pos = t.position;
            Quaternion rot = t.rotation;
            if (levelOrigin != null)
            {
                pos = levelOrigin.InverseTransformPoint(t.position);
                rot = Quaternion.Inverse(levelOrigin.rotation) * t.rotation;
            }

            data.objects.Add(new LevelObjectData
            {
                type = obj.itemType,
                position = pos,
                rotation = rot,
                scale = t.lossyScale
            });
        }

        string path = Path.Combine(LevelsDir, levelName + ".json");
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log($"Exported {data.objects.Count} objects to {path}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}