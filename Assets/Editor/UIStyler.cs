using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using TMPro;

// EDITOR TOOL ONLY - not part of the game.
// Unity top menu > Tools > Style UI. Run it AFTER Tools > Build Menus
// (Build Menus makes new plain panels, so run Style UI again if you ever rebuild them).
// It does these things:
//   1. Title screen: the game's sky and graveyard behind it, the game title, pink ashes floating on top
//   2. "How to Play" screen with all the keys (shown after pressing Play)
//   3. Objective banner at the top right while playing
//   4. Puts the Dark Ages UI frames and buttons on the menus
//   5. Uses the gothic font for every text
//   6. Health bar, lives and souls at the top left (HealthBar.cs keeps the bar up to date)
// Running it again is safe: it deletes what it made last time and makes it again.
public static class UIStyler
{
    const string GameTitle = "Soulbound";    // change this to your game's name, then run Style UI again

    const string SheetPath = "Assets/ass/32x32-Tilesheet.png";
    const string FontPath = "Assets/ass/gothic-pixel-font.ttf";
    const string FontAssetPath = "Assets/ass/gothic-pixel-font SDF.asset";
    const string SkyPath = "Assets/ass/nature/Environment/background.png";        // pink moon sky
    const string GraveyardPath = "Assets/ass/nature/Environment/graveyard.png";   // graveyard hills

    // Sprite names inside the sheet (click the arrow on the sheet in the Project window to see them)
    const string PanelSprite = "32x32-Tilesheet_1";       // dark square frame with knot corners
    const string ButtonSprite = "32x32-Tilesheet_3";      // dark button
    const string ButtonHoverSprite = "32x32-Tilesheet_4"; // lighter button, shown when the mouse is on it
    const string BarFrameSprite = "32x32-Tilesheet_11";   // gold bar frame with the little crown
    const string BarFillSprite = "32x32-Tilesheet_14";    // red bar

    const float PixelScale = 4f;   // 1 pixel of the art = 4 pixels on a 1920x1080 screen, so it stays chunky

    // ---------- HUD layout (in pixels on a 1920x1080 screen) ----------
    //   [ health bar ]  Lives: 3
    //   Souls: 0 / 5
    const float HudScale = 6f;                       // the health bar is drawn bigger than the menus
    static readonly Vector2 hudStart = new Vector2(80, -40);   // top-left corner of the health bar
    const float HudGap = 30f;                        // space between the bar and the texts
    const float HudFontSize = 56f;
    static readonly Color gold = new Color(0.95f, 0.8f, 0.5f, 1f);
    static readonly Color pink = new Color(0.95f, 0.75f, 1f, 1f);            // titles
    static readonly Color dim = new Color(0.05f, 0.02f, 0.08f, 0.85f);       // dark see-through screen cover

    // The keys shown on the "How to Play" screen: what it does, which keys
    static readonly string[,] controls =
    {
        { "Move",   "A / D  or  Left / Right" },
        { "Jump",   "Space  (hold to jump higher)" },
        { "Dash",   "Q  or  Left Shift  (pass through enemies)" },
        { "Attack", "J,  X  or  Left Click" },
        { "Pause",  "Esc" },
        { "Mute",   "M" },
    };
    const string ObjectiveText = "Objective: collect the souls and reach the portal";

    [MenuItem("Tools/Style UI")]
    static void Style()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (canvas == null || gm == null || gm.titlePanel == null || gm.hud == null)
        {
            Debug.LogError("Run Tools > Build Menus first.");
            return;
        }

        SetBorders();   // tells Unity which part of each sprite is the edge (see below)

        Sprite panel = LoadSprite(PanelSprite);
        Sprite button = LoadSprite(ButtonSprite);
        Sprite buttonHover = LoadSprite(ButtonHoverSprite);
        Sprite barFrame = LoadSprite(BarFrameSprite);
        Sprite barFill = LoadSprite(BarFillSprite);
        if (panel == null || button == null || buttonHover == null || barFrame == null || barFill == null)
        {
            Debug.LogError("Couldn't find the sprites in " + SheetPath + ". Is the sheet still sliced?");
            return;
        }

