// Factory Frenzy – builds the game world around the level: sky, clouds, floating factories.
// Put this file in Assets/Editor/. Open a scene and click:  Factory Frenzy > Build Environment
// It is an editor TOOL: it does not run in the game.
//
// It creates the "Environment" object in the scene AND saves it as a prefab:
//   Assets/FactoryFrenzyGen/Prefabs/Environment.prefab
// Give this prefab (+ its materials and mesh) to the VR person: same world in both apps.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class EnvironmentBuilder
{
    const string Root = "Assets/FactoryFrenzyGen";
    const string MatDir = Root + "/Materials";
    const string MeshDir = Root + "/Meshes";
    const string PrefabDir = Root + "/Prefabs";

    // the level goes roughly from z = 0 (start) to z = 60 (finish): the world is built around it
    static readonly Vector3 LevelCenter = new Vector3(0, 0, 30);

    [MenuItem("Factory Frenzy/Build Environment (sky, clouds, factories)")]
    public static void Build()
    {
        MakeFolder("Assets", "FactoryFrenzyGen");
        MakeFolder(Root, "Materials");
        MakeFolder(Root, "Meshes");
        MakeFolder(Root, "Prefabs");

        // remove an old version
        GameObject old = GameObject.Find("Environment");
        if (old != null) Object.DestroyImmediate(old);

        GameObject environment = new GameObject("Environment");

        // ---------- sky, fog, light (applied now, and again in the game by EnvironmentSettings)
        EnvironmentSettings settings = environment.AddComponent<EnvironmentSettings>();
        settings.skybox = MakeSky();
        MakeSunIfMissing();
        settings.Apply();

        // ---------- clouds far below the level (a soft "floor" of the world)
        Transform clouds = new GameObject("Clouds").transform;
        clouds.SetParent(environment.transform, false);
        Material cloudMat = Mat("FF_Cloud", "#FFFFFF", 0f);
        Random.InitState(7); // same clouds every time
        for (int i = 0; i < 90; i++)
        {
            float size = Random.Range(8f, 22f);
            Vector3 position = LevelCenter + new Vector3(Random.Range(-160f, 160f), Random.Range(-56f, -46f), Random.Range(-160f, 160f));
            GameObject cloud = Shape(PrimitiveType.Sphere, "Cloud", clouds, position, new Vector3(size, size * 0.45f, size), cloudMat);
            cloud.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        // ---------- floating factories in the distance (decoration only)
        Transform factories = new GameObject("FloatingFactories").transform;
        factories.SetParent(environment.transform, false);
        Island(factories, new Vector3(-62, -8, 85), 1.5f);
        Island(factories, new Vector3(70, -4, 120), 1.9f);
        Island(factories, new Vector3(-30, 4, 175), 2.4f);
        Island(factories, new Vector3(58, -16, 10), 1.1f);
        Island(factories, new Vector3(-75, -12, -20), 1.3f);

        // ---------- save as a prefab (the scene keeps a linked copy)
        string path = PrefabDir + "/Environment.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(environment, path, InteractionMode.UserAction);

        EditorSceneManager.MarkSceneDirty(environment.scene);
        Selection.activeGameObject = environment;
        EditorUtility.DisplayDialog("Factory Frenzy", "Environment created and saved as\n" + path + "\n\nSave the scene (Ctrl+S).", "OK");
    }

    // ------------------------------------------------------------------ one floating factory island
    static void Island(Transform parent, Vector3 position, float scale)
    {
        Transform island = new GameObject("Island").transform;
        island.SetParent(parent, false);
        island.localPosition = position;
        island.localScale = Vector3.one * scale;

        Material rock = Mat("FF_IslandRock", "#8F9AB0");
        Material top = Mat("FF_IslandTop", "#B8C2D6");
        Material pink = Mat("FF_IslandPink", "#EF5DA8");
        Material blue = Mat("FF_IslandBlue", "#3D7BE0");
        Material metal = Mat("FF_IslandMetal", "#5B6375", 0.5f, 0.4f);
        Material yellow = Mat("FF_IslandYellow", "#FFC93C");
        Material smoke = Mat("FF_Cloud", "#FFFFFF", 0f);

        // upside-down rock cone under the island
        GameObject cone = new GameObject("Rock");
        cone.transform.SetParent(island, false);
        cone.transform.localPosition = new Vector3(0, -4f, 0);
        cone.AddComponent<MeshFilter>().sharedMesh = ConeMesh();
        cone.AddComponent<MeshRenderer>().sharedMaterial = rock;

        Shape(PrimitiveType.Cube, "Ground", island, new Vector3(0, 0.2f, 0), new Vector3(12, 1, 12), top);
        Shape(PrimitiveType.Cube, "FactoryPink", island, new Vector3(-2, 3.6f, 0), new Vector3(5, 6, 4), pink);
        Shape(PrimitiveType.Cube, "FactoryBlue", island, new Vector3(3, 2.6f, -2), new Vector3(3, 4, 3), blue);
        Shape(PrimitiveType.Cylinder, "Chimney", island, new Vector3(-3, 5f, 2), new Vector3(1.4f, 4.5f, 1.4f), metal);
        Shape(PrimitiveType.Cylinder, "ChimneyStripe", island, new Vector3(-3, 9.2f, 2), new Vector3(1.5f, 0.3f, 1.5f), yellow);
        Shape(PrimitiveType.Cylinder, "Tank", island, new Vector3(0, 4f, 3), new Vector3(1.2f, 3.5f, 1.2f), metal);

        // yellow pipe: two legs and a top bar
        Shape(PrimitiveType.Cylinder, "PipeLeft", island, new Vector3(-1.5f, 4.5f, -1.5f), new Vector3(0.7f, 2.2f, 0.7f), yellow);
        Shape(PrimitiveType.Cylinder, "PipeRight", island, new Vector3(3.5f, 4.5f, -1.5f), new Vector3(0.7f, 2.2f, 0.7f), yellow);
        GameObject bar = Shape(PrimitiveType.Cylinder, "PipeTop", island, new Vector3(1f, 6.7f, -1.5f), new Vector3(0.7f, 2.5f, 0.7f), yellow);
        bar.transform.localRotation = Quaternion.Euler(0, 0, 90);

        // smoke puffs above the chimney
        for (int k = 0; k < 3; k++)
        {
            float s = 1.6f + k * 0.5f;
            GameObject puff = Shape(PrimitiveType.Sphere, "Smoke", island, new Vector3(-3 + k * 0.6f, 10.5f + k * 1.4f, 2), new Vector3(s, s * 0.8f, s), smoke);
            puff.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    // ------------------------------------------------------------------ sky and sun

    // Unity's procedural sky, tuned to a bright cartoon sky (blue on top, light pink at the horizon)
    static Material MakeSky()
    {
        string path = MatDir + "/FF_Sky.mat";
        Material sky = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (sky == null)
        {
            sky = new Material(Shader.Find("Skybox/Procedural"));
            AssetDatabase.CreateAsset(sky, path);
        }
        sky.SetColor("_SkyTint", new Color(0.40f, 0.62f, 1f));
        sky.SetColor("_GroundColor", new Color(1f, 0.90f, 0.94f));
        sky.SetFloat("_Exposure", 1.3f);
        sky.SetFloat("_AtmosphereThickness", 0.7f);
        sky.SetFloat("_SunSize", 0.04f);
        EditorUtility.SetDirty(sky);
        return sky;
    }

    static void MakeSunIfMissing()
    {
        foreach (Light light in Object.FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional) return; // the scene already has a sun
        }
        Light sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    // ------------------------------------------------------------------ helpers

    // a Unity primitive without collider (decoration must not block the player)
    static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic = true;
        return go;
    }

    // an upside-down cone (Unity has no cone primitive), saved as an asset so the prefab keeps it
    static Mesh ConeMesh()
    {
        string path = MeshDir + "/FF_IslandCone.asset";
        Mesh saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (saved != null) return saved;

        const int sides = 7;
        const float radius = 6f, height = 8f;
        Vector3[] vertices = new Vector3[sides * 3];
        int[] triangles = new int[sides * 3];
        for (int i = 0; i < sides; i++)
        {
            float a1 = i * Mathf.PI * 2 / sides;
            float a2 = (i + 1) * Mathf.PI * 2 / sides;
            vertices[i * 3] = new Vector3(Mathf.Cos(a1) * radius, height / 2, Mathf.Sin(a1) * radius);
            vertices[i * 3 + 1] = new Vector3(Mathf.Cos(a2) * radius, height / 2, Mathf.Sin(a2) * radius);
            vertices[i * 3 + 2] = new Vector3(0, -height / 2, 0); // the tip points down
            triangles[i * 3] = i * 3;
            triangles[i * 3 + 1] = i * 3 + 1;
            triangles[i * 3 + 2] = i * 3 + 2;
        }
        Mesh mesh = new Mesh { name = "FF_IslandCone", vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    // same material style as the prefab builder (works with URP and Built-in)
    static Material Mat(string name, string hex, float smoothness = 0.35f, float metallic = 0.1f)
    {
        string path = MatDir + "/" + name + ".mat";
        bool urp = GraphicsSettings.currentRenderPipeline != null;
        Shader shader = urp ? Shader.Find("Universal Render Pipeline/Lit") : null;
        if (shader == null) shader = Shader.Find("Standard");

        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, path);
        }
        ColorUtility.TryParseHtmlString(hex, out Color color);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(m);
        return m;
    }

    static void MakeFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }
}
#endif

