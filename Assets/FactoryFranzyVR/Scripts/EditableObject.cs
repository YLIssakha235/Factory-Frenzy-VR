using UnityEngine;

public class EditableObject : MonoBehaviour
{
    [Header("Item Info")]
    public string itemType = "Platform_Basic"; // ID matching the PC team's prefab list

    [Header("State")]
    public bool IsLocked = false;

    // Call this whenever an item is spawned
    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterObject(this);
        }
    }

    public void ToggleLock()
    {
        IsLocked = !IsLocked;
    }

    public class PlatformData
    {
        public string type;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}