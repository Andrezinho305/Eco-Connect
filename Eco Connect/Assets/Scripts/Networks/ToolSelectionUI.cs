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
        waterButton.color = normalColor;
        electricityButton.color = normalColor;
        sewageButton.color = normalColor;
        eraserButton.color = normalColor;

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
}

