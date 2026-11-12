using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    [Header("Interface")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressFill;

    private void OnEnable()
    {
        if (levelManager != null)
        {
            levelManager.OnProgressChanged += UpdateProgress;
        }
    }

    private void Start()
    {
        UpdateProgress(
            levelManager.SuppliedBuildings,
            levelManager.TotalBuildings
        );
    }

    private void OnDisable()
    {
        if (levelManager != null)
        {
            levelManager.OnProgressChanged -= UpdateProgress;
        }
    }

    private void UpdateProgress(int supplied, int total)
    {
        progressText.text =
            supplied + "/" + total + " edificações";

        float progress = 0f;

        if (total > 0)
        {
            progress = (float)supplied / total;
        }

        progressFill.fillAmount = progress;
    }
}

