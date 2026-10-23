using UnityEngine;

public class EditableObject : MonoBehaviour
{
    [Header("Platform Identity")]
    [Tooltip("Select which of the 9 types this prefab is")]
    public LevelItemType itemType;

    private string instanceId;

    private void Awake()
    {
        instanceId = System.Guid.NewGuid().ToString();
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterObject(this);
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterObject(this);
        }
    }

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