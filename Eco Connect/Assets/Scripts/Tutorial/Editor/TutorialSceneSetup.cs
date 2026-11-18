using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Monta o tutorial na cena aberta: cria o Tutorial Manager, o painel de
// instruções e liga todas as referências automaticamente.
public static class TutorialSceneSetup
{
    private const string MenuRoot = "EcoConnect/Tutorial/";

    [MenuItem(MenuRoot + "Montar Tutorial na Cena")]
    private static void SetupTutorial()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (Object.FindAnyObjectByType<TutorialManager>(
                FindObjectsInactive.Include) != null)
        {
            EditorUtility.DisplayDialog(
                "Tutorial",
                "Esta cena já possui um Tutorial Manager.",
                "OK"
            );
            return;
        }

        if (!scene.name.Contains("Tutorial") &&
            !EditorUtility.DisplayDialog(
                "Tutorial",
                "A cena aberta (" + scene.name + ") não parece ser a " +
                "cena do tutorial. Montar mesmo assim?",
                "Montar", "Cancelar"))
        {
            return;
        }

        // ---------- Referências existentes ----------
        NetworkBuilder builder = Find<NetworkBuilder>();
        GridManager grid = Find<GridManager>();
        NetworkConnectivity connectivity = Find<NetworkConnectivity>();
        LevelManager levelManager = Find<LevelManager>();
        PauseManager pauseManager = Find<PauseManager>();

        if (builder == null || grid == null ||
            connectivity == null || levelManager == null)
        {
            EditorUtility.DisplayDialog(
                "Tutorial",
                "Não encontrei NetworkBuilder, GridManager, " +
                "NetworkConnectivity ou LevelManager na cena.",
                "OK"
            );
            return;
        }

        RectTransform waterButton = FindRect("Water Button");
        RectTransform electricityButton = FindRect("Electricity Button");
        RectTransform sewageButton = FindRect("Sewage Button");
        RectTransform eraserButton = FindRect("Eraser Button");
        RectTransform budgetPanel = FindRect("Budget Panel");
        RectTransform progressPanel = FindRect("Progress Panel");
        RectTransform starPanel = FindRect("Star Pannel");

