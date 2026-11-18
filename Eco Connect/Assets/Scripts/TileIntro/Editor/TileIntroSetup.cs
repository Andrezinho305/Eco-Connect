using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Configura os cards "Novo elemento!" e a Legenda:
// 1. cria os assets dos elementos e o catálogo;
// 2. marca os elementos novos no Level01 e Level02;
// 3. monta a interface em todas as fases do Build Settings.
public static class TileIntroSetup
{
    private const string AssetFolder = "Assets/Tile Intros";
    private const string CatalogPath = AssetFolder + "/TileIntroCatalog.asset";

    [MenuItem("EcoConnect/Elementos/Configurar Elementos e Legenda")]
    private static void SetupAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string originalScene = SceneManager.GetActiveScene().path;

        // ---------- 1. Assets ----------
        if (!AssetDatabase.IsValidFolder(AssetFolder))
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(AssetFolder));

        TileIntroData waterSource = CreateElement(
            "01_CaixaDagua", "Caixa d'água",
            "Fonte de água. Ligue canos de água a ela para abastecer as casas.",
            StructureType.WaterSource);

        TileIntroData powerSource = CreateElement(
            "02_Usina", "Usina",
            "Fonte de energia. Ligue a rede elétrica a ela para abastecer as casas.",
            StructureType.PowerSource);

        TileIntroData sewage = CreateElement(
            "03_EstacaoTratamento", "Estação de tratamento",
            "Recebe o esgoto das casas. Ligue a rede de esgoto a ela.",
            StructureType.SewageTreatment);

        TileIntroData house = CreateElement(
            "04_Casa", "Casa",
            "Precisa de um ou mais serviços. Os indicadores mostram o que " +
            "falta: vermelho = falta, verde = abastecida.",
            StructureType.Building);

        TileIntroData multiHouse = CreateElement(
            "05_CasaMultipla", "Casas com várias necessidades",
            "Algumas casas precisam de mais de um serviço. Elas só ficam " +
            "completas quando TODOS os indicadores estiverem verdes.",
            StructureType.Building,
            onlyMultiService: true);

        TileIntroData vegetation = CreateElement(
            "06_Vegetacao", "Vegetação",
            "Áreas verdes protegidas. Não é possível construir redes sobre " +
            "elas, então contorne-as!",
            StructureType.Obstacle, matchVariant: true, variant: 0);

        TileIntroData rock = CreateElement(
            "07_Rocha", "Rocha",
            "Rochas são duras demais para escavar. Não é possível construir " +
            "redes sobre elas: encontre outro caminho!",
            StructureType.Obstacle, matchVariant: true, variant: 1);

        TileIntroCatalog catalog = CreateCatalog(new List<TileIntroData>
        {
            waterSource, powerSource, sewage,
            house, multiHouse, vegetation, rock
        });

        // ---------- 2. Elementos novos por fase ----------
        AssignNewElements("Level01_Data", vegetation, multiHouse);
        AssignNewElements("Level02_Data", rock);

        AssetDatabase.SaveAssets();

        // ---------- 3. Cenas ----------
        List<string> configured = new List<string>();
        List<string> skipped = new List<string>();

        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled || buildScene.path.Contains("MainMenu"))
                continue;

            Scene scene = EditorSceneManager.OpenScene(
                buildScene.path, OpenSceneMode.Single);

            if (SetupScene(catalog, out string reason))
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                configured.Add(scene.name);
            }
            else
            {
                skipped.Add(scene.name + " (" + reason + ")");
            }
        }

        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        EditorUtility.DisplayDialog(
            "Elementos e Legenda",
            "Configurado em: " +
            (configured.Count > 0 ? string.Join(", ", configured) : "nenhuma cena") +
            (skipped.Count > 0 ? "\n\nIgnorado: " + string.Join(", ", skipped) : "") +
            "\n\nOs textos ficam em " + AssetFolder +
            " e os elementos de cada fase no campo \"New Elements\" do LevelData.",
            "OK"
        );
    }


    // ---------- Assets ----------

    private static TileIntroData CreateElement(
        string fileName, string title, string description,
        StructureType type, bool matchVariant = false, int variant = 0,
        bool onlyMultiService = false)
    {
        string path = AssetFolder + "/" + fileName + ".asset";

        TileIntroData existing =
            AssetDatabase.LoadAssetAtPath<TileIntroData>(path);

        // Não sobrescreve textos já editados
        if (existing != null)
            return existing;

        TileIntroData element = ScriptableObject.CreateInstance<TileIntroData>();
        element.title = title;
        element.description = description;
        element.structureType = type;
        element.matchVariant = matchVariant;
        element.visualVariant = variant;
        element.onlyMultiServiceBuildings = onlyMultiService;

        AssetDatabase.CreateAsset(element, path);
        return element;
    }

    private static TileIntroCatalog CreateCatalog(List<TileIntroData> elements)
    {
        TileIntroCatalog catalog =
            AssetDatabase.LoadAssetAtPath<TileIntroCatalog>(CatalogPath);

        if (catalog != null)
            return catalog;

        catalog = ScriptableObject.CreateInstance<TileIntroCatalog>();
        AssetDatabase.CreateAsset(catalog, CatalogPath);

        SerializedObject so = new SerializedObject(catalog);

        so.FindProperty("visualDatabase").objectReferenceValue =
            FindAsset<StructureVisualDatabase>(null);

        SerializedProperty list = so.FindProperty("elements");
        list.arraySize = elements.Count;

        for (int i = 0; i < elements.Count; i++)
            list.GetArrayElementAtIndex(i).objectReferenceValue = elements[i];

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);

        return catalog;
    }

    private static void AssignNewElements(
        string levelDataName, params TileIntroData[] elements)
    {
        LevelData level = FindAsset<LevelData>(levelDataName);

        if (level == null)
        {
            Debug.LogWarning("LevelData não encontrado: " + levelDataName);
            return;
        }

        // Não sobrescreve uma lista já configurada
        if (level.newElements != null && level.newElements.Count > 0)
            return;

        level.newElements = new List<TileIntroData>(elements);
        EditorUtility.SetDirty(level);
    }

    private static T FindAsset<T>(string exactName) where T : Object
    {
        foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (exactName != null &&
                Path.GetFileNameWithoutExtension(path) != exactName)
                continue;

            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
                return asset;
        }

        return null;
    }


    // ---------- Cena ----------

    private static bool SetupScene(TileIntroCatalog catalog, out string reason)
    {
        reason = "";

        if (Object.FindAnyObjectByType<LevelIntroManager>(
                FindObjectsInactive.Include) != null)
        {
            reason = "já configurada";
            return false;
        }

        GridManager grid = Find<GridManager>();
        PauseManager pause = Find<PauseManager>();

        RectTransform budgetPanel = FindRect("Budget Panel");

        Canvas canvas = budgetPanel != null
            ? budgetPanel.GetComponentInParent<Canvas>(true).rootCanvas
            : Find<Canvas>();

        if (grid == null || canvas == null)
        {
            reason = "sem GridManager ou Canvas";
            return false;
        }

        GameObject managerObject = new GameObject("Level Intro & Legenda");

        TutorialHighlighter highlighter =
            managerObject.AddComponent<TutorialHighlighter>();
        TileIntroCardUI cardUI = managerObject.AddComponent<TileIntroCardUI>();
        LevelIntroManager introManager =
            managerObject.AddComponent<LevelIntroManager>();
        GlossaryUI glossary = managerObject.AddComponent<GlossaryUI>();

        BuildCard(canvas.transform, cardUI);
        BuildGlossary(canvas.transform, glossary);

        SerializedObject introSO = new SerializedObject(introManager);
        Set(introSO, "gridManager", grid);
        Set(introSO, "catalog", catalog);
        Set(introSO, "pauseManager", pause);
        Set(introSO, "cardUI", cardUI);
        Set(introSO, "highlighter", highlighter);
        introSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject glossarySO = new SerializedObject(glossary);
        Set(glossarySO, "gridManager", grid);
        Set(glossarySO, "catalog", catalog);
        Set(glossarySO, "pauseManager", pause);
        glossarySO.ApplyModifiedPropertiesWithoutUndo();

        return true;
    }

    private static void BuildCard(Transform canvas, TileIntroCardUI cardUI)
    {
        // Camada que bloqueia cliques no tabuleiro enquanto o card está aberto
        GameObject overlay = CreateUIObject("Tile Intro Overlay", canvas);
        Stretch(overlay.GetComponent<RectTransform>());
        AddOwnCanvas(overlay, 30);

        Image dim = overlay.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.2f);

        // Card no lado direito, para o tabuleiro continuar visível
        GameObject card = CreateUIObject("Card", overlay.transform);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = cardRect.anchorMax = new Vector2(1f, 0.5f);
        cardRect.pivot = new Vector2(1f, 0.5f);
        cardRect.anchoredPosition = new Vector2(-30f, 0f);
        cardRect.sizeDelta = new Vector2(460f, 580f);
        AddBackground(card, new Color(0.07f, 0.12f, 0.1f, 0.95f));

        TMP_Text header = CreateText("Header", card.transform,
            "NOVO ELEMENTO!", 26f, new Color(1f, 0.85f, 0.1f));
        header.fontStyle = FontStyles.Bold;
        header.alignment = TextAlignmentOptions.Center;
        TopBand(header.rectTransform, 20f, 40f);

        GameObject iconObject = CreateUIObject("Icon", card.transform);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 1f);
        iconRect.pivot = new Vector2(0.5f, 1f);
        iconRect.anchoredPosition = new Vector2(0f, -75f);
        iconRect.sizeDelta = new Vector2(150f, 150f);
        Image icon = iconObject.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        TMP_Text title = CreateText("Title", card.transform,
            "Elemento", 34f, Color.white);
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.enableAutoSizing = true;
        title.fontSizeMin = 22f;
        title.fontSizeMax = 34f;
        TopBand(title.rectTransform, 240f, 50f);

        TMP_Text description = CreateText("Description", card.transform,
            "Descrição", 24f, Color.white);
        description.alignment = TextAlignmentOptions.Top;
        description.textWrappingMode = TextWrappingModes.Normal;
        description.enableAutoSizing = true;
        description.fontSizeMin = 16f;
        description.fontSizeMax = 24f;
        RectTransform descRect = description.rectTransform;
        descRect.anchorMin = Vector2.zero;
        descRect.anchorMax = Vector2.one;
        descRect.offsetMin = new Vector2(25f, 95f);
        descRect.offsetMax = new Vector2(-25f, -300f);

        TMP_Text page = CreateText("Page", card.transform,
            "1/2", 22f, new Color(0.7f, 0.85f, 0.75f));
        page.alignment = TextAlignmentOptions.Center;
        RectTransform pageRect = page.rectTransform;
        pageRect.anchorMin = pageRect.anchorMax = new Vector2(0.5f, 0f);
        pageRect.pivot = new Vector2(0.5f, 0f);
        pageRect.anchoredPosition = new Vector2(0f, 30f);
        pageRect.sizeDelta = new Vector2(80f, 40f);

        Button skip = CreateButton("Skip Button", card.transform, "Pular",
            new Color(0.35f, 0.35f, 0.35f),
            new Vector2(0f, 0f), new Vector2(25f, 22f),
            new Vector2(130f, 55f), out _);

        Button next = CreateButton("Next Button", card.transform, "Entendi",
            new Color(0.2f, 0.65f, 0.35f),
            new Vector2(1f, 0f), new Vector2(-25f, 22f),
            new Vector2(160f, 55f), out TMP_Text nextLabel);

        SerializedObject so = new SerializedObject(cardUI);
        Set(so, "root", overlay);
        Set(so, "icon", icon);
        Set(so, "titleText", title);
        Set(so, "descriptionText", description);
        Set(so, "pageText", page);
        Set(so, "nextButton", next);
        Set(so, "nextButtonLabel", nextLabel);
        Set(so, "skipButton", skip);
        so.ApplyModifiedPropertiesWithoutUndo();

        overlay.SetActive(false);
    }

    private static void BuildGlossary(Transform canvas, GlossaryUI glossary)
    {
        // Botão "?" no canto superior direito
        Button openButton = CreateButton("Legenda Button", canvas, "?",
            new Color(0.15f, 0.45f, 0.7f),
            new Vector2(1f, 1f), new Vector2(-30f, -30f),
            new Vector2(70f, 70f), out TMP_Text openLabel);
        openLabel.fontSize = 44f;
        openLabel.fontStyle = FontStyles.Bold;

        // Janela
        GameObject panel = CreateUIObject("Legenda Panel", canvas);
        Stretch(panel.GetComponent<RectTransform>());
        AddOwnCanvas(panel, 30);

        Image dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject window = CreateUIObject("Window", panel.transform);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(900f, 720f);
        AddBackground(window, new Color(0.07f, 0.12f, 0.1f, 0.97f));

        TMP_Text title = CreateText("Title", window.transform,
            "Legenda", 40f, Color.white);
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        TopBand(title.rectTransform, 20f, 60f);

        Button closeButton = CreateButton("Close Button", window.transform, "X",
            new Color(0.7f, 0.25f, 0.25f),
            new Vector2(1f, 1f), new Vector2(-20f, -20f),
            new Vector2(60f, 60f), out _);

        // Lista com rolagem
        GameObject scrollObject = CreateUIObject("Scroll View", window.transform);
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(30f, 30f);
        scrollRect.offsetMax = new Vector2(-30f, -100f);
        Image scrollHitArea = scrollObject.AddComponent<Image>();
        scrollHitArea.color = Color.clear;

        GameObject viewport = CreateUIObject("Viewport", scrollObject.transform);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.AddComponent<RectMask2D>();

        GameObject content = CreateUIObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        // Modelo de linha (clonado para cada elemento)
        GameObject row = CreateUIObject("Row Template", content.transform);
        AddBackground(row, new Color(1f, 1f, 1f, 0.06f));
        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.minHeight = 120f;
        rowLayout.preferredHeight = 120f;

        GameObject rowIconObject = CreateUIObject("Icon", row.transform);
        RectTransform rowIconRect = rowIconObject.GetComponent<RectTransform>();
        rowIconRect.anchorMin = rowIconRect.anchorMax = new Vector2(0f, 0.5f);
        rowIconRect.pivot = new Vector2(0f, 0.5f);
        rowIconRect.anchoredPosition = new Vector2(10f, 0f);
        rowIconRect.sizeDelta = new Vector2(100f, 100f);
        Image rowIcon = rowIconObject.AddComponent<Image>();
        rowIcon.preserveAspect = true;
        rowIcon.raycastTarget = false;

        TMP_Text rowTitle = CreateText("Title", row.transform,
            "Elemento", 28f, Color.white);
        rowTitle.fontStyle = FontStyles.Bold;
        RectTransform rowTitleRect = rowTitle.rectTransform;
        rowTitleRect.anchorMin = new Vector2(0f, 1f);
        rowTitleRect.anchorMax = new Vector2(1f, 1f);
        rowTitleRect.pivot = new Vector2(0.5f, 1f);
        rowTitleRect.offsetMin = new Vector2(125f, -50f);
        rowTitleRect.offsetMax = new Vector2(-15f, -10f);

        TMP_Text rowDescription = CreateText("Description", row.transform,
            "Descrição", 20f, new Color(0.85f, 0.9f, 0.87f));
        rowDescription.alignment = TextAlignmentOptions.TopLeft;
        rowDescription.textWrappingMode = TextWrappingModes.Normal;
        rowDescription.enableAutoSizing = true;
        rowDescription.fontSizeMin = 14f;
        rowDescription.fontSizeMax = 20f;
        RectTransform rowDescRect = rowDescription.rectTransform;
        rowDescRect.anchorMin = Vector2.zero;
        rowDescRect.anchorMax = Vector2.one;
        rowDescRect.offsetMin = new Vector2(125f, 10f);
        rowDescRect.offsetMax = new Vector2(-15f, -50f);

        GlossaryRowUI rowUI = row.AddComponent<GlossaryRowUI>();
        SerializedObject rowSO = new SerializedObject(rowUI);
        Set(rowSO, "icon", rowIcon);
        Set(rowSO, "titleText", rowTitle);
        Set(rowSO, "descriptionText", rowDescription);
        rowSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject so = new SerializedObject(glossary);
        Set(so, "openButton", openButton);
        Set(so, "panel", panel);
        Set(so, "closeButton", closeButton);
        Set(so, "contentRoot", contentRect);
        Set(so, "rowTemplate", rowUI);
        so.ApplyModifiedPropertiesWithoutUndo();

        row.SetActive(false);
        panel.SetActive(false);
    }


    // ---------- Utilidades de UI ----------

    private static T Find<T>() where T : Object
    {
        return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
    }

    private static RectTransform FindRect(string objectName)
    {
        foreach (GameObject root in
                 SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (RectTransform rect in
                     root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.name == objectName)
                    return rect;
            }
        }

        return null;
    }

    private static void Set(SerializedObject so, string property, Object value)
    {
        SerializedProperty prop = so.FindProperty(property);

        if (prop == null)
        {
            Debug.LogWarning("Propriedade não encontrada: " + property);
            return;
        }

        prop.objectReferenceValue = value;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.layer = LayerMask.NameToLayer("UI");
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // Faixa horizontal no topo do pai
    private static void TopBand(RectTransform rect, float top, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(20f, -top - height);
        rect.offsetMax = new Vector2(-20f, -top);
    }

    // Canvas próprio para ficar acima do resto da interface
    private static void AddOwnCanvas(GameObject obj, int sortingOrder)
    {
        Canvas canvas = obj.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        obj.AddComponent<GraphicRaycaster>();
    }

    private static Image AddBackground(GameObject obj, Color color)
    {
        Image image = obj.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(
            "UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(
        string name, Transform parent, string text,
        float fontSize, Color color)
    {
        GameObject obj = CreateUIObject(name, parent);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.raycastTarget = false;

        return tmp;
    }

    private static Button CreateButton(
        string name, Transform parent, string label, Color color,
        Vector2 anchor, Vector2 position, Vector2 size,
        out TMP_Text labelText)
    {
        GameObject obj = CreateUIObject(name, parent);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = AddBackground(obj, color);

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;

        labelText = CreateText("Label", obj.transform, label, 24f, Color.white);
        labelText.alignment = TextAlignmentOptions.Center;
        Stretch(labelText.rectTransform);

        return button;
    }
}
