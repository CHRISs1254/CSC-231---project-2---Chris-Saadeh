using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

// EDITOR TOOL ONLY - not part of the game. Delete this file after using it.
// Unity top menu > Tools > Build Menus creates the HUD (souls + lives), the 4 menu panels and their buttons,
// connects every button to the GameManager, and fills in the GameManager's and SoulCounter's fields.
public static class MenuBuilder
{
    static readonly Color panelColor = new Color(0.05f, 0.02f, 0.08f, 0.85f); // dark purple, see-through
    static readonly Color buttonColor = new Color(0.30f, 0.10f, 0.35f, 1f);    // gothic purple
    static readonly Color titleColor = new Color(0.95f, 0.75f, 1f, 1f);       // light pink

    [MenuItem("Tools/Build Menus")]
    static void Build()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No Canvas in the scene."); return; }
        Transform root = canvas.transform;

        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player == null) { Debug.LogError("No Player with PlayerHealth in the scene."); return; }

        // Make the UI scale with the window so it looks the same in the build
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Undo.RecordObject(scaler, "Build Menus");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Delete the old panels so we start clean
        foreach (string n in new[] { "TitlePanel", "PausePanel", "GameOverPanel", "WinPanel", "Panel" })
        {
            Transform old = FindChild(root, n);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        }

        // ---------- HUD (stretched over the whole screen) ----------
        Transform hud = FindChild(root, "HUD");
        if (hud == null)
        {
            GameObject h = NewUI("HUD", root);
            hud = h.transform;
        }
        Undo.RecordObject(hud, "Build Menus");
        Stretch((RectTransform)hud);

        // Souls text (top-left)
        TMP_Text soul = FindText("SoulText");
        if (soul == null) soul = NewText("SoulText", hud, "Souls: 0 / 0", 40);
        else Undo.SetTransformParent(soul.transform, hud, "Build Menus");
        Undo.RecordObject(soul.rectTransform, "Build Menus");
        Place(soul.rectTransform, new Vector2(0, 1), new Vector2(40, -30), new Vector2(500, 60));
        soul.alignment = TextAlignmentOptions.Left;

        // Lives text (under the souls)
        TMP_Text lives = FindText("LivesText");
        if (lives == null) lives = NewText("LivesText", hud, "Lives: 3   HP: 3", 40);
        else Undo.SetTransformParent(lives.transform, hud, "Build Menus");
        Undo.RecordObject(lives.rectTransform, "Build Menus");
        Place(lives.rectTransform, new Vector2(0, 1), new Vector2(40, -90), new Vector2(500, 60));
        lives.alignment = TextAlignmentOptions.Left;

        // ---------- SoulCounter: exactly one, on the Canvas, using SoulText ----------
        foreach (SoulCounter sc in Object.FindObjectsByType<SoulCounter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (sc.gameObject != canvas.gameObject)
                Undo.DestroyObjectImmediate(sc);          // remove the extra ones (e.g. on the soul)

        SoulCounter counter = canvas.GetComponent<SoulCounter>();
        if (counter == null) counter = Undo.AddComponent<SoulCounter>(canvas.gameObject);
        Undo.RecordObject(counter, "Build Menus");
        counter.text = soul;
        EditorUtility.SetDirty(counter);

        // ---------- GameManager ----------
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject go = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(go, "Build Menus");
            gm = go.AddComponent<GameManager>();
        }

        // ---------- Panels + buttons ----------
        GameObject title = NewPanel("TitlePanel", root, Application.productName);
        AddButton(title, "Play", 0, gm.StartGame);
        AddButton(title, "Quit", -110, gm.QuitGame);

        GameObject pause = NewPanel("PausePanel", root, "Paused");
        AddButton(pause, "Resume", 0, gm.Resume);
        AddButton(pause, "Restart", -110, gm.Restart);
        AddButton(pause, "Quit", -220, gm.QuitToTitle);

        GameObject over = NewPanel("GameOverPanel", root, "Game Over");
        AddButton(over, "Try Again", 0, gm.Restart);
        AddButton(over, "Menu", -110, gm.QuitToTitle);

        GameObject win = NewPanel("WinPanel", root, "Level Complete!");
        AddButton(win, "Play Again", 0, gm.Restart);
        AddButton(win, "Menu", -110, gm.QuitToTitle);

        // Hide the panels in the editor so they don't cover the Scene view (GameManager shows them in play)
        title.SetActive(false); pause.SetActive(false); over.SetActive(false); win.SetActive(false);

        // Fill in the GameManager's fields
        Undo.RecordObject(gm, "Build Menus");
        gm.titlePanel = title;
        gm.pausePanel = pause;
        gm.gameOverPanel = over;
        gm.winPanel = win;
        gm.hud = hud.gameObject;
        gm.livesText = lives;
        gm.player = player;
        EditorUtility.SetDirty(gm);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("Menus built! Press Ctrl+S to save the scene.");
    }

    // ---------- Helpers ----------

    static GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Build Menus");
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    static TMP_Text NewText(string name, Transform parent, string text, float size)
    {
        TextMeshProUGUI t = NewUI(name, parent).AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }

    static GameObject NewPanel(string name, Transform parent, string titleText)
    {
        GameObject p = NewUI(name, parent);
        Stretch(p.GetComponent<RectTransform>());
        p.AddComponent<Image>().color = panelColor;

        TMP_Text t = NewText("TitleText", p.transform, titleText, 96);
        t.color = titleColor;
        t.fontStyle = FontStyles.Bold;
        Place(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 230), new Vector2(1600, 180));
        return p;
    }

    static void AddButton(GameObject panel, string label, float y, UnityAction action)
    {
        GameObject b = NewUI(label.Replace(" ", "") + "Button", panel.transform);
        Place(b.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(380, 85));
        b.AddComponent<Image>().color = buttonColor;
        Button btn = b.AddComponent<Button>();

        TMP_Text t = NewText("Text", b.transform, label, 46);
        Stretch(t.rectTransform);

        UnityEventTools.AddPersistentListener(btn.onClick, action);  // same as dragging GameManager into On Click
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static Transform FindChild(Transform parent, string name)
    {
        foreach (Transform c in parent)
            if (c.name.Trim() == name) return c;
        return null;
    }

    static TMP_Text FindText(string name)
    {
        foreach (TMP_Text t in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name.Trim() == name) return t;
        return null;
    }
}