        TMP_FontAsset font = GetFont();
        if (font == null) return;

        Sprite sky = AssetDatabase.LoadAssetAtPath<Sprite>(SkyPath);
        Sprite graveyard = AssetDatabase.LoadAssetAtPath<Sprite>(GraveyardPath);

        // ---------- 1. Frame behind each menu ----------
        // Title screen: a small frame only around Play and Quit, so the moon and graveyard stay visible
        AddFrame(gm.titlePanel, panel, canvas, new Vector2(0, -55), new Vector2(560, 340));
        foreach (GameObject p in new[] { gm.pausePanel, gm.gameOverPanel, gm.winPanel })
            AddFrame(p, panel, canvas, new Vector2(0, -10), new Vector2(1000, 700));

        // The HUD must cover the whole screen, so things can stick to any corner of it
        RectTransform hudRect = (RectTransform)gm.hud.transform;
        Undo.RecordObject(hudRect, "Style UI");
        Stretch(hudRect);

        // ---------- New screens (made before the buttons and font steps, so those style them too) ----------
        BuildTitleScreen(gm, sky, graveyard);
        BuildControlsPanel(gm, canvas, panel);
        BuildObjective(gm, canvas, panel);

        // ---------- 2. Buttons ----------
        foreach (Button b in canvas.GetComponentsInChildren<Button>(true))
        {
            Undo.RecordObject(b.image, "Style UI");
            b.image.sprite = button;
            b.image.type = Image.Type.Sliced;       // corners stay sharp, only the middle stretches
            b.image.color = Color.white;            // show the sprite's real colors
            b.image.pixelsPerUnitMultiplier = Multiplier(button, canvas);

            Undo.RecordObject(b, "Style UI");
            b.transition = Selectable.Transition.SpriteSwap;   // change the picture instead of the color
            SpriteState state = new SpriteState();
            state.highlightedSprite = buttonHover;   // mouse over it
            state.pressedSprite = buttonHover;       // clicking it
            state.selectedSprite = button;
            b.spriteState = state;
        }

        // ---------- 3. Gothic font on every text (except the key list, which uses a plain font so it's easy to read) ----------
        TMP_FontAsset plainFont = TMP_Settings.defaultFontAsset;   // TextMeshPro's normal font (Liberation Sans)
        foreach (TMP_Text t in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            Undo.RecordObject(t, "Style UI");
            bool keyList = t.transform.parent == gm.controlsPanel.transform
                           && (t.name.StartsWith("Action") || t.name.StartsWith("Keys"));
            t.font = keyList && plainFont != null ? plainFont : font;
            if (t.GetComponentInParent<Button>(true) != null)
                t.color = gold;                      // button labels in gold
        }

        // ---------- 4. Health bar, lives and souls at the top left ----------
        Vector2 barSize = AddHealthBar(gm, barFrame, barFill);

        // Lives: to the right of the bar, same height
        if (gm.livesText != null)
            PlaceText(gm.livesText, hudStart + new Vector2(barSize.x + HudGap, 0), new Vector2(400, barSize.y));

