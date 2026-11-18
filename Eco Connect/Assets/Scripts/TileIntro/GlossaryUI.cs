using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Botão "?" que abre a Legenda com os elementos presentes na fase
public class GlossaryUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TileIntroCatalog catalog;
    [SerializeField] private PauseManager pauseManager;

    [Header("Interface")]
    [SerializeField] private Button openButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private GlossaryRowUI rowTemplate;

    private bool isOpen;

    private void Awake()
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        openButton.onClick.RemoveListener(Open);
        closeButton.onClick.RemoveListener(Close);

        OverlayEscape.Unregister(HandleEscape);
    }

    private void Start()
    {
        panel.SetActive(false);
        rowTemplate.gameObject.SetActive(false);

        BuildRows();
    }

    private void Update()
    {
        // Fecha se o jogo for pausado
        if (isOpen && pauseManager != null && pauseManager.IsPaused)
            Close();
    }


    public void Open()
    {
        if (pauseManager != null && pauseManager.IsPaused)
            return;

        if (!isOpen)
            SFXManager.Play(Sfx.PanelOpen);

        isOpen = true;
        panel.SetActive(true);

        OverlayEscape.Register(HandleEscape);
    }

    public void Close()
    {
        if (isOpen)
            SFXManager.Play(Sfx.PanelClose);

        isOpen = false;
        panel.SetActive(false);

        OverlayEscape.Unregister(HandleEscape);
    }


    private void BuildRows()
    {
        if (catalog == null)
            return;

        List<TileIntroData> elements =
            catalog.GetElementsPresent(gridManager);

        foreach (TileIntroData element in elements)
        {
            GlossaryRowUI row = Instantiate(rowTemplate, contentRoot);

            row.gameObject.name = "Row - " + element.title;
            row.gameObject.SetActive(true);

            row.Setup(
                element.title,
                element.description,
                catalog.GetIcon(element)
            );
        }
    }

    private bool HandleEscape()
    {
        if (!isOpen)
            return false;

        Close();
        return true;
    }
}
