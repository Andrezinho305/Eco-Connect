using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// "Sensação" dos botões do tema Massinha:
// cresce um pouco ao passar o mouse (ou com foco do teclado)
// e o conteúdo afunda junto com o sprite pressionado.
// Funciona com o jogo pausado (usa tempo real).
[DisallowMultipleComponent]
public class UIButtonFeel : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    [Tooltip("Ícone + texto. Desce quando o botão é pressionado.")]
    [SerializeField] private RectTransform content;

    [SerializeField] private float pressOffset = 10f;
    [SerializeField] private float hoverScale = 1.04f;

    [Tooltip("Pulsa de leve quando parado (use só no botão principal da tela).")]
    [SerializeField] private bool idlePulse;

    private const float Speed = 18f;

    private Selectable selectable;
    private Vector3 baseScale = Vector3.one;
    private Vector2 contentBase;

    private bool hovered;
    private bool pressed;
    private bool selected;
    private bool held;

    // Mantém o conteúdo afundado (ex.: ferramenta selecionada)
    public void SetHeld(bool value) => held = value;

    private float currentScale = 1f;
    private float currentOffset;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        baseScale = transform.localScale;

        if (content != null)
            contentBase = content.anchoredPosition;
    }

    private void OnDisable()
    {
        hovered = pressed = selected = false;
        currentScale = 1f;
        currentOffset = 0f;
        Apply();
    }

    private void Update()
    {
        bool interactable = selectable == null || selectable.IsInteractable();

        float targetScale = 1f;

        if (interactable && (hovered || selected))
            targetScale = hoverScale;
        else if (interactable && idlePulse)
            targetScale = 1f + 0.025f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.6f));

        bool down = interactable && (pressed || held);
        float targetOffset = down ? -pressOffset : 0f;

        float t = 1f - Mathf.Exp(-Speed * Time.unscaledDeltaTime);

        currentScale = Mathf.Lerp(currentScale, targetScale, t);

        // Afundar é imediato (combina com a troca de sprite); voltar é suave
        currentOffset = down ? targetOffset : Mathf.Lerp(currentOffset, targetOffset, t);

        Apply();
    }

    private void Apply()
    {
        transform.localScale = baseScale * currentScale;

        if (content != null)
            content.anchoredPosition = contentBase + new Vector2(0f, currentOffset);
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        pressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData) => pressed = false;

    public void OnSelect(BaseEventData eventData) => selected = true;

    public void OnDeselect(BaseEventData eventData) => selected = false;
}
