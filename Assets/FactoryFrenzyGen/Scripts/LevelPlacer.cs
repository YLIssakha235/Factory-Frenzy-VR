// Factory Frenzy - places Level 1, 2 or 3 directly in the open scene (no JSON).
// Put this file in Assets/Editor/. Menu: Factory Frenzy > Place Level > Level 1 / 2 / 3
// After that, the objects are normal objects in your scene: you can move them, delete them, save the scene.
//
// Each line is:  Place("PrefabName", x, y, z)   or   Place("PrefabName", x, y, z, scaleX, scaleZ, rotationY)

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LevelPlacer
{
    const string PrefabFolder = "Assets/FactoryFrenzyGen/Prefabs/";

    static Transform level;      // parent object "Level" in the scene
    static float lowestY;        // to put the kill zone under the level
    static Transform startPlatform;

    // ---------------------------------------------------------------- LEVEL 1
    [MenuItem("Factory Frenzy/Place Level/Level 1 - Tutorial Factory")]
    static void Level1()
    {
        Begin("Level 1");
        Place("Start", 0, 0, 0);
        Place("Classic", 0, 0, 7);
        Place("Classic", 0, 0, 12.5f, 0.75f, 0.75f, 0);
        Place("Classic", 0, 0, 18, 0.75f, 0.75f, 0);
        Place("Moving", -2.5f, 0, 24);   // moves to x=2.5 in 3 s
        Place("Classic", 0, 0, 30.5f, 1, 1.25f, 0);
        Place("Checkpoint", 0, 0, 37.5f);
        Place("Trampoline", 0, 0, 43);
        Place("Classic", 0, 4, 47.5f);
        Place("Classic", 0, 4, 54);
        Place("Finish", 0, 4, 61);
        End();
    }

    // ---------------------------------------------------------------- LEVEL 2
    [MenuItem("Factory Frenzy/Place Level/Level 2 - Bumper Alley")]
    static void Level2()
    {
        Begin("Level 2");
        Place("Start", 0, 0, 0);
        Place("Classic", 0, 0, 10, 0.8f, 3.5f, 0);
        Place("Bumper", -0.8f, 0.3f, 6);
        Place("Bumper", 0.8f, 0.3f, 10);
        Place("Bumper", -0.8f, 0.3f, 14);
        Place("Checkpoint", 0, 0, 21);
        Place("Classic", 0, 0, 31, 0.5f, 3, 0);
        Place("Classic", -4.5f, 0, 31, 0.5f, 0.5f, 0);
        Place("Fan", -4.5f, 0.3f, 31, 1, 1, 90);
        Place("Moving", -2, 0, 41.5f);   // moves to x=2 in 2.5 s
        Place("Moving", 2, 0, 47);   // moves to x=-2 in 2.5 s
        Place("Checkpoint", 0, 0, 53);
        Place("Finish", 0, 0, 60);
        End();
    }

    // ---------------------------------------------------------------- LEVEL 3
    [MenuItem("Factory Frenzy/Place Level/Level 3 - The Watchtower")]
    static void Level3()
    {
        Begin("Level 3");
        Place("Start", 0, 0, 0);
        Place("Classic", 0, 0, 7);
        Place("Checkpoint", 0, 0, 13);
        Place("Launcher", 0, -3, 25);
        Place("Classic", 3, 0, 18.5f, 0.75f, 0.75f, 0);
        Place("Classic", 6.5f, 0, 24, 0.75f, 0.75f, 0);
        Place("Classic", 4.7f, 1.3f, 24, 0.1f, 0.65f, 0, 3.33f);   // cover wall (thin, tall Classic)
        Place("Moving", 6.5f, 0, 30);   // moves to x=3.5 in 2.5 s
        Place("Classic", 1.5f, 0, 35, 0.75f, 0.75f, 0);
        Place("Classic", 1.5f, 1.3f, 33.7f, 0.65f, 0.1f, 0, 3.33f);   // cover wall (thin, tall Classic)
        Place("Checkpoint", -2.5f, 0, 40.5f);
        Place("Trampoline", -2.5f, 0, 46);
        Place("Finish", -2.5f, 4, 51.5f);
        End();
    }

    // ---------------------------------------------------------------- helpers

    // removes the old level and creates an empty "Level" object
    static void Begin(string name)
    {
        GameObject old = GameObject.Find("Level");
        if (old != null) Undo.DestroyObjectImmediate(old);

        level = new GameObject("Level").transform;
        Undo.RegisterCreatedObjectUndo(level.gameObject, "Place " + name);
        lowestY = 0;
        startPlatform = null;
    }

    // creates one prefab in the level
    static void Place(string prefabName, float x, float y, float z, float scaleX = 1, float scaleZ = 1, float rotationY = 0, float scaleY = 1)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + prefabName + ".prefab");
        if (prefab == null)
        {
            Debug.LogWarning("Place Level: prefab not found " + PrefabFolder + prefabName + ".prefab");
            return;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, level);
        go.transform.position = new Vector3(x, y, z);
        go.transform.rotation = Quaternion.Euler(0, rotationY, 0);
        go.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        // bigger checkpoint trigger, so the player always touches it
        if (prefabName == "Checkpoint")
        {
            Transform trigger = go.transform.Find("CheckpointTrigger");
            if (trigger != null && trigger.GetComponent<BoxCollider>() != null)
            {
                trigger.GetComponent<BoxCollider>().size = new Vector3(3.6f, 3f, 3.6f);
                trigger.GetComponent<BoxCollider>().center = new Vector3(0, 1.5f, 0);
            }
        }

        if (prefabName == "Start") startPlatform = go.transform;
        if (y < lowestY) lowestY = y;
    }

    // adds the kill zone, puts the player on the start, saves
    static void End()
    {
        // kill zone 8 m under the lowest object
        Place("KillZone", 0, lowestY - 8, 30, 1, 1, 0, 10);

        // put the player on the start platform
        GameObject player = GameObject.Find("PlayerArmature");
        if (player != null && startPlatform != null)
        {
            Undo.RecordObject(player.transform, "Move player");
            Transform spawn = startPlatform.Find("SpawnPoint");
            player.transform.position = spawn != null ? spawn.position + Vector3.up * 0.1f : startPlatform.position + Vector3.up;
        }

        EditorSceneManager.MarkSceneDirty(level.gameObject.scene);
        Selection.activeGameObject = level.gameObject;
        Debug.Log("Level placed: " + level.childCount + " objects. Save the scene (Ctrl+S).");
    }
}
#endif

