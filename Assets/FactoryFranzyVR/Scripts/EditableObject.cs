using UnityEngine;

public class EditableObject : MonoBehaviour
{
    [Header("Platform Identity")]
    [Tooltip("Select which of the 9 types this prefab is")]
    public LevelItemType itemType;

    private string instanceId;

    private void Awake()
    {
        // Generate a unique ID for this specific block in the level
        instanceId = System.Guid.NewGuid().ToString();
    }

    private void Start()
    {
        // Tell the LevelManager that this object exists in the scene
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterObject(this);
        }
    }

    private void OnDestroy()
    {
        // Remove it from the export list if you delete it with your VR laser
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterObject(this);
        }
    }

    // The LevelManager calls this when you press the Export button
    public ObjectData ToData()
    {
        return new ObjectData
        {
            instanceId = this.instanceId,
            itemType = this.itemType.ToString(),
            position = new SerializableVector3(transform.position),
            rotation = new SerializableQuaternion(transform.rotation),
            scale = new SerializableVector3(transform.localScale),
        };
    }
}