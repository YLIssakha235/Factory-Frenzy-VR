// Factory Frenzy – builds the race UI (HUD + end screen) from the UI kit sprites.
// Put this file in Assets/Editor/. Then open your level scene and click:  Factory Frenzy > Build Race UI
// It is an editor TOOL (like the prefab builder): it does not run in the game.
//
// What it does:
//  1. sets the sprites in Assets/UI/Sprites to "Sprite" mode with the 9-slice borders
//  2. creates a Canvas "RaceUI" laid out like the mockups
//  3. links everything to the RaceManager (creates it if needed) and the Restart / Menu buttons

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class RaceUIBuilder
{
    const string SpriteFolder = "Assets/UI/Sprites/";

    static readonly Color Yellow = new Color(1f, 0.79f, 0.24f);    // #FFC93C
    static readonly Color Grey = new Color(0.56f, 0.6f, 0.69f);    // #8F9AB0

    static TMP_FontAsset font;

    [MenuItem("Factory Frenzy/Build Race UI")]
    public static void Build()
    {
        PrepareSprites();
        font = FindFont();

        // remove an old version of the UI
        GameObject old = GameObject.Find("RaceUI");
        if (old != null) Object.DestroyImmediate(old);

        // ---------- Canvas ----------
        GameObject canvasGO = new GameObject("RaceUI", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        Transform root = canvasGO.transform;
        MakeEventSystem();

        Vector2 topLeft = new Vector2(0, 1), topCenter = new Vector2(0.5f, 1), topRight = new Vector2(1, 1), center = new Vector2(0.5f, 0.5f);

        // ---------- HUD ----------
        GameObject levelPill = MakeImage("LevelPill", root, "pill", topLeft, new Vector2(350, -84), new Vector2(620, 96));
        MakeImage("FlagIcon", levelPill.transform, "icon_flag", center, new Vector2(-255, 0), new Vector2(52, 52));
        MakeText("LevelText", levelPill.transform, "LEVEL 1", 40, Color.white, center, new Vector2(30, 0), new Vector2(520, 80));

        GameObject timerPill = MakeImage("TimerPill", root, "pill", topCenter, new Vector2(0, -90), new Vector2(360, 120));
        MakeImage("ClockIcon", timerPill.transform, "icon_clock", center, new Vector2(-120, 0), new Vector2(60, 60));
        TextMeshProUGUI timerText = MakeText("TimerText", timerPill.transform, "00:00.0", 64, Color.white, center, new Vector2(30, 0), new Vector2(260, 100));

        GameObject bestPill = MakeImage("BestPill", root, "pill", topRight, new Vector2(-240, -84), new Vector2(400, 96));
        GameObject star = MakeImage("StarIcon", bestPill.transform, "icon_star", center, new Vector2(-150, 0), new Vector2(48, 48));
        star.GetComponent<Image>().color = Yellow;
        TextMeshProUGUI bestText = MakeText("BestText", bestPill.transform, "BEST --:--", 38, Yellow, center, new Vector2(25, 0), new Vector2(320, 80));

        GameObject getReady = MakeImage("GetReadyBanner", root, "banner_pink", topCenter, new Vector2(0, -210), new Vector2(600, 120));
        MakeText("GetReadyText", getReady.transform, "GET READY!", 60, Color.white, center, new Vector2(0, 6), new Vector2(560, 100));

        TextMeshProUGUI countdownText = MakeText("CountdownText", root, "3", 400, Yellow, center, new Vector2(0, 60), new Vector2(900, 450));

        // ---------- End screen ----------
        GameObject endPanel = MakeImage("EndPanel", root, null, center, Vector2.zero, Vector2.zero);
        RectTransform endRect = endPanel.GetComponent<RectTransform>();
        endRect.anchorMin = Vector2.zero; endRect.anchorMax = Vector2.one; // full screen
        endRect.offsetMin = Vector2.zero; endRect.offsetMax = Vector2.zero;
        endPanel.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.09f, 0.55f);

        Transform end = endPanel.transform;
        MakeImage("Panel", end, "panel", center, new Vector2(0, -20), new Vector2(800, 780));
        GameObject banner = MakeImage("TitleBanner", end, "banner_pink", center, new Vector2(0, 370), new Vector2(600, 130));
        MakeText("TitleText", banner.transform, "FINISHED!", 72, Color.white, center, new Vector2(0, 6), new Vector2(560, 110));
        MakeImage("Medal", end, "medal_gold", center, new Vector2(0, 240), new Vector2(140, 160));
        MakeText("YourTimeLabel", end, "YOUR TIME", 28, Grey, center, new Vector2(0, 130), new Vector2(600, 40));
        TextMeshProUGUI resultText = MakeText("ResultText", end, "00:00.0", 128, Color.white, center, new Vector2(0, -20), new Vector2(700, 260));
        Button restart = MakeButton("RestartButton", end, "button_yellow", "RESTART", new Vector2(-155, -310));
        Button menu = MakeButton("MenuButton", end, "button_blue", "MENU", new Vector2(155, -310));

        // ---------- Link to the RaceManager ----------
        RaceManager race = Object.FindObjectOfType<RaceManager>();
        if (race == null)
        {
            race = new GameObject("RaceManager").AddComponent<RaceManager>();
        }
        race.countdownText = countdownText;
        race.timerText = timerText;
        race.endPanel = endPanel;
        race.resultText = resultText;
        race.bestTimeText = bestText;
        race.getReadyBanner = getReady;
        if (race.player == null) race.player = GameObject.Find("PlayerArmature");

        UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(race.Restart));
        UnityEventTools.AddPersistentListener(menu.onClick, new UnityAction(race.GoToMenu));

        EditorUtility.SetDirty(race);
        EditorSceneManager.MarkSceneDirty(race.gameObject.scene);
        Selection.activeGameObject = canvasGO;

        string playerInfo = race.player != null ? "Player linked: " + race.player.name : "Drag PlayerArmature into RaceManager > Player.";
        EditorUtility.DisplayDialog("Factory Frenzy", "Race UI created and linked to RaceManager.\n" + playerInfo + "\n\nSave the scene (Ctrl+S).", "OK");
    }

    // ---------------------------------------------------------------- helpers

    // Sprite mode + 9-slice borders for all the UI sprites
    static void PrepareSprites()
    {
        string[] sliced = { "button_yellow", "button_blue", "button_pink", "button_green", "button_grey", "banner_pink", "pill", "panel" };
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/UI/Sprites" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;

            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (System.Array.IndexOf(sliced, fileName) >= 0)
            {
                float b = fileName == "panel" ? 44 : 40;
                importer.spriteBorder = new Vector4(b, b, b, b); // left, bottom, right, top
            }
            importer.SaveAndReimport();
        }
    }

    // uses the Lilita One font if you made it, otherwise the TextMeshPro default font
    static TMP_FontAsset FindFont()
    {
        string[] guids = AssetDatabase.FindAssets("Lilita t:TMP_FontAsset");
        if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return TMP_Settings.defaultFontAsset;
    }

    static void MakeEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        // Starter Assets uses the new Input System: use its UI module if it exists
        System.Type inputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModule != null) es.AddComponent(inputModule);
        else es.AddComponent<StandaloneInputModule>();
    }

    static RectTransform Place(GameObject go, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static GameObject MakeImage(string name, Transform parent, string spriteName, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Place(go, parent, anchor, position, size);
        Image image = go.AddComponent<Image>();
        if (spriteName != null)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + spriteName + ".png");
            if (sprite == null) Debug.LogWarning("Race UI: sprite not found: " + SpriteFolder + spriteName + ".png");
            image.sprite = sprite;
            if (sprite != null && sprite.border != Vector4.zero) image.type = Image.Type.Sliced;
        }
        return go;
    }

    static TextMeshProUGUI MakeText(string name, Transform parent, string text, float fontSize, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Place(go, parent, anchor, position, size);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button MakeButton(string name, Transform parent, string spriteName, string label, Vector2 position)
    {
        GameObject go = MakeImage(name, parent, spriteName, new Vector2(0.5f, 0.5f), position, new Vector2(290, 120));
        Button button = go.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.94f, 0.94f, 0.94f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f);
        button.colors = colors;
        MakeText("Text", go.transform, label, 56, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0, 6), new Vector2(270, 100));
        return button;
    }
}
#endif
