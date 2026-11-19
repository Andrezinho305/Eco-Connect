using System.Collections;
using UnityEngine;

// Anima a entrada de um painel sempre que ele é ativado:
// o card cresce de 90% para 100% com um leve "quique" e o painel aparece.
// Usa tempo real, então funciona no menu de pausa.
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class UIPanelPop : MonoBehaviour
{
    [Tooltip("O card que cresce. Se vazio, só faz o fade.")]
    [SerializeField] private RectTransform card;

    [SerializeField] private float duration = 0.22f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        float time = 0f;

        while (time < duration)
        {
            float t = Mathf.Clamp01(time / duration);

            canvasGroup.alpha = Mathf.Clamp01(t * 2f);

            if (card != null)
                card.localScale = Vector3.one * Mathf.LerpUnclamped(0.9f, 1f, EaseOutBack(t));

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        if (card != null)
            card.localScale = Vector3.one;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        float x = t - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }
}
