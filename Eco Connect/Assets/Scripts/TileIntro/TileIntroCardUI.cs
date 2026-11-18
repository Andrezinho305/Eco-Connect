using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Card "Novo elemento!" mostrado no início das fases
public class TileIntroCardUI : MonoBehaviour
{
    [Header("Raiz (bloqueia cliques no tabuleiro)")]
    [SerializeField] private GameObject root;

    [Header("Conteúdo")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pageText;

    [Header("Botões")]
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text nextButtonLabel;
    [SerializeField] private Button skipButton;

    [Header("Textos dos botões")]
    [SerializeField] private string nextLabel = "Próximo";
    [SerializeField] private string lastLabel = "Entendi";

    public event Action OnNextClicked;
    public event Action OnSkipClicked;

    private void Awake()
    {
        nextButton.onClick.AddListener(HandleNext);

        if (skipButton != null)
            skipButton.onClick.AddListener(HandleSkip);
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(HandleNext);

        if (skipButton != null)
            skipButton.onClick.RemoveListener(HandleSkip);
    }

    public void Show(
        string title,
        string description,
        Sprite sprite,
        int page,
        int totalPages)
    {
        titleText.text = title;
        descriptionText.text = description;

        icon.sprite = sprite;
        icon.enabled = sprite != null;
        icon.preserveAspect = true;

        bool hasManyPages = totalPages > 1;

        if (pageText != null)
        {
            pageText.gameObject.SetActive(hasManyPages);
            pageText.text = page + "/" + totalPages;
        }

        // Com um card só, "Pular" faria o mesmo que "Entendi"
        if (skipButton != null)
            skipButton.gameObject.SetActive(hasManyPages);

        if (nextButtonLabel != null)
            nextButtonLabel.text =
                page >= totalPages ? lastLabel : nextLabel;

        SetVisible(true);
    }

    public void SetVisible(bool visible)
    {
        if (root.activeSelf != visible)
            root.SetActive(visible);
    }

    private void HandleNext()
    {
        OnNextClicked?.Invoke();
    }

    private void HandleSkip()
    {
        OnSkipClicked?.Invoke();
    }
}
