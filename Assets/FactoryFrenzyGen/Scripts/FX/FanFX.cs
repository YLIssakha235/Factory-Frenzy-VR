using UnityEngine;

// PC-TRA-T3 – the fan blades turn all the time.
public class FanFX : MonoBehaviour
{
    public float speed = 1500; // degrees per second
    Transform blades;

    void Start()
    {
        blades = FindChild("Blades");
        if (blades == null) Debug.LogWarning("FanFX: no child called Blades in " + name);
    }

    Transform FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == childName) return t;
        }
        return null;
    }

    void Update()
    {
        if (blades != null)
        {
            blades.Rotate(0, 0, speed * Time.deltaTime);
        }
    }
}
