using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

// Aplica o tema "Massinha" nos menus da cena aberta:
// menu principal, configurações, créditos, pausa e vitória.
// Encontra os painéis pelas referências dos scripts do jogo
// (MainMenuSettings, PauseManager, LevelCompleteUI, AudioSettingsUI).
// Pode ser rodado de novo: não duplica nada.
// A cena só é marcada como alterada — feche sem salvar para desfazer.
public static class MassinhaThemeTool
{
    private const string Root = "Assets/UI/Massinha";
    private const string FontFolder = Root + "/Fonts";

    // Paleta
    private static readonly Color Ink = Hex("#22352A");
    private static readonly Color Green = Hex("#43A34F");
    private static readonly Color GreenDark = Hex("#1F5A2C");
    private static readonly Color Sun = Hex("#FFC83D");
    private static readonly Color StarLocked = Hex("#D8D0BC");
    private static readonly Color StampRed = Hex("#D9483B");
    private static readonly Color DimColor = new Color(0.08f, 0.16f, 0.10f, 0.5f);

    private const string CommonCharacters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
        "ÁÀÂÃÉÊÍÓÔÕÚÜÇáàâãéêíóôõúüç0123456789 .,:;!?%$+-–()/'\"#@&*";

    private static TMP_FontAsset fredokaBold;
    private static TMP_FontAsset fredokaSemiBold;
    private static TMP_FontAsset nunitoBold;
    private static TMP_FontAsset nunitoExtraBold;
    private static Material logoMaterial;

    private static readonly List<string> Report = new List<string>();


    // ================================================================
    // Menu
    // ================================================================

    [MenuItem("Eco Connect/Tema Massinha/Aplicar na cena aberta")]
    private static void ApplyToOpenScene()
    {
        if (!PrepareAssets())
            return;

        Report.Clear();

        MainMenuSettings mainMenu = FindInScene<MainMenuSettings>();
        PauseManager pause = FindInScene<PauseManager>();
        LevelCompleteUI victory = FindInScene<LevelCompleteUI>();
        ToolSelectionUI tools = FindInScene<ToolSelectionUI>();

        if (mainMenu != null) StyleMainMenu(mainMenu);
        if (pause != null) StylePause(pause);
        if (victory != null) StyleVictory(victory);
        if (tools != null) StyleHud(tools, pause);

        if (mainMenu == null && pause == null && victory == null && tools == null)
        {
            EditorUtility.DisplayDialog(
                "Tema Massinha",
                "Não encontrei menus nesta cena (MainMenuSettings, PauseManager ou LevelCompleteUI).",
                "OK");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);

        string summary = string.Join("\n", Report);
        Debug.Log("[Tema Massinha] " + scene.name + "\n" + summary);

        EditorUtility.DisplayDialog(
            "Tema Massinha",
            "Tema aplicado em \"" + scene.name + "\".\n\n" + summary +
            "\n\nSalve a cena (Ctrl+S) se gostar. Para desfazer, feche sem salvar.",
            "OK");
    }

    [MenuItem("Eco Connect/Tema Massinha/Preparar sprites e fontes")]
    private static void PrepareAssetsMenu()
    {
        if (PrepareAssets())
            Debug.Log("[Tema Massinha] Sprites e fontes prontos.");
    }


    // ================================================================
    // Sprites e fontes
    // ================================================================

    private static bool PrepareAssets()
    {
        // Garante que os sprites foram importados com o MassinhaSpriteImporter
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { MassinhaSpriteImporter.SpriteFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);

            if (importer != null && importer.textureType != TextureImporterType.Sprite)
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        if (LoadSprite("btn_cream") == null)
        {
            EditorUtility.DisplayDialog("Tema Massinha",
                "Sprites não encontrados em " + MassinhaSpriteImporter.SpriteFolder, "OK");
            return false;
        }

        fredokaBold = EnsureFontAsset("Fredoka-Bold", 12);
        fredokaSemiBold = EnsureFontAsset("Fredoka-SemiBold", 9);
        nunitoBold = EnsureFontAsset("Nunito-Bold", 9);
        nunitoExtraBold = EnsureFontAsset("Nunito-ExtraBold", 9);

        if (fredokaBold == null || fredokaSemiBold == null || nunitoBold == null || nunitoExtraBold == null)
        {
            EditorUtility.DisplayDialog("Tema Massinha",
                "Não consegui criar as fontes TMP a partir de " + FontFolder, "OK");
            return false;
        }

        logoMaterial = EnsureLogoMaterial(fredokaBold);

        return true;
    }

    private static TMP_FontAsset EnsureFontAsset(string fontName, int padding)
    {
        string assetPath = FontFolder + "/" + fontName + " SDF.asset";

        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

        if (existing != null)
        {
            // Versões antigas tinham ~22 mil registros de kerning de alfabetos que
            // não usamos (10–14 MB por arquivo). Regenera a partir da fonte já cortada.
            if (new System.IO.FileInfo(assetPath).Length > 6 * 1024 * 1024)
                RebuildFontData(existing);

            return existing;
        }

        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontFolder + "/" + fontName + ".ttf");

