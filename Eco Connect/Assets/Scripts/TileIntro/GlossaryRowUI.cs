using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Uma linha da Legenda: ícone, nome e descrição
public class GlossaryRowUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    public void Setup(string title, string description, Sprite sprite)
    {
        titleText.text = title;
        descriptionText.text = description;

        icon.sprite = sprite;
        icon.enabled = sprite != null;
        icon.preserveAspect = true;
    }
}