        // Souls: under the bar
        SoulCounter souls = Object.FindFirstObjectByType<SoulCounter>();
        if (souls != null && souls.text != null)
            PlaceText(souls.text, hudStart + new Vector2(0, -barSize.y - HudGap / 2), new Vector2(600, 80));

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("UI styled! Press Ctrl+S to save the scene.");
    }

    // ---------- Title screen ----------
    // Order of the title panel's children (first = drawn first = at the back):
    //   BG Sky, BG Graveyard, Frame, TitleText, buttons..., Ashes (last = on top of everything)
    static void BuildTitleScreen(GameManager gm, Sprite sky, Sprite graveyard)
    {
        Transform title = gm.titlePanel.transform;
        foreach (string n in new[] { "BG Sky", "BG Graveyard", "Ashes" })
        {
            Transform old = title.Find(n);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        }

        // The title text that Build Menus made
        TMP_Text titleText = title.Find("TitleText") != null ? title.Find("TitleText").GetComponent<TMP_Text>() : null;
        if (titleText != null)
        {
            Undo.RecordObject(titleText, "Style UI");
            titleText.text = GameTitle;
            titleText.fontSize = 120;
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            Place(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 380), new Vector2(1700, 200));
        }

        // Graveyard hills along the bottom, as wide as the screen
        if (graveyard != null)
        {
            GameObject g = NewImage("BG Graveyard", title, graveyard);
            RectTransform rt = (RectTransform)g.transform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);            // stretched left to right, stuck to the bottom
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = Vector2.zero;
            AspectRatioFitter fit = g.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;   // height follows the width, no squashing
            fit.aspectRatio = graveyard.rect.width / graveyard.rect.height;
            g.transform.SetSiblingIndex(0);
        }

        // Sky with the pink moon, covering the whole screen (behind the graveyard)
        if (sky != null)
        {
            GameObject s = NewImage("BG Sky", title, sky);
            AspectRatioFitter fit = s.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;        // fill the screen, no black bars
            fit.aspectRatio = sky.rect.width / sky.rect.height;
            s.transform.SetSiblingIndex(0);
        }

        // Pink ashes floating on top of everything
        GameObject ashes = NewUI("Ashes", title);
        Stretch((RectTransform)ashes.transform);
        ashes.AddComponent<AshesUI>();
        ashes.transform.SetAsLastSibling();
    }

    // ---------- "How to Play" screen ----------
    static void BuildControlsPanel(GameManager gm, Canvas canvas, Sprite panelSprite)
    {
        Transform old = canvas.transform.Find("ControlsPanel");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        GameObject p = NewUI("ControlsPanel", canvas.transform);
        Stretch((RectTransform)p.transform);
        p.AddComponent<Image>().color = dim;         // darkens the game behind it

        AddFrame(p, panelSprite, canvas, Vector2.zero, new Vector2(1300, 850));   // bigger than the other menus

        TMP_Text heading = NewText(p.transform, "TitleText", "How to Play", 80, pink, TextAlignmentOptions.Center);
        Place(heading.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 320), new Vector2(1100, 120));

        // One row per key: the action on the left in gold, the keys on the right in white
        for (int i = 0; i < controls.GetLength(0); i++)
        {
            float y = 200 - i * 75;

            TMP_Text action = NewText(p.transform, "Action " + i, controls[i, 0], 44, gold, TextAlignmentOptions.Right);
            PlaceWithPivot(action.rectTransform, new Vector2(1, 0.5f), new Vector2(-200, y), new Vector2(380, 70));

            TMP_Text keys = NewText(p.transform, "Keys " + i, controls[i, 1], 42, Color.white, TextAlignmentOptions.Left);
            PlaceWithPivot(keys.rectTransform, new Vector2(0, 0.5f), new Vector2(-160, y), new Vector2(720, 70));
            keys.enableAutoSizing = true;            // shrink a long line instead of going outside the frame
            keys.fontSizeMin = 26;
            keys.fontSizeMax = 42;
        }

        NewButton(p.transform, "Start", new Vector2(0, -300), gm.BeginPlaying);

        p.SetActive(false);                          // hidden until the player presses Play
        Undo.RecordObject(gm, "Style UI");
        gm.controlsPanel = p;
        EditorUtility.SetDirty(gm);
    }

    // ---------- Objective banner (part of the HUD, so it shows only while playing) ----------
    static void BuildObjective(GameManager gm, Canvas canvas, Sprite panelSprite)
    {
        Transform hud = gm.hud.transform;
        Transform old = hud.Find("Objective");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        GameObject o = NewUI("Objective", hud);
        Place((RectTransform)o.transform, new Vector2(1, 1), new Vector2(-60, -40), new Vector2(760, 110));  // top right

        Image img = o.AddComponent<Image>();
        img.sprite = panelSprite;
        img.type = Image.Type.Tiled;
        img.pixelsPerUnitMultiplier = Multiplier(panelSprite, canvas, 2f);   // thinner border than the menus, so the text fits
        img.raycastTarget = false;

        TMP_Text t = NewText(o.transform, "ObjectiveText", ObjectiveText, 40, gold, TextAlignmentOptions.Center);
        Stretch(t.rectTransform);
        t.margin = new Vector4(30, 10, 30, 10);      // keep the words off the border (left, top, right, bottom)
        t.enableAutoSizing = true;
        t.fontSizeMin = 22;
        t.fontSizeMax = 40;
        t.textWrappingMode = TextWrappingModes.Normal;   // allowed to use two lines if it needs to
    }

    // A dark frame inside a menu, behind its title and buttons
    static void AddFrame(GameObject menu, Sprite sprite, Canvas canvas, Vector2 pos, Vector2 size)
    {
        Transform old = menu.transform.Find("Frame");
        GameObject frame = old != null ? old.gameObject : NewUI("Frame", menu.transform);
        frame.transform.SetAsFirstSibling();         // first child = drawn first = behind everything

        Place((RectTransform)frame.transform, new Vector2(0.5f, 0.5f), pos, size);

        Image img = frame.GetComponent<Image>();
        if (img == null) img = Undo.AddComponent<Image>(frame);
        Undo.RecordObject(img, "Style UI");
        img.sprite = sprite;
        img.type = Image.Type.Tiled;                  // repeats the knot pattern instead of stretching it
        img.pixelsPerUnitMultiplier = Multiplier(sprite, canvas);
        img.raycastTarget = false;                    // it's only decoration, don't block clicks
    }

    // Gold frame + red fill, top left of the HUD. Gives back its size so the texts can line up with it.
    static Vector2 AddHealthBar(GameManager gm, Sprite frameSprite, Sprite fillSprite)
    {
        float s = HudScale;
        Transform hud = gm.hud.transform;
        Vector2 size = new Vector2(frameSprite.rect.width * s, frameSprite.rect.height * s);

        Transform old = hud.Find("HealthBar");
        GameObject bar = old != null ? old.gameObject : NewUI("HealthBar", hud);
        Place((RectTransform)bar.transform, new Vector2(0f, 1f), hudStart, size);

        Image frame = bar.GetComponent<Image>();
        if (frame == null) frame = Undo.AddComponent<Image>(bar);
        Undo.RecordObject(frame, "Style UI");
        frame.sprite = frameSprite;
        frame.type = Image.Type.Simple;
        frame.raycastTarget = false;

        // The red fill sits inside the frame's dark middle:
        // 3 pixels in from the left, right and bottom, and 4 pixels tall
        Transform oldFill = bar.transform.Find("Fill");
        GameObject fillGO = oldFill != null ? oldFill.gameObject : NewUI("Fill", bar.transform);
        RectTransform fillRect = (RectTransform)fillGO.transform;
        Undo.RecordObject(fillRect, "Style UI");
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.zero;
        fillRect.pivot = Vector2.zero;
        fillRect.anchoredPosition = new Vector2(3 * s, 3 * s);
        fillRect.sizeDelta = new Vector2((frameSprite.rect.width - 6) * s, 4 * s);

        Image fill = fillGO.GetComponent<Image>();
        if (fill == null) fill = Undo.AddComponent<Image>(fillGO);
        Undo.RecordObject(fill, "Style UI");
        fill.sprite = fillSprite;
        fill.type = Image.Type.Filled;               // can show only part of the picture
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;   // empties from right to left
        fill.fillAmount = 1f;
        fill.raycastTarget = false;

        HealthBar hb = bar.GetComponent<HealthBar>();
        if (hb == null) hb = Undo.AddComponent<HealthBar>(bar);
        Undo.RecordObject(hb, "Style UI");
        hb.player = gm.player;
        hb.fill = fill;
        EditorUtility.SetDirty(hb);
        return size;
    }

    // Moves a HUD text to the top-left area, bigger, with its letters lined up on the left
    static void PlaceText(TMP_Text t, Vector2 pos, Vector2 size)
    {
        Place(t.rectTransform, new Vector2(0f, 1f), pos, size);
        Undo.RecordObject(t, "Style UI");
        t.fontSize = HudFontSize;
        t.alignment = TextAlignmentOptions.Left;      // middle-left: lines up with the middle of the bar
        t.textWrappingMode = TextWrappingModes.NoWrap;   // keep it on one line
    }

    // Borders = how many pixels on each side are "edge". With them, Sliced/Tiled images keep
    // their corners the same size and only stretch or repeat the middle.
    static void SetBorders()
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(SheetPath);
        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider data = factory.GetSpriteEditorDataProviderFromObject(importer);
        data.InitSpriteEditorDataProvider();

        SpriteRect[] rects = data.GetSpriteRects();
        foreach (SpriteRect r in rects)
        {
            // order is left, bottom, right, top
            if (r.name == PanelSprite) r.border = new Vector4(14, 14, 14, 14);
            if (r.name == ButtonSprite || r.name == ButtonHoverSprite) r.border = new Vector4(6, 6, 6, 6);
        }
        data.SetSpriteRects(rects);
        data.Apply();
        importer.SaveAndReimport();
    }

    // Makes a TextMeshPro font from the .ttf the first time, then reuses it
    static TMP_FontAsset GetFont()
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset != null) return fontAsset;

        Font source = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (source == null) { Debug.LogError("No font at " + FontPath); return null; }

        // same settings as right-click > Create > TextMeshPro > Font Asset
        fontAsset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        fontAsset.atlasTextures[0].name = "gothic-pixel-font Atlas";
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);   // save the letters picture inside it
        fontAsset.material.name = "gothic-pixel-font Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }

    // ---------- Helpers ----------

    static Sprite LoadSprite(string name)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(SheetPath))
            if (o is Sprite sp && sp.name == name) return sp;
        return null;
    }

    // Makes Sliced/Tiled borders show at 'scale' size (PixelScale if not given), whatever the sprite's Pixels Per Unit is
    static float Multiplier(Sprite sprite, Canvas canvas, float scale = PixelScale)
    {
        return canvas.referencePixelsPerUnit / (sprite.pixelsPerUnit * scale);
    }

    static GameObject NewImage(string name, Transform parent, Sprite sprite)
    {
        GameObject go = NewUI(name, parent);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return go;
    }

    static TMP_Text NewText(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align)
    {
        TextMeshProUGUI t = NewUI(name, parent).AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    // A plain button; step 2 of Style() gives it the gothic look afterwards
    static void NewButton(Transform parent, string label, Vector2 pos, UnityAction action)
    {
        GameObject b = NewUI(label + "Button", parent);
        Place((RectTransform)b.transform, new Vector2(0.5f, 0.5f), pos, new Vector2(380, 85));
        b.AddComponent<Image>();
        Button btn = b.AddComponent<Button>();

        TMP_Text t = NewText(b.transform, "Text", label, 46, gold, TextAlignmentOptions.Center);
        Stretch(t.rectTransform);

        UnityEventTools.AddPersistentListener(btn.onClick, action);   // same as dragging GameManager into On Click
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // Like Place, but anchored in the middle with its own pivot (1 = right edge, 0 = left edge)
    static void PlaceWithPivot(RectTransform rt, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Style UI");
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        Undo.RecordObject(rt, "Style UI");
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