        Canvas canvas = waterButton != null
            ? waterButton.GetComponentInParent<Canvas>(true).rootCanvas
            : Find<Canvas>();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Tutorial", "Nenhum Canvas encontrado na cena.", "OK");
            return;
        }

        // ---------- Painel de instruções ----------
        GameObject panel = CreateUIObject("Tutorial Panel", canvas.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        // Lado direito da tela, fora do tabuleiro
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-30f, 0f);
        panelRect.sizeDelta = new Vector2(460f, 400f);

        // Canvas próprio para ficar por cima da tela de vitória
        Canvas panelCanvas = panel.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 20;
        panel.AddComponent<GraphicRaycaster>();

        Image background = panel.AddComponent<Image>();
        background.sprite = GetBuiltinSprite("UI/Skin/UISprite.psd");
        background.type = Image.Type.Sliced;
        background.color = new Color(0.07f, 0.12f, 0.1f, 0.92f);

        // Contador de passos
        TMP_Text counter = CreateText(
            "Step Counter", panel.transform, "Passo 1/11", 22f,
            new Color(0.7f, 0.85f, 0.75f));

        RectTransform counterRect = counter.rectTransform;
        counterRect.anchorMin = new Vector2(0f, 1f);
        counterRect.anchorMax = new Vector2(1f, 1f);
        counterRect.pivot = new Vector2(0.5f, 1f);
        counterRect.offsetMin = new Vector2(25f, -55f);
        counterRect.offsetMax = new Vector2(-25f, -15f);

        // Mensagem
        TMP_Text message = CreateText(
            "Message", panel.transform, "Mensagem do tutorial", 27f,
            Color.white);

        message.alignment = TextAlignmentOptions.TopLeft;
        message.textWrappingMode = TextWrappingModes.Normal;
        message.enableAutoSizing = true;
        message.fontSizeMin = 18f;
        message.fontSizeMax = 27f;

        RectTransform messageRect = message.rectTransform;
        messageRect.anchorMin = Vector2.zero;
        messageRect.anchorMax = Vector2.one;
        messageRect.offsetMin = new Vector2(25f, 95f);
        messageRect.offsetMax = new Vector2(-25f, -65f);

        // Botões
        Button skipButton = CreateButton(
            "Skip Button", panel.transform, "Pular tutorial",
            new Color(0.35f, 0.35f, 0.35f),
            new Vector2(0f, 0f), new Vector2(25f, 22f),
            new Vector2(180f, 55f), out _);

        Button continueButton = CreateButton(
            "Continue Button", panel.transform, "Continuar",
            new Color(0.2f, 0.65f, 0.35f),
            new Vector2(1f, 0f), new Vector2(-25f, 22f),
            new Vector2(190f, 55f), out TMP_Text continueLabel);

        TutorialUI tutorialUI = panel.AddComponent<TutorialUI>();

        Undo.RegisterCreatedObjectUndo(panel, "Montar Tutorial");

        // ---------- Tutorial Manager ----------
        GameObject managerObject = new GameObject("Tutorial Manager");
        Undo.RegisterCreatedObjectUndo(managerObject, "Montar Tutorial");

        TutorialHighlighter highlighter =
            managerObject.AddComponent<TutorialHighlighter>();

        TutorialManager manager =
            managerObject.AddComponent<TutorialManager>();

        // ---------- Ligações ----------
        SerializedObject uiSO = new SerializedObject(tutorialUI);
        Set(uiSO, "panel", panel);
        Set(uiSO, "messageText", message);
        Set(uiSO, "stepCounterText", counter);
        Set(uiSO, "continueButton", continueButton);
        Set(uiSO, "continueButtonLabel", continueLabel);
        Set(uiSO, "skipButton", skipButton);
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject managerSO = new SerializedObject(manager);
        Set(managerSO, "networkBuilder", builder);
        Set(managerSO, "gridManager", grid);
        Set(managerSO, "connectivity", connectivity);
        Set(managerSO, "levelManager", levelManager);
        Set(managerSO, "pauseManager", pauseManager);
        Set(managerSO, "tutorialUI", tutorialUI);
        Set(managerSO, "highlighter", highlighter);
        Set(managerSO, "waterButton", waterButton);
        Set(managerSO, "electricityButton", electricityButton);
        Set(managerSO, "sewageButton", sewageButton);
        Set(managerSO, "eraserButton", eraserButton);
        Set(managerSO, "budgetPanel", budgetPanel);
        Set(managerSO, "progressPanel", progressPanel);
        Set(managerSO, "starPanel", starPanel);
        managerSO.ApplyModifiedPropertiesWithoutUndo();

        // ---------- Desativa a dica antiga ----------
        RectTransform oldTutorialText = FindRect("Tutorial Text");

        if (oldTutorialText != null)
        {
            Undo.RecordObject(oldTutorialText.gameObject, "Montar Tutorial");
            oldTutorialText.gameObject.SetActive(false);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = managerObject;

        string missing = "";
        if (waterButton == null) missing += "\n• Water Button";
        if (electricityButton == null) missing += "\n• Electricity Button";
        if (sewageButton == null) missing += "\n• Sewage Button";
        if (eraserButton == null) missing += "\n• Eraser Button";
        if (budgetPanel == null) missing += "\n• Budget Panel";
        if (starPanel == null) missing += "\n• Star Pannel";
        if (pauseManager == null) missing += "\n• PauseManager";

        EditorUtility.DisplayDialog(
            "Tutorial",
            "Tutorial montado! Salve a cena (Ctrl+S)." +
            (missing.Length > 0
                ? "\n\nNão encontrei (ligue manualmente no Tutorial Manager):" + missing
                : ""),
            "OK"
        );
    }


    [MenuItem(MenuRoot + "Resetar Tutorial Concluído")]
    private static void ResetTutorialCompleted()
    {
        PlayerPrefs.DeleteKey(TutorialManager.CompletedKey);
        PlayerPrefs.Save();

        Debug.Log("Tutorial marcado como não concluído.");
    }


    // ---------- Utilidades ----------

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

    private static Sprite GetBuiltinSprite(string path)
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.layer = LayerMask.NameToLayer("UI");
        obj.transform.SetParent(parent, false);
        return obj;
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

        Image image = obj.AddComponent<Image>();
        image.sprite = GetBuiltinSprite("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = color;

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;

        labelText = CreateText("Label", obj.transform, label, 24f, Color.white);
        labelText.alignment = TextAlignmentOptions.Center;

        RectTransform labelRect = labelText.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }
}
