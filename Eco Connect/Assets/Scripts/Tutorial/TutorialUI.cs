using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject panel;

    [Header("Textos")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text stepCounterText;

    [Header("Botões")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonLabel;
    [SerializeField] private Button skipButton;

    public event Action OnContinueClicked;
    public event Action OnSkipClicked;

    public bool IsContinueVisible =>
        continueButton != null &&
        continueButton.gameObject.activeSelf;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(HandleContinue);

        if (skipButton != null)
            skipButton.onClick.AddListener(HandleSkip);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(HandleContinue);

        if (skipButton != null)
            skipButton.onClick.RemoveListener(HandleSkip);
    }

    public void ShowStep(
        string message,
        int stepNumber,
        int totalSteps,
        bool showContinue,
        string continueLabel,
        bool showSkip)
    {
        messageText.text = message;

        if (stepCounterText != null)
            stepCounterText.text =
                "Passo " + stepNumber + "/" + totalSteps;

        continueButton.gameObject.SetActive(showContinue);

        if (continueButtonLabel != null)
            continueButtonLabel.text = continueLabel;

        if (skipButton != null)
            skipButton.gameObject.SetActive(showSkip);
    }

    public void SetVisible(bool visible)
    {
        if (panel.activeSelf != visible)
            panel.SetActive(visible);
    }

    private void HandleContinue()
    {
        OnContinueClicked?.Invoke();
    }

    private void HandleSkip()
    {
        OnSkipClicked?.Invoke();
    }
}