        if (font == null)
            return null;

        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
            font, 90, padding, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic, true);

        asset.name = fontName + " SDF";

        AssetDatabase.CreateAsset(asset, assetPath);

        asset.atlasTextures[0].name = fontName + " SDF Atlas";
        AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);

        asset.material.name = fontName + " SDF Material";
        AssetDatabase.AddObjectToAsset(asset.material, asset);

        asset.TryAddCharacters(CommonCharacters, out _);

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        return asset;
    }

    [MenuItem("Eco Connect/Tema Massinha/Reconstruir dados das fontes")]
    private static void RebuildAllFontsMenu()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { FontFolder }))
        {
            TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));

            if (asset != null)
                RebuildFontData(asset);
        }

        Debug.Log("[Tema Massinha] Dados das fontes reconstruídos.");
    }

    // Limpa glifos, kerning e atlas e preenche de novo só com os caracteres usados.
    // O arquivo continua o mesmo, então as cenas não perdem a referência.
    private static void RebuildFontData(TMP_FontAsset asset)
    {
        asset.ClearFontAssetData(true);
        asset.TryAddCharacters(CommonCharacters, out _);

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        Debug.Log("[Tema Massinha] Fonte regenerada: " + asset.name);
    }

    // Material do logo: contorno verde-escuro + "degrau" embaixo
    private static Material EnsureLogoMaterial(TMP_FontAsset font)
    {
        string path = FontFolder + "/Fredoka-Bold SDF Logo.mat";

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(font.material) { name = "Fredoka-Bold SDF Logo" };
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetColor("_FaceColor", Color.white);
        material.SetFloat("_OutlineWidth", 0.24f);
        material.SetColor("_OutlineColor", GreenDark);

        material.EnableKeyword("UNDERLAY_ON");
        material.SetColor("_UnderlayColor", GreenDark);
        material.SetFloat("_UnderlayOffsetX", 0f);
        material.SetFloat("_UnderlayOffsetY", -0.6f);
        material.SetFloat("_UnderlayDilate", 0.3f);
        material.SetFloat("_UnderlaySoftness", 0f);

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();

        return material;
    }


    // ================================================================
    // Menu principal
    // ================================================================

    private static void StyleMainMenu(MainMenuSettings menu)
    {
        Transform canvas = menu.transform;

        // Logo
        TMP_Text title = canvas.Find("Title")?.GetComponent<TMP_Text>();

        if (title != null)
        {
            SetText(title, fredokaBold, 170, Color.white, TextAlignmentOptions.TopLeft, "Eco\nConnect");
            title.fontSharedMaterial = logoMaterial;
            title.lineSpacing = -28;
            Place(Rect(title), new Vector2(0, 1), new Vector2(0, 1), new Vector2(110, -60), new Vector2(900, 380), 3f);

            // Remove a faixa amarela de versões anteriores do tema
            Transform tagline = canvas.Find("Tagline");

            if (tagline != null)
                Object.DestroyImmediate(tagline.gameObject);

            Report.Add("• Logo do menu principal");
        }

        // Botões: coluna no canto inferior esquerdo
        Transform buttons = canvas.Find("Buttons");

        if (buttons != null)
        {
            GridLayoutGroup grid = buttons.GetComponent<GridLayoutGroup>();

            if (grid != null)
                Object.DestroyImmediate(grid);

            VerticalLayoutGroup layout = Comp<VerticalLayoutGroup>(buttons.gameObject);
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset();

            Place((RectTransform)buttons, Vector2.zero, Vector2.zero, new Vector2(115, 95),
                new Vector2(500, 124 + 100 * 3 + 24 * 3));

            StyleMenuButton(buttons, "Play Button", 0, "green", "Jogar", "play", 124, 46, true);
            StyleMenuButton(buttons, "Settings Button", 1, "cream", "Configurações", "gear", 100, 38, false);
            StyleMenuButton(buttons, "Credits Button", 2, "cream", "Créditos", "heart", 100, 38, false);
            StyleMenuButton(buttons, "Quit Button", 3, "cream", "Sair", "exit", 100, 38, false);

            Report.Add("• Botões do menu principal");
        }

        GameObject settings = Ref<GameObject>(menu, "settingsPanel");

        if (settings != null)
            StyleSettings(settings, true, menu.CloseSettings);

        GameObject credits = Ref<GameObject>(menu, "creditsPanel");

        if (credits != null)
            StyleCredits(credits, menu.CloseCredits);
    }

    private static void StyleMenuButton(Transform parent, string name, int order,
        string kind, string label, string icon, float height, float fontSize, bool pulse)
    {
        Transform t = parent.Find(name);

        if (t == null)
        {
            Report.Add("  (não encontrei " + name + ")");
            return;
        }

        t.SetSiblingIndex(order);

        StyleButton(t.GetComponent<Button>(), kind, label, icon, fontSize,
            alignLeft: true, pulse: pulse);

        LayoutElement element = Comp<LayoutElement>(t.gameObject);
        element.preferredHeight = height;
        element.minHeight = height;
    }


    // ================================================================
    // Configurações e créditos
    // ================================================================

    private static void StyleSettings(GameObject panel, bool withDim, UnityAction close)
    {
        AudioSettingsUI audio = panel.GetComponent<AudioSettingsUI>();
        RectTransform card = panel.transform.Find("Background") as RectTransform;

        if (audio == null || card == null)
        {
            Report.Add("  (painel de configurações sem AudioSettingsUI ou Background: " + panel.name + ")");
            return;
        }

        TMP_Text title = FindText(panel.transform, "Settings Title");
        Slider music = Ref<Slider>(audio, "musicVolumeSlider");
        Slider sfx = Ref<Slider>(audio, "sfxVolumeSlider");

        MakeModal(panel, card, new Vector2(900, 480), withDim, title, "Configurações");

        if (music != null)
        {
            music.transform.SetParent(card, false);
            TMP_Text value = StyleSlider(music, "Música", "music", "bubble_sky", Color.white, -175);
            SetRef(audio, "musicValueText", value);
        }

        if (sfx != null)
        {
            sfx.transform.SetParent(card, false);
            TMP_Text value = StyleSlider(sfx, "Efeitos", "sound", "bubble_sun", Ink, -285);
            SetRef(audio, "sfxValueText", value);
        }

        AddBackButton(card, close);

        Report.Add("• Configurações (" + panel.scene.name + ")");
    }

    private static void StyleCredits(GameObject panel, UnityAction close)
    {
        RectTransform card = panel.transform.Find("Background") as RectTransform;

        if (card == null)
            return;

        TMP_Text body = panel.GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(t => t.transform.parent == panel.transform || t.transform.parent == card && t.name != "Label");

        MakeModal(panel, card, new Vector2(1000, 620), true, null, "Créditos");

        if (body != null)
        {
            body.transform.SetParent(card, false);
            SetText(body, nunitoBold, 32, Ink, TextAlignmentOptions.Center);
            body.textWrappingMode = TextWrappingModes.Normal;
            body.lineSpacing = 8;
            Stretch(Rect(body), 70, 170, 70, 110);
        }

        AddBackButton(card, close);

        Report.Add("• Créditos");
    }

    private static TMP_Text StyleSlider(Slider slider, string label, string icon,
        string bubbleSprite, Color iconColor, float y)
    {
        RectTransform rect = (RectTransform)slider.transform;

        // Linha: [bolha][rótulo][trilho][%]. O trilho é o próprio Slider.
        Place(rect, new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(73, y), new Vector2(380, 44));

        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.transition = Selectable.Transition.None;

        RectTransform background = rect.Find("Background") as RectTransform;

        if (background != null)
        {
            Stretch(background);
            SetImage(background.gameObject, "slider_track", raycast: true);
        }

        if (slider.fillRect != null)
        {
            Stretch((RectTransform)slider.fillRect.parent, 6, 6, 6, 6);
            slider.fillRect.offsetMin = Vector2.zero;
            slider.fillRect.offsetMax = Vector2.zero;
            slider.fillRect.localScale = Vector3.one;
            SetImage(slider.fillRect.gameObject, "slider_fill");
        }

        if (slider.handleRect != null)
        {
            Stretch((RectTransform)slider.handleRect.parent, 22, 0, 22, 0);
            slider.handleRect.sizeDelta = new Vector2(58, 20);
            // Algumas cenas tinham o puxador com escala X = 3 (ficava uma pílula esticada)
            slider.handleRect.localScale = Vector3.one;
            slider.handleRect.localRotation = Quaternion.identity;

            Image knob = SetImage(slider.handleRect.gameObject, "slider_knob", sliced: false, raycast: true);
            knob.preserveAspect = true;
            slider.targetGraphic = knob;
        }

        // Rótulo (o texto que já existia dentro do slider)
        TMP_Text text = slider.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name != "Value");

        if (text != null)
        {
            SetText(text, nunitoExtraBold, 34, Ink, TextAlignmentOptions.MidlineLeft, label);
            Place(Rect(text), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-180, 0), new Vector2(160, 56));
        }

        // Bolha com ícone
        RectTransform bubble = Child(rect, "Bubble");
        SetImage(bubble.gameObject, bubbleSprite, sliced: false).preserveAspect = true;
        Place(bubble, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(-196, 0), new Vector2(72, 72));

        RectTransform bubbleIcon = Child(bubble, "Icon");
        SetImage(bubbleIcon.gameObject, "icon_" + icon, iconColor, sliced: false).preserveAspect = true;
        Place(bubbleIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40, 40));

        // Porcentagem
        TMP_Text value = TextChild(rect, "Value");
        SetText(value, nunitoExtraBold, 32, Ink, TextAlignmentOptions.MidlineLeft,
            Mathf.RoundToInt(slider.value * 100) + "%");
        Place(Rect(value), new Vector2(1, 0.5f), new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(100, 56));

        return value;
    }

    private static void AddBackButton(RectTransform card, UnityAction close)
    {
        Button back = EnsureButton(card, "Back Button");

        StyleButton(back, "green", "Voltar", null, 38);
        Place((RectTransform)back.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 46), new Vector2(300, 104));

        if (back.onClick.GetPersistentEventCount() == 0 && close != null)
            UnityEventTools.AddPersistentListener(back.onClick, close);
    }


    // ================================================================
    // Pausa
    // ================================================================

    private static void StylePause(PauseManager pause)
    {
        GameObject pausePanel = Ref<GameObject>(pause, "pausePanel");
        GameObject pauseMenu = Ref<GameObject>(pause, "pauseMenu");
        GameObject settings = Ref<GameObject>(pause, "settingsPanel");

        if (pausePanel == null || pauseMenu == null)
            return;

        // Fundo escurecido compartilhado (pausa e configurações)
        Stretch((RectTransform)pausePanel.transform);
        AddDim(pausePanel.transform);

        RectTransform card = pauseMenu.transform.Find("Background") as RectTransform;
        Transform buttons = pauseMenu.transform.Find("Buttons");

        if (card != null && buttons != null)
        {
            TMP_Text title = FindText(pauseMenu.transform, "Pause Title");

            // Conserta o botão "Sair" que estava dentro do botão Configurações
            Button mainMenuButton = pauseMenu.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.name == "Main Menu Button");

            if (mainMenuButton != null && mainMenuButton.transform.parent != buttons)
            {
                mainMenuButton.transform.SetParent(buttons, false);
                Report.Add("• Botão \"Menu principal\" tirado de dentro de Configurações");
            }

            MakeModal(pauseMenu, card, new Vector2(640, 640), false, title, "Pausado");

            buttons.SetParent(card, false);
            Stretch((RectTransform)buttons, 70, 60, 70, 110);

            VerticalLayoutGroup layout = Comp<VerticalLayoutGroup>(buttons.gameObject);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset();

            StylePauseButton(buttons, "Resume Button", 0, "green", "Continuar", "play");
            StylePauseButton(buttons, "Restart Button", 1, "cream", "Reiniciar", "retry");
            StylePauseButton(buttons, "Settings Button", 2, "cream", "Configurações", "gear");
            StylePauseButton(buttons, "Main Menu Button", 3, "cream", "Menu principal", "home");

            Report.Add("• Menu de pausa");
        }

        if (settings != null)
            StyleSettings(settings, false, pause.CloseSettings);
    }

    private static void StylePauseButton(Transform parent, string name, int order,
        string kind, string label, string icon)
    {
        Transform t = parent.Find(name);

        if (t == null)
        {
            Report.Add("  (não encontrei " + name + ")");
            return;
        }

        t.SetSiblingIndex(order);

        StyleButton(t.GetComponent<Button>(), kind, label, icon, 36);

        LayoutElement element = Comp<LayoutElement>(t.gameObject);
        element.preferredHeight = 100;
        element.minHeight = 100;
    }


    // ================================================================
    // Vitória
    // ================================================================

    private static void StyleVictory(LevelCompleteUI victory)
    {
        GameObject panel = Ref<GameObject>(victory, "victoryPanel");

        if (panel == null)
            return;

        RectTransform card =
            (panel.transform.Find("Backgound") ?? panel.transform.Find("Background")) as RectTransform;

        if (card == null)
        {
            Report.Add("  (vitória sem Background)");
            return;
        }

        TMP_Text title = FindText(panel.transform, "Victory Tile");
        Transform starPanel = panel.transform.Find("Star Pannel") ?? card.Find("Star Pannel");
        Transform buttons = panel.transform.Find("Buttons") ?? card.Find("Buttons");

        TMP_Text spent = Ref<TMP_Text>(victory, "spentBudgetText");
        TMP_Text remaining = Ref<TMP_Text>(victory, "remainingBudgetText");

        MakeModal(panel, card, new Vector2(1000, 680), true, title, "Fase concluída!");

        // Estrelas (a do meio maior)
        if (starPanel != null)
        {
            starPanel.SetParent(card, false);
            Place((RectTransform)starPanel, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -100), new Vector2(640, 200));

            HorizontalLayoutGroup layout = Comp<HorizontalLayoutGroup>(starPanel.gameObject);
            layout.spacing = 18;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            SerializedObject so = new SerializedObject(victory);
            SerializedProperty stars = so.FindProperty("stars");

            for (int i = 0; i < stars.arraySize; i++)
            {
                Image star = stars.GetArrayElementAtIndex(i).objectReferenceValue as Image;

                if (star == null)
                    continue;

                SetImage(star.gameObject, "star", sliced: false).preserveAspect = true;

                float size = i == 1 ? 190 : 150;
                star.rectTransform.sizeDelta = new Vector2(size, size);
            }

            so.FindProperty("earnedStarColor").colorValue = Sun;
            so.FindProperty("lockedStarColor").colorValue = StarLocked;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Dinheiro em "pílulas"
        RectTransform chips = Child(card, "Chips");
        Place(chips, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -330), new Vector2(720, 72));

        HorizontalLayoutGroup chipLayout = Comp<HorizontalLayoutGroup>(chips.gameObject);
        chipLayout.spacing = 22;
        chipLayout.childAlignment = TextAnchor.MiddleCenter;
        chipLayout.childControlWidth = true;
        chipLayout.childControlHeight = true;
        chipLayout.childForceExpandWidth = false;
        chipLayout.childForceExpandHeight = false;

        MakeChip(chips, "Chip Gasto", spent, "<color=#5B6B60>Gasto</color>  R$ 0");
        MakeChip(chips, "Chip Saldo", remaining, "<color=#5B6B60>Saldo</color>  R$ 0");

        // Botões: [Menu] [Repetir] [Próxima fase]
        if (buttons != null)
        {
            buttons.SetParent(card, false);
            Place((RectTransform)buttons, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 52), new Vector2(780, 112));

            HorizontalLayoutGroup layout = Comp<HorizontalLayoutGroup>(buttons.gameObject);
            layout.spacing = 22;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset();

            StyleVictoryButton(buttons, "Quit Button", 0, "cream", "Menu", "home", 112, iconOnly: true);
            StyleVictoryButton(buttons, "Retry Button", 1, "sun", "Repetir", "retry", 250);
            StyleVictoryButton(buttons, "Next Level Button", 2, "green", "Próxima fase", "next", 340,
                iconRight: true, pulse: true);
        }

        // Carimbo "Aprovado" no canto
        RectTransform stamp = Child(card, "Stamp");
        stamp.SetAsLastSibling();
        SetImage(stamp.gameObject, "stamp_frame", StampRed);
        Comp<CanvasGroup>(stamp.gameObject);
        Place(stamp, new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-110, -72), new Vector2(280, 112), 9f);

        TMP_Text stampText = TextChild(stamp, "Stamp Text");
        SetText(stampText, fredokaBold, 40, StampRed, TextAlignmentOptions.Center, "APROVADO");
        Stretch(Rect(stampText), 16, 40, 16, 8);

        TMP_Text stampDetail = TextChild(stamp, "Stamp Detail");
        SetText(stampDetail, nunitoExtraBold, 20, StampRed, TextAlignmentOptions.Center, "3 ESTRELAS");
        Stretch(Rect(stampDetail), 10, 14, 10, 72);

        stamp.gameObject.SetActive(false);

        SetRef(victory, "stamp", stamp);
        SetRef(victory, "stampDetailText", stampDetail);

        Report.Add("• Tela de vitória com carimbo");
    }

    private static void MakeChip(RectTransform chips, string name, TMP_Text text, string placeholder)
    {
        if (text == null)
            return;

        RectTransform chip = Child(chips, name);
        SetImage(chip.gameObject, "pill_white");

        LayoutElement element = Comp<LayoutElement>(chip.gameObject);
        element.preferredWidth = 320;
        element.preferredHeight = 72;

        text.transform.SetParent(chip, false);
        Stretch(Rect(text), 20, 4, 20, 4);
        SetText(text, nunitoExtraBold, 30, Ink, TextAlignmentOptions.Center, placeholder);
        text.richText = true;
    }

    private static void StyleVictoryButton(Transform parent, string name, int order, string kind,
        string label, string icon, float width, bool iconOnly = false, bool iconRight = false, bool pulse = false)
    {
        Transform t = parent.Find(name);

        if (t == null)
        {
            Report.Add("  (não encontrei " + name + ")");
            return;
        }

        t.SetSiblingIndex(order);

        StyleButton(t.GetComponent<Button>(), kind, label, icon, 36,
            iconOnly: iconOnly, iconRight: iconRight, pulse: pulse);

        LayoutElement element = Comp<LayoutElement>(t.gameObject);
        element.preferredWidth = width;
        element.preferredHeight = 108;
    }


    // ================================================================
    // HUD da fase (só a interface: o tabuleiro não é alterado)
    // ================================================================

    // Cores dos canos do tabuleiro (arte original), usadas nas bolhas das ferramentas
    private static readonly Color WaterColor = Hex("#3DC2B1");
    private static readonly Color PowerColor = Hex("#D3E14E");
    private static readonly Color SewageColor = Hex("#44FB36");
    private static readonly Color EraserColor = Hex("#F4B6A8");
    private static readonly Color Muted = Hex("#5B6B60");
    private static readonly Color RefundGreen = Hex("#1F7A3A");

    private static void StyleHud(ToolSelectionUI tools, PauseManager pause)
    {
        Transform canvas = tools.GetComponentInParent<Canvas>().transform;
        int hudIndex = tools.transform.GetSiblingIndex();

        // --- Nome da fase (coluna da esquerda, topo)
        GridManager grid = FindInScene<GridManager>();

        RectTransform levelRibbon = Child(canvas, "Level Name");
        levelRibbon.SetSiblingIndex(hudIndex + 1);
        SetImage(levelRibbon.gameObject, "ribbon");
        Place(levelRibbon, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -34), new Vector2(220, 88));

        HorizontalLayoutGroup ribbonLayout = Comp<HorizontalLayoutGroup>(levelRibbon.gameObject);
        ribbonLayout.padding = new RectOffset(36, 36, 12, 24);
        ribbonLayout.childAlignment = TextAnchor.MiddleCenter;
        ribbonLayout.childControlWidth = true;
        ribbonLayout.childControlHeight = true;
        ribbonLayout.childForceExpandWidth = false;
        ribbonLayout.childForceExpandHeight = false;

        ContentSizeFitter ribbonFit = Comp<ContentSizeFitter>(levelRibbon.gameObject);
        ribbonFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        ribbonFit.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        string levelName = grid != null && grid.CurrentLevel != null
            ? grid.CurrentLevel.levelName.Replace("Level", "Fase")
            : "Fase";

        TMP_Text levelText = TextChild(levelRibbon, "Text");
        SetText(levelText, fredokaBold, 40, Color.white, TextAlignmentOptions.Center, levelName);

        LevelNameUI levelNameUI = Comp<LevelNameUI>(levelRibbon.gameObject);
        SetRef(levelNameUI, "gridManager", grid);
        SetRef(levelNameUI, "label", levelText);

        // --- Orçamento, ferramentas, progresso, botões, custo e destaque
        BudgetUI budget = FindInScene<BudgetUI>();

        if (budget != null)
            StyleBudget(budget);

        StyleTools(tools);

        LevelProgressUI progress = FindInScene<LevelProgressUI>();

        if (progress != null)
            StyleProgress(progress);

        StyleTopRightButtons(canvas, pause, hudIndex + 2);

        ConstructionCostPreview cost = FindInScene<ConstructionCostPreview>();

        if (cost != null)
            StyleCostPreview(cost);

        EnsureCellHighlight(grid, pause);

        Report.Add("• HUD da fase (nome, orçamento, ferramentas, progresso, pausa, custo, destaque)");
    }

    private static void StyleBudget(BudgetUI budget)
    {
        RectTransform panel = Rect(budget);
        SetImage(panel.gameObject, "card");
        Place(panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -138), new Vector2(420, 160));

        RectTransform coin = Child(panel, "Coin");
        SetImage(coin.gameObject, "bubble_sun", sliced: false).preserveAspect = true;
        Place(coin, new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -20), new Vector2(78, 78));

        TMP_Text coinText = TextChild(coin, "Text");
        SetText(coinText, fredokaBold, 28, Ink, TextAlignmentOptions.Center, "R$");
        Stretch(Rect(coinText));

        TMP_Text money = Ref<TMP_Text>(budget, "budgetText");

        if (money != null)
        {
            SetText(money, fredokaBold, 52, Ink, TextAlignmentOptions.MidlineLeft, "5.000");
            Place(Rect(money), new Vector2(0, 1), new Vector2(0, 1), new Vector2(116, -12), new Vector2(200, 58));
        }

        TMP_Text initial = TextChild(panel, "Initial");
        SetText(initial, nunitoExtraBold, 22, Muted, TextAlignmentOptions.MidlineLeft, "de 5.000");
        Place(Rect(initial), new Vector2(0, 1), new Vector2(0, 1), new Vector2(118, -68), new Vector2(200, 26));

        // Nota atual (estrelas no canto)
        RectTransform rating = Child(panel, "Rating");
        Place(rating, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -22), new Vector2(112, 34));

        HorizontalLayoutGroup ratingLayout = Comp<HorizontalLayoutGroup>(rating.gameObject);
        ratingLayout.spacing = 4;
        ratingLayout.childAlignment = TextAnchor.MiddleRight;
        ratingLayout.childControlWidth = false;
        ratingLayout.childControlHeight = false;
        ratingLayout.childForceExpandWidth = false;
        ratingLayout.childForceExpandHeight = false;

        Image[] stars = new Image[3];

        for (int i = 0; i < 3; i++)
        {
            RectTransform star = Child(rating, "Star " + (i + 1));
            stars[i] = SetImage(star.gameObject, "star", Sun, sliced: false);
            stars[i].preserveAspect = true;
            star.sizeDelta = new Vector2(34, 34);
        }

        // Barra do saldo
        RectTransform bar = Child(panel, "Bar");
        bar.anchorMin = new Vector2(0, 0);
        bar.anchorMax = new Vector2(1, 0);
        bar.pivot = new Vector2(0.5f, 0);
        bar.offsetMin = new Vector2(116, 46);
        bar.offsetMax = new Vector2(-24, 70);
        bar.localRotation = Quaternion.identity;
        bar.localScale = Vector3.one;
        SetImage(bar.gameObject, "slider_track");

        RectTransform fillArea = Child(bar, "Fill Area");
        Stretch(fillArea, 5, 5, 5, 5);

        RectTransform fill = Child(fillArea, "Fill");
        Stretch(fill);
        fill.anchorMax = new Vector2(0.77f, 1);
        SetImage(fill.gameObject, "slider_fill", Green);

        RectTransform threeTick = MakeTick(bar, "Tick 3", "3", 0.35f);
        RectTransform twoTick = MakeTick(bar, "Tick 2", "2", 0.15f);

        SerializedObject so = new SerializedObject(budget);
        so.FindProperty("showCurrencyPrefix").boolValue = false;
        so.FindProperty("scoreCalculator").objectReferenceValue = FindInScene<LevelScoreCalculator>();
        so.FindProperty("initialBudgetText").objectReferenceValue = initial;
        so.FindProperty("barFill").objectReferenceValue = fill;
        so.FindProperty("threeStarTick").objectReferenceValue = threeTick;
        so.FindProperty("twoStarTick").objectReferenceValue = twoTick;
        so.FindProperty("shakeTarget").objectReferenceValue = panel;

        SerializedProperty starsProperty = so.FindProperty("ratingStars");
        starsProperty.arraySize = stars.Length;

        for (int i = 0; i < stars.Length; i++)
            starsProperty.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Marca vertical na barra com "3★" / "2★" embaixo
    private static RectTransform MakeTick(RectTransform bar, string name, string digit, float position)
    {
        RectTransform tick = Child(bar, name);
        tick.anchorMin = new Vector2(position, 0);
        tick.anchorMax = new Vector2(position, 1);
        tick.pivot = new Vector2(0.5f, 0.5f);
        tick.sizeDelta = new Vector2(5, 14);
        tick.anchoredPosition = Vector2.zero;
        tick.localRotation = Quaternion.identity;
        tick.localScale = Vector3.one;

        Image line = Comp<Image>(tick.gameObject);
        line.sprite = null;
        line.color = Ink;
        line.raycastTarget = false;

        RectTransform label = Child(tick, "Label");
        Place(label, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, -2), new Vector2(40, 20));

        HorizontalLayoutGroup layout = Comp<HorizontalLayoutGroup>(label.gameObject);
        layout.spacing = 1;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        TMP_Text text = TextChild(label, "Digit");
        SetText(text, nunitoExtraBold, 18, Muted, TextAlignmentOptions.Center, digit);
        Rect(text).sizeDelta = new Vector2(12, 20);

        RectTransform star = Child(label, "Star");
        SetImage(star.gameObject, "star", Muted, sliced: false).preserveAspect = true;
        star.sizeDelta = new Vector2(17, 17);

        return tick;
    }

    private static void StyleTools(ToolSelectionUI tools)
    {
        RectTransform panel = Rect(tools);
        Place(panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -336), new Vector2(330, 112 * 4 + 16 * 3));

        VerticalLayoutGroup layout = Comp<VerticalLayoutGroup>(panel.gameObject);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset();

        TMP_Text waterPrice = StyleToolButton(Ref<Image>(tools, "waterButton"), "Água", "drop", WaterColor, Color.white, "1", "R$ 100");
        TMP_Text powerPrice = StyleToolButton(Ref<Image>(tools, "electricityButton"), "Luz", "bolt", PowerColor, Ink, "2", "R$ 150");
        TMP_Text sewagePrice = StyleToolButton(Ref<Image>(tools, "sewageButton"), "Esgoto", "sewer", SewageColor, Ink, "3", "R$ 200");
        StyleToolButton(Ref<Image>(tools, "eraserButton"), "Apagar", "eraser", EraserColor, Ink, "4", "devolve");

        SerializedObject so = new SerializedObject(tools);
        so.FindProperty("normalSprite").objectReferenceValue = LoadSprite("btn_cream");
        so.FindProperty("selectedSprite").objectReferenceValue = LoadSprite("btn_tool_selected");
        so.FindProperty("lockedColor").colorValue = new Color(0.78f, 0.78f, 0.78f, 0.6f);
        so.FindProperty("budgetManager").objectReferenceValue = FindInScene<BudgetManager>();
        so.FindProperty("waterPrice").objectReferenceValue = waterPrice;
        so.FindProperty("electricityPrice").objectReferenceValue = powerPrice;
        so.FindProperty("sewagePrice").objectReferenceValue = sewagePrice;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static TMP_Text StyleToolButton(Image image, string label, string icon,
        Color bubbleColor, Color iconColor, string key, string price)
    {
        if (image == null)
            return null;

        Button button = image.GetComponent<Button>();

        SetImage(image.gameObject, "btn_cream", raycast: true);

        if (button != null)
        {
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { pressedSprite = LoadSprite("btn_cream_pressed") };
        }

        LayoutElement element = Comp<LayoutElement>(image.gameObject);
        element.preferredHeight = 112;
        element.minHeight = 112;

        RectTransform content = Child(image.transform, "Content");
        Stretch(content, 16, 13, 16, 0);

        // Bolha com a cor do cano
        RectTransform bubble = Child(content, "Bubble");
        SetImage(bubble.gameObject, "bubble_white", bubbleColor, sliced: false).preserveAspect = true;
        Place(bubble, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(72, 72));

        RectTransform bubbleIcon = Child(bubble, "Icon");
        SetImage(bubbleIcon.gameObject, "icon_" + icon, iconColor, sliced: false).preserveAspect = true;
        Place(bubbleIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40, 40));

        // Nome (o texto que já existia no botão)
        TMP_Text name = image.GetComponentsInChildren<TMP_Text>(true)
                            .FirstOrDefault(t => t.name == "Name" || t.name == "Text (TMP)")
                        ?? TextChild(content, "Name");

        name.name = "Name";
        name.transform.SetParent(content, false);
        SetText(name, fredokaSemiBold, 36, Ink, TextAlignmentOptions.BottomLeft, label);
        Place(Rect(name), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(92, -2), new Vector2(170, 44));

        TMP_Text priceText = TextChild(content, "Price");
        SetText(priceText, nunitoExtraBold, 22, Muted, TextAlignmentOptions.TopLeft, price);
        Place(Rect(priceText), new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(93, -6), new Vector2(170, 30));

        // Tecla de atalho
        RectTransform keyBadge = Child(content, "Key");
        SetImage(keyBadge.gameObject, "bubble_white", sliced: false).preserveAspect = true;
        Place(keyBadge, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-2, -8), new Vector2(40, 40));

        TMP_Text keyText = TextChild(keyBadge, "Text");
        SetText(keyText, nunitoExtraBold, 22, Ink, TextAlignmentOptions.Center, key);
        Stretch(Rect(keyText), 0, 2, 0, 0);

        // Cadeado (aparece quando o tutorial bloqueia a ferramenta)
        RectTransform lockIcon = Child(content, "Lock");
        SetImage(lockIcon.gameObject, "icon_lock", Ink, sliced: false).preserveAspect = true;
        Place(lockIcon, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(38, 38));
        lockIcon.gameObject.SetActive(false);

        UIButtonFeel feel = Comp<UIButtonFeel>(image.gameObject);
        SerializedObject so = new SerializedObject(feel);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("idlePulse").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();

        return price.StartsWith("R$") ? priceText : null;
    }

    private static void StyleProgress(LevelProgressUI progress)
    {
        RectTransform panel = Rect(progress);
        SetImage(panel.gameObject, "btn_cream");
        Place(panel, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -156), new Vector2(360, 112));

        TMP_Text text = Ref<TMP_Text>(progress, "progressText");

        if (text != null)
        {
            SetText(text, fredokaBold, 40, Ink, TextAlignmentOptions.MidlineRight, "0/3");
            Place(Rect(text), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-26, 6), new Vector2(100, 56));
        }

        // A barra antiga (esticada por escala) sai de cena
        Image oldBar = Ref<Image>(progress, "progressFill");

        if (oldBar != null)
        {
            oldBar.transform.localScale = Vector3.one;
            oldBar.gameObject.SetActive(false);
        }

        RectTransform houses = Child(panel, "Houses");
        Place(houses, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 6), new Vector2(232, 52));

        HorizontalLayoutGroup layout = Comp<HorizontalLayoutGroup>(houses.gameObject);
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        RectTransform template = Child(houses, "House Template");
        Image templateImage = SetImage(template.gameObject, "house_badge", sliced: false);
        templateImage.preserveAspect = true;
        template.sizeDelta = new Vector2(34, 34);

        SerializedObject so = new SerializedObject(progress);
        so.FindProperty("houseTemplate").objectReferenceValue = templateImage;
        so.FindProperty("suppliedHouseColor").colorValue = Green;
        so.FindProperty("missingHouseColor").colorValue = Color.white;
        so.FindProperty("compactText").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // [?] e [pausa] no canto superior direito
    private static void StyleTopRightButtons(Transform canvas, PauseManager pause, int siblingIndex)
    {
        GlossaryUI glossary = FindInScene<GlossaryUI>();
        Button legend = glossary != null ? Ref<Button>(glossary, "openButton") : null;

        if (legend != null)
        {
            StyleButton(legend, "cream", "?", null, 54);
            Place((RectTransform)legend.transform, new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-160, -30), new Vector2(100, 104));
        }

        if (pause == null)
            return;

        Button pauseButton = EnsureButton((RectTransform)canvas, "Pause Button");
        pauseButton.transform.SetSiblingIndex(siblingIndex);

        StyleButton(pauseButton, "cream", "Pausar", "pause", 44, iconOnly: true);
        Place((RectTransform)pauseButton.transform, new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-40, -30), new Vector2(100, 104));

        if (pauseButton.onClick.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(pauseButton.onClick, pause.PauseGame);
    }

    private static void StyleCostPreview(ConstructionCostPreview preview)
    {
        TMP_Text costText = Ref<TMP_Text>(preview, "costText");

        if (costText == null)
            return;

        RectTransform pill = Child(preview.transform, "Cost Pill");
        Image pillImage = SetImage(pill.gameObject, "pill_white");
        pill.anchorMin = pill.anchorMax = new Vector2(0.5f, 0.5f);
        pill.pivot = Vector2.zero;
        pill.localRotation = Quaternion.identity;
        pill.localScale = Vector3.one;

        HorizontalLayoutGroup layout = Comp<HorizontalLayoutGroup>(pill.gameObject);
        layout.padding = new RectOffset(24, 24, 14, 18);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fit = Comp<ContentSizeFitter>(pill.gameObject);
        fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        costText.transform.SetParent(pill, false);
        SetText(costText, nunitoExtraBold, 30, Ink, TextAlignmentOptions.Center, "− R$ 100");

        pillImage.enabled = false;

        SerializedObject so = new SerializedObject(preview);
        so.FindProperty("pill").objectReferenceValue = pill;
        so.FindProperty("availableColor").colorValue = Ink;
        so.FindProperty("insufficientColor").colorValue = StampRed;
        so.FindProperty("refundColor").colorValue = RefundGreen;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureCellHighlight(GridManager grid, PauseManager pause)
    {
        CellHoverHighlight highlight = FindInScene<CellHoverHighlight>();

        if (highlight == null)
        {
            GameObject go = new GameObject("Cell Hover Highlight");
            highlight = go.AddComponent<CellHoverHighlight>();
        }

        SerializedObject so = new SerializedObject(highlight);
        so.FindProperty("gridManager").objectReferenceValue = grid;
        so.FindProperty("networkBuilder").objectReferenceValue = FindInScene<NetworkBuilder>();
        so.FindProperty("budgetManager").objectReferenceValue = FindInScene<BudgetManager>();
        so.FindProperty("levelManager").objectReferenceValue = FindInScene<LevelManager>();
        so.FindProperty("pauseManager").objectReferenceValue = pause;
        so.FindProperty("sprite").objectReferenceValue = LoadSprite("cell_hover");
        so.FindProperty("insufficientColor").colorValue = StampRed;
        so.ApplyModifiedPropertiesWithoutUndo();
    }


    // ================================================================
    // Peças reutilizáveis
    // ================================================================

    // Painel modal: fundo escuro (opcional), card centralizado, faixa com título e animação de entrada
    private static void MakeModal(GameObject panel, RectTransform card, Vector2 size,
        bool withDim, TMP_Text title, string titleText)
    {
        Stretch((RectTransform)panel.transform);

        if (withDim)
            AddDim(panel.transform);

        card.SetParent(panel.transform, false);
        SetImage(card.gameObject, "card", raycast: true);
        Place(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);

        // Tudo o que estava solto no painel vai para dentro do card
        foreach (Transform child in panel.transform.Cast<Transform>().ToList())
        {
            if (child != card && child.name != "Dim")
                child.SetParent(card, false);
        }

        if (title == null)
            title = TextChild(Child(card, "Ribbon"), "Title");

        MakeRibbon(card, title, titleText);

        UIPanelPop pop = Comp<UIPanelPop>(panel);
        SetRef(pop, "card", card);
    }

    private static void MakeRibbon(RectTransform card, TMP_Text title, string text)
    {
        RectTransform ribbon = Child(card, "Ribbon");
        SetImage(ribbon.gameObject, "ribbon");

        title.transform.SetParent(ribbon, false);
        SetText(title, fredokaBold, 50, Color.white, TextAlignmentOptions.Center, text);
        Stretch(Rect(title), 30, 11, 30, 0);

        float width = Mathf.Max(360f, title.GetPreferredValues(text).x + 110f);
        Place(ribbon, new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, 6), new Vector2(width, 96));
        ribbon.SetAsLastSibling();
    }

    private static void AddDim(Transform panel)
    {
        // O fundo escuro precisa cobrir a tela toda: estica os objetos
        // "organizadores" entre o painel e o Canvas (ex.: "Pause" tinha 100x100)
        StretchEmptyAncestors(panel);

        RectTransform dim = Child(panel, "Dim");
        dim.SetSiblingIndex(0);
        Stretch(dim);

        Image image = Comp<Image>(dim.gameObject);
        image.sprite = null;
        image.color = DimColor;
        image.raycastTarget = true;
    }

    private static void StretchEmptyAncestors(Transform panel)
    {
        for (Transform t = panel.parent; t != null; t = t.parent)
        {
            if (t.GetComponent<Canvas>() != null)
                return;

            bool isOrganizer =
                t is RectTransform &&
                t.GetComponent<Graphic>() == null &&
                t.GetComponent<LayoutGroup>() == null;

            if (isOrganizer)
            {
                Stretch((RectTransform)t);
                Report.Add("• \"" + t.name + "\" esticado para a tela toda");
            }
        }
    }

    private static void StyleButton(Button button, string kind, string label, string icon, float fontSize,
        bool alignLeft = false, bool iconOnly = false, bool iconRight = false, bool pulse = false)
    {
        if (button == null)
            return;

        Image image = SetImage(button.gameObject, "btn_" + kind, raycast: true);

        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState { pressedSprite = LoadSprite("btn_" + kind + "_pressed") };

        Color textColor = kind == "green" ? Color.white : Ink;

        // Conteúdo (ícone + texto) acima do "degrau" do sprite
        RectTransform content = Child(button.transform, "Content");
        Stretch(content, 20, 13, 20, 0);

        HorizontalLayoutGroup layout = Comp<HorizontalLayoutGroup>(content.gameObject);
        layout.childAlignment = alignLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
        layout.padding = new RectOffset(alignLeft ? 10 : 0, 0, 0, 0);
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        TMP_Text text = button.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault()
                        ?? TextChild(content, "Label");

        text.transform.SetParent(content, false);
        SetText(text, fredokaSemiBold, fontSize, textColor, TextAlignmentOptions.Center, label);
        text.gameObject.SetActive(!iconOnly);

        Transform oldIcon = content.Find("Icon");

        if (icon != null)
        {
            RectTransform iconRect = Child(content, "Icon");
            Image iconImage = SetImage(iconRect.gameObject, "icon_" + icon, textColor, sliced: false);
            iconImage.preserveAspect = true;

            LayoutElement element = Comp<LayoutElement>(iconRect.gameObject);
            element.preferredWidth = element.preferredHeight = fontSize * 1.05f;

            if (iconRight)
                iconRect.SetAsLastSibling();
            else
                iconRect.SetAsFirstSibling();
        }
        else if (oldIcon != null)
        {
            Object.DestroyImmediate(oldIcon.gameObject);
        }

        UIButtonFeel feel = Comp<UIButtonFeel>(button.gameObject);

        SerializedObject so = new SerializedObject(feel);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("idlePulse").boolValue = pulse;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Button EnsureButton(RectTransform parent, string name)
    {
        Transform existing = parent.Find(name);

        if (existing != null)
            return existing.GetComponent<Button>();

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        return go.GetComponent<Button>();
    }


    // ================================================================
    // Utilitários
    // ================================================================

    private static T FindInScene<T>() where T : Object
    {
        return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault();
    }

    private static T Ref<T>(Object owner, string field) where T : Object
    {
        SerializedProperty property = new SerializedObject(owner).FindProperty(field);
        return property != null ? property.objectReferenceValue as T : null;
    }

    private static void SetRef(Object owner, string field, Object value)
    {
        SerializedObject so = new SerializedObject(owner);
        SerializedProperty property = so.FindProperty(field);

        if (property == null)
            return;

        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(MassinhaSpriteImporter.SpriteFolder + "/" + name + ".png");
    }

    private static T Comp<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static RectTransform Rect(Component c) => (RectTransform)c.transform;

    private static RectTransform Child(Transform parent, string name)
    {
        Transform existing = parent.Find(name);

        if (existing != null)
            return (RectTransform)existing;

        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);

        return (RectTransform)go.transform;
    }

    private static TMP_Text TextChild(Transform parent, string name)
    {
        RectTransform rect = Child(parent, name);
        return Comp<TextMeshProUGUI>(rect.gameObject);
    }

    private static TMP_Text FindText(Transform root, string name)
    {
        return root.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == name);
    }

    private static Image SetImage(GameObject go, string sprite, Color? color = null,
        bool sliced = true, bool raycast = false)
    {
        Image image = Comp<Image>(go);
        image.sprite = LoadSprite(sprite);
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.pixelsPerUnitMultiplier = 1f;
        image.color = color ?? Color.white;
        image.raycastTarget = raycast;
        return image;
    }

    private static void SetText(TMP_Text text, TMP_FontAsset font, float size, Color color,
        TextAlignmentOptions alignment, string value = null)
    {
        text.font = font;
        text.fontSharedMaterial = font.material;
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.fontStyle = FontStyles.Normal;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.margin = Vector4.zero;
        text.raycastTarget = false;

        if (value != null)
            text.text = value;
    }

    private static void Stretch(RectTransform rect, float left = 0, float bottom = 0, float right = 0, float top = 0)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position,
        Vector2 size, float rotationZ = 0f)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localRotation = Quaternion.Euler(0, 0, rotationZ);
        rect.localScale = Vector3.one;
    }

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}
