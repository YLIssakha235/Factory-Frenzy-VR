// Factory Frenzy – prefab builder
// Put this file in an "Editor" folder of your Unity project (e.g. Assets/Editor/FactoryFrenzyPrefabBuilder.cs).
// Then use the menu:  Factory Frenzy > Build Prefabs
// It creates Assets/FactoryFrenzyGen/{Materials, Meshes, Prefabs} with every platform and trap.
// Works with the Built-in pipeline and URP (Unity 2022.3).

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class FactoryFrenzyPrefabBuilder
{
    const string Root = "Assets/FactoryFrenzyGen";
    const string MatDir = Root + "/Materials";
    const string MeshDir = Root + "/Meshes";
    const string PrefabDir = Root + "/Prefabs";

    static bool IsURP => GraphicsSettings.currentRenderPipeline != null;

    // ------------------------------------------------------------------ menu

    [MenuItem("Factory Frenzy/Build Prefabs")]
    public static void BuildAll()
    {
        EnsureFolders();

        BuildStart();
        BuildFinish();
        BuildClassic();
        BuildMoving();
        BuildTrampoline();
        BuildCheckpoint();
        BuildBumper();
        BuildLauncher();
        BuildProjectile();
        BuildFan();
        BuildKillZone();
        BuildPlayerBean();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Factory Frenzy",
            "Prefabs created in " + PrefabDir + ".\n\nUse 'Factory Frenzy > Place All In Scene' to see them side by side.", "OK");
    }

    [MenuItem("Factory Frenzy/Place All In Scene")]
    public static void PlaceAll()
    {
        string[] names = { "Start", "Classic", "Moving", "Trampoline", "Checkpoint", "Bumper", "Launcher", "Fan", "Finish", "PlayerBean" };
        var parent = new GameObject("FactoryFrenzy_Showcase");
        Undo.RegisterCreatedObjectUndo(parent, "Place Factory Frenzy prefabs");
        float x = 0f;
        foreach (var n in names)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{n}.prefab");
            if (prefab == null) { Debug.LogWarning($"Prefab {n} not found. Run 'Factory Frenzy > Build Prefabs' first."); continue; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            go.transform.localPosition = new Vector3(x, 0f, 0f);
            x += 9f;
        }
        Selection.activeGameObject = parent;
    }

    // -------------------------------------------------------------- platforms
    // Every platform = rounded "Rim" body (with the collider) + inset "Top" plate + bolts + dark "Under" block.
    // The walkable top stays at y = height / 2 (+0.01), like before, so JSON positions don't change.

    // PC-PLA-P2 – green platform, white line, start gate with 3 countdown lights (Light1..3)
    static void BuildStart()
    {
        var root = new GameObject("Start");
        Plate(root.transform, "Platform", 6f, 0.6f, 5f, Mat("FF_Start", "#3DBE6B"), Mat("FF_StartRim", "#2A8F4F"));
        Box("StartLine", root.transform, new Vector3(0f, 0.325f, 1.4f), new Vector3(5.4f, 0.03f, 0.3f), Mat("FF_White", "#FFFFFF"), false);

        var gate = Empty("Gate", root.transform, new Vector3(0f, 0f, 1.9f));
        var post = Mat("FF_Post", "#2B3142", smooth: 0.5f);
        RBox("PostL", gate, new Vector3(-2.75f, 1.6f, 0f), new Vector3(0.3f, 2.6f, 0.3f), 0.1f, post);
        RBox("PostR", gate, new Vector3(2.75f, 1.6f, 0f), new Vector3(0.3f, 2.6f, 0.3f), 0.1f, post);
        RBox("Bar", gate, new Vector3(0f, 3.0f, 0f), new Vector3(5.8f, 0.5f, 0.4f), 0.18f, post);

        // countdown lights: swap to FF_LightRed on 3-2-1, all FF_LightGreen on GO
        Mat("FF_LightRed", "#E5484D", emissionHex: "#E5484D", emissionIntensity: 1.6f);
        Mat("FF_LightGreen", "#3DBE6B", emissionHex: "#3DBE6B", emissionIntensity: 1.6f);
        var off = Mat("FF_LightOff", "#4A5163", smooth: 0.8f);
        for (int i = 0; i < 3; i++)
            Prim(PrimitiveType.Sphere, $"Light{i + 1}", gate, new Vector3(-0.8f + i * 0.8f, 3.0f, -0.22f), Vector3.one * 0.34f, off, false);

        Empty("SpawnPoint", root.transform, new Vector3(0f, 0.31f, -1.2f));
        Save(root);
    }

    // PC-PLA-P3 – blue platform, checkered line, banner arch with glowing orbs, finish trigger
    static void BuildFinish()
    {
        var root = new GameObject("Finish");
        Plate(root.transform, "Platform", 6f, 0.6f, 5f, Mat("FF_Finish", "#3D7BE0"), Mat("FF_FinishRim", "#2A5BB0"));

        var checkers = Empty("Checkers", root.transform, new Vector3(0f, 0.325f, -1f));
        var white = Mat("FF_White", "#FFFFFF");
        for (int i = 0; i < 11; i++)
            for (int j = 0; j < 2; j++)
            {
                if ((i + j) % 2 == 1) continue;
                Box($"Checker_{i}_{j}", checkers, new Vector3(-2.5f + i * 0.5f, 0f, -0.125f + j * 0.25f), new Vector3(0.5f, 0.03f, 0.25f), white, false);
            }

        var arch = Empty("Arch", root.transform, new Vector3(0f, 0f, -1f));
        var archMat = Mat("FF_FinishArch", "#3D7BE0", emissionHex: "#10285A");
        RBox("PillarL", arch, new Vector3(-2.8f, 1.95f, 0f), new Vector3(0.5f, 3.3f, 0.5f), 0.18f, archMat);
        RBox("PillarR", arch, new Vector3(2.8f, 1.95f, 0f), new Vector3(0.5f, 3.3f, 0.5f), 0.18f, archMat);
        RBox("Banner", arch, new Vector3(0f, 3.75f, 0f), new Vector3(6.2f, 0.8f, 0.36f), 0.2f, Mat("FF_Banner", "#FFFFFF"));
        RBox("BannerStripe", arch, new Vector3(0f, 3.75f, -0.19f), new Vector3(5.6f, 0.3f, 0.02f), 0.01f, archMat, false);
        var orb = Mat("FF_Orb", "#FFE066", emissionHex: "#FFB000", emissionIntensity: 1.5f);
        Prim(PrimitiveType.Sphere, "OrbL", arch, new Vector3(-2.8f, 3.85f, 0f), Vector3.one * 0.55f, orb, false);
        Prim(PrimitiveType.Sphere, "OrbR", arch, new Vector3(2.8f, 3.85f, 0f), Vector3.one * 0.55f, orb, false);

        Trigger("FinishLine", root.transform, new Vector3(0f, 1.8f, -1f), new Vector3(5.2f, 3f, 0.5f));
        Save(root);
    }

    // PC-PLA-P4 – steel top with a yellow safety rim
    static void BuildClassic()
    {
        var root = new GameObject("Classic");
        Plate(root.transform, "Platform", 4f, 0.6f, 4f, Mat("FF_Classic", "#9AA3B5"), Mat("FF_Hazard", "#F2C14E"));
        Save(root);
    }

    // PC-PLA-P5 – purple platform with arrows showing it moves (along local X by default)
    static void BuildMoving()
    {
        var root = new GameObject("Moving");
        Plate(root.transform, "Platform", 3.5f, 0.5f, 3.5f, Mat("FF_Moving", "#B5A6E6"), Mat("FF_MovingRim", "#7E68C9"));
        var arrowMat = Mat("FF_Arrow", "#F3EEFF", emissionHex: "#8E7BE0", emissionIntensity: 0.6f);
        var arrows = Empty("Arrows", root.transform, new Vector3(0f, 0.275f, 0f));
        foreach (var side in new[] { -1f, 1f })
            for (int k = 0; k < 2; k++)
            {
                var a = Box($"Arrow_{side}_{k}", arrows, new Vector3(side * 1.05f, 0f, (k == 0 ? 0.2f : -0.2f)), new Vector3(0.55f, 0.03f, 0.13f), arrowMat, false);
                a.transform.localEulerAngles = new Vector3(0f, (k == 0 ? -45f : 45f) * side, 0f);
            }
        Save(root);
    }

    // PC-PLA-P6 – dark base, springs, frame ring and a pink pad
    static void BuildTrampoline()
    {
        var root = new GameObject("Trampoline");
        RBox("Base", root.transform, Vector3.zero, new Vector3(3f, 0.2f, 3f), 0.08f, Mat("FF_TrampolineBase", "#4A5163"));
        var dark = Mat("FF_Post", "#2B3142", smooth: 0.5f);
        var spring = Mat("FF_Spring", "#C9D1DE", smooth: 0.8f, metal: 0.7f);
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4f;
            Cyl($"Spring{i}", root.transform, new Vector3(Mathf.Cos(a) * 1.3f, 0.25f, Mathf.Sin(a) * 1.3f), 0.06f, 0.3f, spring, false);
        }
        MeshObj("Frame", root.transform, Torus("FF_Torus_Trampoline", 1.32f, 0.1f), new Vector3(0f, 0.4f, 0f), new Vector3(90f, 0f, 0f), dark);
        Cyl("Pad", root.transform, new Vector3(0f, 0.38f, 0f), 1.25f, 0.1f, Mat("FF_TrampolinePad", "#FF8CC6"));
        Trigger("BounceTrigger", root.transform, new Vector3(0f, 0.6f, 0f), new Vector3(2.4f, 0.4f, 2.4f));
        Save(root);
    }

    // PC-PLA-P7 – platform + arch + flag. Swap the Arch renderers to FF_CheckpointOn when activated.
    static void BuildCheckpoint()
    {
        var root = new GameObject("Checkpoint");
        Plate(root.transform, "Platform", 4f, 0.6f, 4f, Mat("FF_Classic", "#9AA3B5"), Mat("FF_Hazard", "#F2C14E"));

        var off = Mat("FF_CheckpointOff", "#5B6375");
        Mat("FF_CheckpointOn", "#3DBE6B", emissionHex: "#1F7A42", emissionIntensity: 1.5f);

        var arch = Empty("Arch", root.transform, Vector3.zero);
        RBox("PostL", arch, new Vector3(-1.9f, 1.6f, 0f), new Vector3(0.3f, 2.6f, 0.3f), 0.1f, off);
        RBox("PostR", arch, new Vector3(1.9f, 1.6f, 0f), new Vector3(0.3f, 2.6f, 0.3f), 0.1f, off);
        RBox("Bar", arch, new Vector3(0f, 3.0f, 0f), new Vector3(4.1f, 0.3f, 0.3f), 0.1f, off);
        Cyl("FlagPole", root.transform, new Vector3(1.9f, 3.7f, 0f), 0.04f, 1.2f, Mat("FF_Bolt", "#C9D1DE", smooth: 0.7f, metal: 0.6f), false);
        RBox("Flag", arch, new Vector3(2.27f, 4.05f, 0f), new Vector3(0.7f, 0.45f, 0.04f), 0.02f, off, false);

        Trigger("CheckpointTrigger", root.transform, new Vector3(0f, 1.6f, 0f), new Vector3(3.5f, 2.6f, 0.6f));
        Empty("RespawnPoint", root.transform, new Vector3(0f, 0.31f, 0.8f));
        Save(root);
    }

    // ------------------------------------------------------------------ traps

    // PC-TRA-T1 – dark base disk, pink body, white ring, glowing cap. Pivot at the bottom.
    static void BuildBumper()
    {
        var root = new GameObject("Bumper");
        Cyl("Base", root.transform, new Vector3(0f, 0.06f, 0f), 1.0f, 0.12f, Mat("FF_Post", "#2B3142", smooth: 0.5f), false);
        Cyl("Body", root.transform, new Vector3(0f, 0.62f, 0f), 0.8f, 1.0f, Mat("FF_Trap", "#EF5DA8", emissionHex: "#5A1036"));
        MeshObj("Ring", root.transform, Torus("FF_Torus_Bumper", 0.86f, 0.08f), new Vector3(0f, 0.62f, 0f), new Vector3(90f, 0f, 0f), Mat("FF_White", "#FFFFFF"));
        Prim(PrimitiveType.Sphere, "Cap", root.transform, new Vector3(0f, 1.12f, 0f), new Vector3(1.5f, 0.45f, 1.5f),
            Mat("FF_TrapGlow", "#FF8CC6", emissionHex: "#EF5DA8", emissionIntensity: 0.8f), false);
        Save(root);
    }

    // PC-TRA-T2 – striped pillar, rotating Head (rounded turret, barrel, muzzle ring, sensor Eye), detection sphere r = 10
    static void BuildLauncher()
    {
        var root = new GameObject("Launcher");
        Cyl("Pillar", root.transform, new Vector3(0f, 2f, 0f), 0.5f, 4f, Mat("FF_Pillar", "#3B4256"));
        var hazard = Mat("FF_Hazard", "#F2C14E");
        Cyl("Stripe1", root.transform, new Vector3(0f, 1.0f, 0f), 0.53f, 0.18f, hazard, false);
        Cyl("Stripe2", root.transform, new Vector3(0f, 3.0f, 0f), 0.53f, 0.18f, hazard, false);

        var head = Empty("Head", root.transform, new Vector3(0f, 4f, 0f)); // rotate this around Y
        Cyl("HeadBase", head, new Vector3(0f, 0.18f, 0f), 0.8f, 0.35f, Mat("FF_HeadBase", "#2B3142"));
        RBox("Turret", head, new Vector3(0f, 0.78f, 0f), new Vector3(1.2f, 0.85f, 1.3f), 0.22f, Mat("FF_Trap", "#EF5DA8", emissionHex: "#5A1036"));
        var barrel = Cyl("Barrel", head, new Vector3(0f, 0.78f, 1.1f), 0.2f, 1.3f, Mat("FF_Barrel", "#1D2130"), false);
        barrel.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        MeshObj("MuzzleRing", head, Torus("FF_Torus_Muzzle", 0.24f, 0.06f), new Vector3(0f, 0.78f, 1.75f), Vector3.zero, hazard);
        // sensor eye: change its material to show the state (search = yellow, orient = orange, fire = red)
        Prim(PrimitiveType.Sphere, "Eye", head, new Vector3(0f, 1.1f, 0.62f), Vector3.one * 0.26f,
            Mat("FF_Eye", "#E5484D", emissionHex: "#E5484D", emissionIntensity: 2f), false);
        Empty("Muzzle", head, new Vector3(0f, 0.78f, 1.85f)); // spawn projectiles here, forward = Head.forward

        var detection = new GameObject("DetectionZone");
        detection.transform.SetParent(root.transform, false);
        detection.transform.localPosition = new Vector3(0f, 4f, 0f);
        var sphere = detection.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 10f;
        Save(root);
    }

    // Launcher projectile – yellow glowing ball, trigger + kinematic rigidbody (move it from a script)
    static void BuildProjectile()
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        root.name = "Projectile";
        root.transform.localScale = Vector3.one * 0.56f;
        root.GetComponent<MeshRenderer>().sharedMaterial = Mat("FF_Projectile", "#FFE066", emissionHex: "#FFB000", emissionIntensity: 2f);
        root.GetComponent<SphereCollider>().isTrigger = true;
        var rb = root.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        Save(root);
    }

    // PC-TRA-T3 – pole + pink ring + cross guard + hub + 3 rounded blades + wind trigger + particles. Blows along +Z.
    static void BuildFan()
    {
        var root = new GameObject("Fan");
        Cyl("Pole", root.transform, new Vector3(0f, 0.9f, 0f), 0.2f, 1.8f, Mat("FF_Pillar", "#3B4256"));
        Cyl("Foot", root.transform, new Vector3(0f, 0.05f, 0f), 0.5f, 0.1f, Mat("FF_Post", "#2B3142", smooth: 0.5f), false);

        var hub = Empty("Hub", root.transform, new Vector3(0f, 1.8f, 0f));
        MeshObj("Ring", hub, Torus("FF_Torus_Fan", 1.05f, 0.12f), Vector3.zero, Vector3.zero, Mat("FF_Trap", "#EF5DA8", emissionHex: "#5A1036"));
        var guardMat = Mat("FF_Bolt", "#C9D1DE", smooth: 0.7f, metal: 0.6f);
        foreach (var a in new[] { 45f, -45f, 0f, 90f })
        {
            var bar = Cyl($"Guard_{a}", hub, new Vector3(0f, 0f, 0.16f), 0.025f, 2.1f, guardMat, false);
            bar.transform.localEulerAngles = new Vector3(0f, 0f, a);
        }
        Prim(PrimitiveType.Sphere, "HubCap", hub, new Vector3(0f, 0f, 0.05f), new Vector3(0.36f, 0.36f, 0.42f), Mat("FF_White", "#FFFFFF"), false);

        var blades = Empty("Blades", hub, Vector3.zero); // rotate this around Z
        var bladeMat = Mat("FF_Blade", "#E8ECF4");
        for (int i = 0; i < 3; i++)
        {
            var pivot = Empty($"BladePivot_{i}", blades, Vector3.zero);
            pivot.localEulerAngles = new Vector3(0f, 0f, i * 120f);
            var bl = RBox("Blade", pivot, new Vector3(0f, 0.5f, 0f), new Vector3(0.34f, 0.85f, 0.06f), 0.03f, bladeMat, false);
            bl.transform.localEulerAngles = new Vector3(0f, 20f, 0f); // pitch, like a real fan
        }

        Trigger("WindZone", hub, new Vector3(0f, 0f, 3.2f), new Vector3(2.2f, 2.2f, 6f));

        var wind = new GameObject("WindParticles");
        wind.transform.SetParent(hub, false);
        var ps = wind.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.9f;
        main.startSpeed = 7f;
        main.startSize = 0.08f;
        main.startColor = Hex("#CFF4FF");
        main.maxParticles = 400;
        var emission = ps.emission;
        emission.rateOverTime = 150f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1.8f, 1.8f, 0.1f);
        wind.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMat("FF_Wind", "#CFF4FF");

        Save(root);
    }

    // PC-PLA-P4 – big invisible trigger under the level. Turn on DebugView's renderer to see it.
    static void BuildKillZone()
    {
        var root = new GameObject("KillZone");
        var bc = root.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(200f, 1f, 200f);
        var view = Box("DebugView", root.transform, Vector3.zero, new Vector3(200f, 0.05f, 200f), Mat("FF_KillZone", "#E5484D"), false);
        view.GetComponent<MeshRenderer>().enabled = false;
        Save(root);
    }

    // PC-PLA-P1 – "bean" character (no colliders). Child of the Starter Assets PlayerArmature; hide the robot's Geometry.
    // Part names matter: BeanAnimator finds them by name.
    static void BuildPlayerBean()
    {
        var root = new GameObject("PlayerBean");
        var visual = Empty("Visual", root.transform, Vector3.zero); // squash / lean / tumble pivot (at the feet)

        var body = Mat("FF_Player", "#FFB547", smooth: 0.55f);
        var limb = Mat("FF_PlayerLimb", "#F29A2E", smooth: 0.5f);
        var face = Mat("FF_PlayerFace", "#FFE9C7", smooth: 0.4f);
        var eye = Mat("FF_PlayerEye", "#1B2233", smooth: 0.9f, metal: 0.2f);
        var white = Mat("FF_White", "#FFFFFF");

        // body: radius 0.42, height 1.45, standing on the legs
        Prim(PrimitiveType.Capsule, "Body", visual, new Vector3(0f, 1.025f, 0f), new Vector3(0.84f, 0.725f, 0.84f), body, false);

        // face window + eyes (eyes are scaled on Y to blink)
        Prim(PrimitiveType.Sphere, "Face", visual, new Vector3(0f, 1.22f, 0.31f), new Vector3(0.62f, 0.44f, 0.32f), face, false);
        foreach (var side in new[] { -1f, 1f })
        {
            var e = Prim(PrimitiveType.Sphere, side < 0 ? "EyeL" : "EyeR", visual, new Vector3(0.11f * side, 1.25f, 0.465f), new Vector3(0.09f, 0.13f, 0.06f), eye, false);
            Prim(PrimitiveType.Sphere, "Shine", e.transform, new Vector3(0.18f, 0.22f, 0.45f), new Vector3(0.35f, 0.25f, 0.3f), white, false);
        }

        // arms: pivot at the shoulder, arm hangs down
        foreach (var side in new[] { -1f, 1f })
        {
            var pivot = Empty(side < 0 ? "ArmL" : "ArmR", visual, new Vector3(0.43f * side, 1.0f, 0f));
            Prim(PrimitiveType.Capsule, "Arm", pivot, new Vector3(0f, -0.2f, 0f), new Vector3(0.18f, 0.22f, 0.18f), limb, false);
        }

        // legs: pivot at the hip, foot touches the ground (y = 0)
        foreach (var side in new[] { -1f, 1f })
        {
            var pivot = Empty(side < 0 ? "LegL" : "LegR", visual, new Vector3(0.17f * side, 0.36f, 0f));
            Prim(PrimitiveType.Capsule, "Leg", pivot, new Vector3(0f, -0.17f, 0f), new Vector3(0.22f, 0.19f, 0.22f), limb, false);
        }

        Save(root);
    }

    // ---------------------------------------------------------------- helpers

    // Platform look: rounded rim body (collider) + inset top plate + 4 bolts + dark underside block
    static Transform Plate(Transform parent, string name, float w, float h, float d, Material top, Material rim)
    {
        var p = Empty(name, parent, Vector3.zero);
        RBox("Rim", p, Vector3.zero, new Vector3(w, h, d), 0.12f, rim, true);
        RBox("Top", p, new Vector3(0f, h / 2f - 0.02f, 0f), new Vector3(w - 0.24f, 0.06f, d - 0.24f), 0.03f, top, false);
        RBox("Under", p, new Vector3(0f, -h / 2f - 0.1f, 0f), new Vector3(w * 0.62f, 0.3f, d * 0.62f), 0.1f, Mat("FF_Under", "#2B3142"), false);
        var bolt = Mat("FF_Bolt", "#C9D1DE", smooth: 0.7f, metal: 0.6f);
        foreach (var sx in new[] { -1f, 1f })
            foreach (var sz in new[] { -1f, 1f })
                Cyl("Bolt", p, new Vector3(sx * (w / 2f - 0.32f), h / 2f + 0.02f, sz * (d / 2f - 0.32f)), 0.07f, 0.04f, bolt, false);
        return p;
    }

    static GameObject RBox(string name, Transform parent, Vector3 pos, Vector3 size, float radius, Material mat, bool collider = true)
    {
        var go = MeshObj(name, parent, RoundedBox(size, radius), pos, Vector3.zero, mat);
        if (collider) go.AddComponent<BoxCollider>().size = size;
        return go;
    }

    // Rounded box mesh (soft edges). Saved once per size in Meshes/.
    static Mesh RoundedBox(Vector3 size, float radius, int seg = 4)
    {
        string key = ("RB_" + size.x.ToString("0.###") + "x" + size.y.ToString("0.###") + "x" + size.z.ToString("0.###") + "_r" + radius.ToString("0.###")).Replace(',', '.');
        string path = $"{MeshDir}/{key}.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh; // already built this size
        mesh = new Mesh { name = key };

        Vector3 half = size * 0.5f;
        radius = Mathf.Min(radius, Mathf.Min(half.x, Mathf.Min(half.y, half.z)));
        Vector3 inner = half - Vector3.one * radius;

        Vector3[] N = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        Vector3[] U = { Vector3.up, Vector3.forward, Vector3.forward, Vector3.right, Vector3.right, Vector3.up };
        Vector3[] V = { Vector3.forward, Vector3.up, Vector3.right, Vector3.forward, Vector3.up, Vector3.right };

        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();

        for (int f = 0; f < 6; f++)
        {
            float hn = Mathf.Abs(Vector3.Dot(N[f], half));
            float[] cu = EdgeCoords(Mathf.Abs(Vector3.Dot(U[f], half)), radius, seg);
            float[] cv = EdgeCoords(Mathf.Abs(Vector3.Dot(V[f], half)), radius, seg);
            int nu = cu.Length, start = verts.Count;
            for (int j = 0; j < cv.Length; j++)
                for (int i = 0; i < nu; i++)
                {
                    Vector3 p = N[f] * hn + U[f] * cu[i] + V[f] * cv[j];
                    Vector3 c = new Vector3(Mathf.Clamp(p.x, -inner.x, inner.x), Mathf.Clamp(p.y, -inner.y, inner.y), Mathf.Clamp(p.z, -inner.z, inner.z));
                    Vector3 n = p - c;
                    n = n.sqrMagnitude > 1e-10f ? n.normalized : N[f];
                    verts.Add(c + n * radius);
                    norms.Add(n);
                    uvs.Add(new Vector2(i / (float)(nu - 1), j / (float)(cv.Length - 1)));
                }
            for (int j = 0; j < cv.Length - 1; j++)
                for (int i = 0; i < nu - 1; i++)
                {
                    int a = start + j * nu + i;
                    tris.Add(a); tris.Add(a + 1); tris.Add(a + nu);
                    tris.Add(a + 1); tris.Add(a + nu + 1); tris.Add(a + nu);
                }
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    // coordinates along one face axis: dense near the rounded edges, flat in the middle
    static float[] EdgeCoords(float half, float r, int seg)
    {
        var list = new List<float>();
        for (int k = 0; k <= seg; k++) list.Add(-half + r - r * Mathf.Cos(k / (float)seg * Mathf.PI * 0.5f));
        for (int k = seg; k >= 0; k--) list.Add(half - r + r * Mathf.Cos(k / (float)seg * Mathf.PI * 0.5f));
        return list.ToArray();
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "FactoryFrenzyGen");
        foreach (var sub in new[] { "Materials", "Meshes", "Prefabs" })
            if (!AssetDatabase.IsValidFolder($"{Root}/{sub}")) AssetDatabase.CreateFolder(Root, sub);
    }

    static void Save(GameObject root)
    {
        PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/{root.name}.prefab");
        Object.DestroyImmediate(root);
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    static Material Mat(string name, string hex, float smooth = 0.35f, float metal = 0.1f, string emissionHex = null, float emissionIntensity = 1f)
    {
        string path = $"{MatDir}/{name}.mat";
        Shader shader = IsURP ? Shader.Find("Universal Render Pipeline/Lit") : null;
        if (shader == null) shader = Shader.Find("Standard");

        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
        else m.shader = shader;

        var c = Hex(hex);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal);

        if (emissionHex != null)
        {
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", Hex(emissionHex) * emissionIntensity);
        }
        else
        {
            m.DisableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", Color.black);
        }
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material ParticleMat(string name, string hex)
    {
        string path = $"{MatDir}/{name}.mat";
        Shader shader = IsURP ? Shader.Find("Universal Render Pipeline/Particles/Unlit") : Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
        else m.shader = shader;

        var c = Hex(hex);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        EditorUtility.SetDirty(m);
        return m;
    }

    static Transform Empty(string name, Transform parent, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go.transform;
    }

    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool keepCollider = true)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, bool collider = true)
        => Prim(PrimitiveType.Cube, name, parent, pos, size, mat, collider);

    // Unity's cylinder is 2 high with radius 0.5 at scale 1, so we convert radius/height to a scale.
    static GameObject Cyl(string name, Transform parent, Vector3 pos, float radius, float height, Material mat, bool collider = true)
    {
        var go = Prim(PrimitiveType.Cylinder, name, parent, pos, new Vector3(radius * 2f, height / 2f, radius * 2f), mat, false);
        if (collider)
        {
            var mc = go.AddComponent<MeshCollider>();
            mc.convex = true;
        }
        return go;
    }

    static GameObject MeshObj(string name, Transform parent, Mesh mesh, Vector3 pos, Vector3 euler, Material mat)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localEulerAngles = euler;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    static BoxCollider Trigger(string name, Transform parent, Vector3 center, Vector3 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var bc = go.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.center = center;
        bc.size = size;
        return bc;
    }

    // Torus lying in the XY plane (its hole faces +Z). R = ring radius, r = tube radius.
    static Mesh Torus(string name, float R, float r, int segments = 48, int sides = 12)
    {
        string path = $"{MeshDir}/{name}.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        bool isNew = mesh == null;
        if (isNew) mesh = new Mesh();
        mesh.Clear();
        mesh.name = name;

        var verts = new Vector3[(segments + 1) * (sides + 1)];
        var normals = new Vector3[verts.Length];
        var uvs = new Vector2[verts.Length];
        int v = 0;
        for (int s = 0; s <= segments; s++)
        {
            float a = s / (float)segments * Mathf.PI * 2f;
            var dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
            for (int k = 0; k <= sides; k++)
            {
                float b = k / (float)sides * Mathf.PI * 2f;
                var n = dir * Mathf.Cos(b) + Vector3.forward * Mathf.Sin(b);
                verts[v] = dir * R + n * r;
                normals[v] = n;
                uvs[v] = new Vector2(s / (float)segments, k / (float)sides);
                v++;
            }
        }

        var tris = new int[segments * sides * 6];
        int t = 0;
        for (int s = 0; s < segments; s++)
            for (int k = 0; k < sides; k++)
            {
                int a0 = s * (sides + 1) + k;
                int a1 = a0 + sides + 1;
                tris[t++] = a0; tris[t++] = a1; tris[t++] = a0 + 1;
                tris[t++] = a0 + 1; tris[t++] = a1; tris[t++] = a1 + 1;
            }

        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();

        if (isNew) AssetDatabase.CreateAsset(mesh, path);
        else EditorUtility.SetDirty(mesh);
        return mesh;
    }
}
#endif