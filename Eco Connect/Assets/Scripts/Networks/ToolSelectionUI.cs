using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolSelectionUI : MonoBehaviour
{
    [SerializeField] private NetworkBuilder networkBuilder;

    [Header("Imagens dos botões")]
    [SerializeField] private Image waterButton;
    [SerializeField] private Image electricityButton;
    [SerializeField] private Image sewageButton;
    [SerializeField] private Image eraserButton;

    [Header("Cores")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

    [Header("Tema (opcional)")]
    [Tooltip("Com sprites definidos, a seleção troca o sprite em vez de pintar o botão")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    [Tooltip("Preço mostrado em cada botão")]
    [SerializeField] private BudgetManager budgetManager;
    [SerializeField] private TMP_Text waterPrice;
    [SerializeField] private TMP_Text electricityPrice;
    [SerializeField] private TMP_Text sewagePrice;

    private bool UsesSprites => normalSprite != null && selectedSprite != null;

    private void OnEnable()
    {
        if (networkBuilder != null)
            networkBuilder.OnToolChanged += UpdateSelection;
    }

    private void Start()
    {
        UpdatePrices();
        UpdateSelection();
    }

    private void OnDisable()
    {
        if (networkBuilder != null)
            networkBuilder.OnToolChanged -= UpdateSelection;
    }

    private void UpdatePrices()
    {
        if (budgetManager == null)
            return;

        SetPrice(waterPrice, NetworkType.Water);
        SetPrice(electricityPrice, NetworkType.Electricity);
        SetPrice(sewagePrice, NetworkType.Sewage);
    }

    private void SetPrice(TMP_Text label, NetworkType network)
    {
        if (label != null)
            label.text = "R$ " + budgetManager.GetNetworkCost(network);
    }

    private void UpdateSelection()
    {
        bool hasTool = networkBuilder.HasToolSelected;
        bool erase = networkBuilder.IsEraseMode;
        NetworkType network = networkBuilder.SelectedNetwork;

        UpdateButtonState(waterButton, ToolMask.Water,
            hasTool && !erase && network == NetworkType.Water);

        UpdateButtonState(electricityButton, ToolMask.Electricity,
            hasTool && !erase && network == NetworkType.Electricity);

        UpdateButtonState(sewageButton, ToolMask.Sewage,
            hasTool && !erase && network == NetworkType.Sewage);

        UpdateButtonState(eraserButton, ToolMask.Eraser,
            hasTool && erase);
    }

    // Aplica o estado normal, selecionado ou bloqueado e desativa o clique se bloqueado
    private void UpdateButtonState(Image buttonImage, ToolMask tool, bool selected)
    {
        bool allowed = networkBuilder.IsToolAllowed(tool);
        selected &= allowed;

        Button button = buttonImage.GetComponent<Button>();

        if (button != null)
            button.interactable = allowed;

        if (UsesSprites)
        {
            buttonImage.sprite = selected ? selectedSprite : normalSprite;
            buttonImage.color = allowed ? Color.white : lockedColor;

            SetChildActive(buttonImage.transform, "Lock", !allowed);
            SetChildActive(buttonImage.transform, "Key", allowed);

            UIButtonFeel feel = buttonImage.GetComponent<UIButtonFeel>();

            if (feel != null)
                feel.SetHeld(selected);
        }
        else
        {
            buttonImage.color = !allowed ? lockedColor : selected ? selectedColor : normalColor;
        }
    }

    private static void SetChildActive(Transform root, string name, bool active)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                child.gameObject.SetActive(active);
                return;
            }
        }
    }
}
