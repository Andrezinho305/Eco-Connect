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

    private void OnEnable()
    {
        if (networkBuilder != null)
            networkBuilder.OnToolChanged += UpdateSelection;
    }

    private void Start()
    {
        UpdateSelection();
    }

    private void OnDisable()
    {
        if (networkBuilder != null)
            networkBuilder.OnToolChanged -= UpdateSelection;
    }

    private void UpdateSelection()
    {
        UpdateButtonState(waterButton, ToolMask.Water);
        UpdateButtonState(electricityButton, ToolMask.Electricity);
        UpdateButtonState(sewageButton, ToolMask.Sewage);
        UpdateButtonState(eraserButton, ToolMask.Eraser);

        if (!networkBuilder.HasToolSelected)
            return;

        if (networkBuilder.IsEraseMode)
        {
            eraserButton.color = selectedColor;
            return;
        }

        switch (networkBuilder.SelectedNetwork)
        {
            case NetworkType.Water:
                waterButton.color = selectedColor;
                break;

            case NetworkType.Electricity:
                electricityButton.color = selectedColor;
                break;

            case NetworkType.Sewage:
                sewageButton.color = selectedColor;
                break;
        }
    }

    // Aplica a cor normal ou bloqueada e desativa o clique se bloqueado
    private void UpdateButtonState(Image buttonImage, ToolMask tool)
    {
        bool allowed = networkBuilder.IsToolAllowed(tool);

        buttonImage.color = allowed ? normalColor : lockedColor;

        Button button = buttonImage.GetComponent<Button>();

        if (button != null)
            button.interactable = allowed;
    }
}

