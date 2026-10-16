using UnityEngine;

// PC-PLA-P7 – the flag waves, and Activate() turns the arch green.
// Checkpoint.cs calls Activate() when the player passes.
public class CheckpointFX : MonoBehaviour
{
    Transform arch;
    Transform flag;
    bool isOn = false;

    void Start()
    {
        arch = FindChild("Arch");
        flag = FindChild("Flag");
        if (arch == null) Debug.LogWarning("CheckpointFX: no child called Arch in " + name);
    }

    Transform FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == childName) return t;
        }
        return null;
    }

    public void Activate()
    {
        isOn = true;
        if (arch == null) return;

        // every part of the arch becomes green and glows
        foreach (Renderer part in arch.GetComponentsInChildren<Renderer>())
        {
            part.material.color = Color.green;
            part.material.EnableKeyword("_EMISSION");
            part.material.SetColor("_EmissionColor", Color.green * 0.5f);
        }
    }

    void Update()
    {
        if (flag == null) return;

        // the flag waves (faster when activated)
        float speed = isOn ? 8 : 2.5f;
        flag.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * speed) * 12, 0);
    }
}
