using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField]
    private List<EditableObject> placedObjects = new List<EditableObject>();

    public IReadOnlyList<EditableObject> PlacedObjects => placedObjects;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterObject(EditableObject obj)
    {
        if (!placedObjects.Contains(obj))
        {
            placedObjects.Add(obj);
        }
    }

    public void UnregisterObject(EditableObject obj)
    {
        if (placedObjects.Contains(obj))
        {
            placedObjects.Remove(obj);
        }
    }

    public void ClearAll()
    {
        foreach (var obj in placedObjects)
        {
            if (obj != null)
            {
                Destroy(obj.gameObject);
            }
        }
        placedObjects.Clear();
    }
}