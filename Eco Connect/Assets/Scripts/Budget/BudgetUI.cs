using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BudgetUI : MonoBehaviour
{
    [SerializeField] private BudgetManager budgetManager;
    [SerializeField] private TMP_Text budgetText;

    [Header("Tema (opcional)")]
    [Tooltip("Desligue quando houver uma moeda \"R$\" ao lado do valor")]
    [SerializeField] private bool showCurrencyPrefix = true;

    [SerializeField] private LevelScoreCalculator scoreCalculator;
    [SerializeField] private TMP_Text initialBudgetText;

    [Tooltip("Barra que esvazia com o gasto (âncora máxima X = saldo / inicial)")]
    [SerializeField] private RectTransform barFill;
    [SerializeField] private RectTransform threeStarTick;
    [SerializeField] private RectTransform twoStarTick;
    [SerializeField] private Image[] ratingStars;

    [Tooltip("O que treme quando falta dinheiro")]
    [SerializeField] private RectTransform shakeTarget;

    [SerializeField] private Color textColor = new Color(0.133f, 0.208f, 0.165f);
    [SerializeField] private Color goodColor = new Color(0.263f, 0.639f, 0.310f);
    [SerializeField] private Color warnColor = new Color(1f, 0.784f, 0.239f);
    [SerializeField] private Color badColor = new Color(0.851f, 0.282f, 0.231f);
    [SerializeField] private Color starOnColor = new Color(1f, 0.784f, 0.239f);
    [SerializeField] private Color starOffColor = new Color(0.847f, 0.816f, 0.737f);

    private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

    private const float CountSpeed = 10f;
    private const float ShakeDuration = 0.35f;
    private const float FlashDuration = 0.6f;

    private float shownBudget;
    private float shakeTime;
    private float flashTime;
    private Vector2 shakeBase;

    private void OnEnable()
    {
        if (budgetManager != null)
        {
            budgetManager.OnBudgetChanged += HandleBudgetChanged;
            budgetManager.OnInsufficientFunds += HandleInsufficientFunds;
        }
    }

    private void OnDisable()
    {
        if (budgetManager != null)
        {
            budgetManager.OnBudgetChanged -= HandleBudgetChanged;
            budgetManager.OnInsufficientFunds -= HandleInsufficientFunds;
        }
    }

    private void Start()
    {
        shownBudget = budgetManager.CurrentBudget;

        if (shakeTarget != null)
            shakeBase = shakeTarget.anchoredPosition;

        if (initialBudgetText != null)
            initialBudgetText.text = "de " + Format(budgetManager.ConfiguredInitialBudget);

        PlaceTicks();
        Refresh();
    }

    private void HandleBudgetChanged(int remainingBudget)
    {
        // O valor mostrado "conta" até o novo saldo no Update
        if (!isActiveAndEnabled)
            Refresh();
    }

    private void HandleInsufficientFunds()
    {
        shakeTime = ShakeDuration;
        flashTime = FlashDuration;
    }

    private void Update()
    {
        float target = budgetManager.CurrentBudget;

        float t = 1f - Mathf.Exp(-CountSpeed * Time.unscaledDeltaTime);
        shownBudget = Mathf.Lerp(shownBudget, target, t);

        if (Mathf.Abs(shownBudget - target) < 1f)
            shownBudget = target;

        if (flashTime > 0f)
            flashTime -= Time.unscaledDeltaTime;

        UpdateShake();
        Refresh();
    }

    private void Refresh()
    {
        bool flashing = flashTime > 0f;

        budgetText.text = showCurrencyPrefix
            ? "R$ " + Format(Mathf.RoundToInt(shownBudget))
            : Format(Mathf.RoundToInt(shownBudget));

        budgetText.color = flashing ? badColor : textColor;

        int initial = budgetManager.ConfiguredInitialBudget;
        float ratio = initial > 0 ? Mathf.Clamp01(shownBudget / initial) : 0f;

        if (barFill != null)
        {
            barFill.anchorMin = Vector2.zero;
            barFill.anchorMax = new Vector2(ratio, 1f);

            Graphic fillGraphic = barFill.GetComponent<Graphic>();

            if (fillGraphic != null)
                fillGraphic.color = flashing ? badColor : ColorFor(ratio);
        }

        if (ratingStars != null && ratingStars.Length > 0 && scoreCalculator != null)
        {
            int stars = scoreCalculator.CalculateStars();

            for (int i = 0; i < ratingStars.Length; i++)
            {
                if (ratingStars[i] != null)
                    ratingStars[i].color = i < stars ? starOnColor : starOffColor;
            }
        }
    }

    private Color ColorFor(float ratio)
    {
        if (scoreCalculator == null)
            return goodColor;

        int initial = budgetManager.ConfiguredInitialBudget;

        if (initial <= 0)
            return goodColor;

        if (ratio >= scoreCalculator.ThreeStarBudget / initial)
            return goodColor;

        if (ratio >= scoreCalculator.TwoStarBudget / initial)
            return warnColor;

        return badColor;
    }

    // Marcas na barra onde se perde a 3ª e a 2ª estrela
    private void PlaceTicks()
    {
        if (scoreCalculator == null)
            return;

        int initial = budgetManager.ConfiguredInitialBudget;

        if (initial <= 0)
            return;

        PlaceTick(threeStarTick, scoreCalculator.ThreeStarBudget / initial);
        PlaceTick(twoStarTick, scoreCalculator.TwoStarBudget / initial);
    }

    private static void PlaceTick(RectTransform tick, float position)
    {
        if (tick == null)
            return;

        position = Mathf.Clamp01(position);

        tick.anchorMin = new Vector2(position, tick.anchorMin.y);
        tick.anchorMax = new Vector2(position, tick.anchorMax.y);
        tick.anchoredPosition = new Vector2(0f, tick.anchoredPosition.y);
    }

    private void UpdateShake()
    {
        if (shakeTarget == null)
            return;

        if (shakeTime <= 0f)
        {
            shakeTarget.anchoredPosition = shakeBase;
            return;
        }

        shakeTime -= Time.unscaledDeltaTime;

        float strength = Mathf.Clamp01(shakeTime / ShakeDuration);
        float offset = Mathf.Sin(Time.unscaledTime * 55f) * 12f * strength;

        shakeTarget.anchoredPosition = shakeBase + new Vector2(offset, 0f);
    }

    private static string Format(int value)
    {
        return value.ToString("N0", PtBr);
    }
}
