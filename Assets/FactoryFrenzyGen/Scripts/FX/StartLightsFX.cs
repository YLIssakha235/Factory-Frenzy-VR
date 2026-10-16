using UnityEngine;

// PC-PLA-P2 – the 3 lights on the start gate.
// The countdown calls SetCountdown(3), (2), (1), then (0) for GO.
public class StartLightsFX : MonoBehaviour
{
    Renderer[] lights = new Renderer[3];

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            Transform light = FindChild("Light" + (i + 1)); // Light1, Light2, Light3
            if (light != null) lights[i] = light.GetComponent<Renderer>();
            else Debug.LogWarning("StartLightsFX: no child called Light" + (i + 1) + " in " + name);
        }
    }

    Transform FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == childName) return t;
        }
        return null;
    }

    // 3 → 1 red light, 2 → 2 red lights, 1 → 3 red lights, 0 → all green
    public void SetCountdown(int number)
    {
        for (int i = 0; i < 3; i++)
        {
            if (lights[i] == null) continue;

            Color color = Color.gray;
            if (number == 0) color = Color.green;
            else if (i < 4 - number) color = Color.red;

            lights[i].material.color = color;
            lights[i].material.EnableKeyword("_EMISSION"); // make it glow
            lights[i].material.SetColor("_EmissionColor", color == Color.gray ? Color.black : color);
        }
    }
}
