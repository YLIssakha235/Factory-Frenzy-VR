using UnityEngine;
using UnityEngine.Rendering;

// Factory Frenzy – applies the sky, fog and light colors when the scene starts.
// It is on the Environment prefab: drop the prefab in a scene (PC game or VR editor)
// and you get the same look everywhere. (Sky, fog and ambient light are scene settings in Unity,
// so they are not saved inside a prefab: this script sets them by code.)
public class EnvironmentSettings : MonoBehaviour
{
    public Material skybox;                                         // FF_Sky (made by the Environment builder)

    [Header("Fog (hides the far end of the world softly)")]
    public Color fogColor = new Color(0.86f, 0.92f, 1f);           // light blue
    public float fogStart = 60f;
    public float fogEnd = 220f;

    [Header("Ambient light (sky / horizon / ground)")]
    public Color skyLight = new Color(0.87f, 0.91f, 1f);
    public Color horizonLight = new Color(1f, 0.90f, 0.94f);
    public Color groundLight = new Color(0.73f, 0.78f, 0.90f);

    [Header("Sun")]
    public Color sunColor = new Color(1f, 0.96f, 0.88f);
    public float sunIntensity = 1.1f;

    void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        if (skybox != null) RenderSettings.skybox = skybox;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = fogStart;
        RenderSettings.fogEndDistance = fogEnd;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = skyLight;
        RenderSettings.ambientEquatorColor = horizonLight;
        RenderSettings.ambientGroundColor = groundLight;

        // the sun = the first directional light of the scene
        foreach (Light light in FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional)
            {
                light.color = sunColor;
                light.intensity = sunIntensity;
                light.shadows = LightShadows.Soft;
                RenderSettings.sun = light;
                break;
            }
        }

        // the camera must draw the sky (not a plain color)
        Camera cam = Camera.main;
        if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;
    }
}

