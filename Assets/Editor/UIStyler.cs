using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using TMPro;

// EDITOR TOOL ONLY - not part of the game.
// Unity top menu > Tools > Style UI. Run it AFTER Tools > Build Menus
// (Build Menus makes new plain panels, so run Style UI again if you ever rebuild them).
// It does 3 things:
//   1. Puts the Dark Ages UI frames and buttons on the menus
//   2. Uses the gothic font for every text
//   3. Adds a health bar at the top of the screen (HealthBar.cs keeps it up to date in the game)
public static class UIStyler
{
    const string SheetPath = "Assets/ass/32x32-Tilesheet.png";
    const string FontPath = "Assets/ass/gothic-pixel-font.ttf";
    const string FontAssetPath = "Assets/ass/gothic-pixel-font SDF.asset";

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

        // ---------- 1. Frame behind each menu ----------
        foreach (GameObject p in new[] { gm.titlePanel, gm.pausePanel, gm.gameOverPanel, gm.winPanel })
            AddFrame(p, panel, canvas);

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

        // ---------- 3. Gothic font on every text ----------
        foreach (TMP_Text t in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            Undo.RecordObject(t, "Style UI");
            t.font = font;
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

    // A dark frame in the middle of the menu, behind the title and the buttons
    static void AddFrame(GameObject menu, Sprite sprite, Canvas canvas)
    {
        Transform old = menu.transform.Find("Frame");
        GameObject frame = old != null ? old.gameObject : NewUI("Frame", menu.transform);
        frame.transform.SetAsFirstSibling();         // first child = drawn first = behind everything

        Place((RectTransform)frame.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1000, 700));

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

    // Makes Sliced/Tiled borders show at PixelScale size, whatever the sprite's Pixels Per Unit is
    static float Multiplier(Sprite sprite, Canvas canvas)
    {
        return canvas.referencePixelsPerUnit / (sprite.pixelsPerUnit * PixelScale);
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
