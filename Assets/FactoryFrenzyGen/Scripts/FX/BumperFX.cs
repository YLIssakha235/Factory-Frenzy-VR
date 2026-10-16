using UnityEngine;

// PC-TRA-T1 – the bumper swells when the player touches it.
// The bumper script calls Pulse().
public class BumperFX : MonoBehaviour
{
    Transform body;
    Vector3 normalSize;
    float timer = 1; // 1 = not animating

    void Start()
    {
        body = FindChild("Body");
        if (body != null) normalSize = body.localScale;
        else Debug.LogWarning("BumperFX: no child called Body in " + name);
    }

    Transform FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == childName) return t;
        }
        return null;
    }

    public void Pulse()
    {
        timer = 0;
    }

    void Update()
    {
        if (body == null) return;

        if (timer < 0.3f)
        {
            timer += Time.deltaTime;
            // big at the start, back to normal after 0.3 seconds
            float big = Mathf.Max(0, 1 - timer / 0.3f);
            body.localScale = new Vector3(normalSize.x * (1 + 0.35f * big), normalSize.y, normalSize.z * (1 + 0.35f * big));
        }
    }
}
