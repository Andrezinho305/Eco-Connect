using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    [Header("Interface")]
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressFill;

    [Header("Casinhas (opcional)")]
    [Tooltip("Uma cópia por edificação; fica colorida quando abastecida")]
    [SerializeField] private Image houseTemplate;
    [SerializeField] private Color suppliedHouseColor = new Color(0.263f, 0.639f, 0.310f);
    [SerializeField] private Color missingHouseColor = Color.white;

    [Tooltip("Mostra só \"1/3\" em vez de \"1/3 edificações\"")]
    [SerializeField] private bool compactText;

    private readonly List<Image> houses = new List<Image>();

    private void OnEnable()
    {
        if (levelManager != null)
        {
            levelManager.OnProgressChanged += UpdateProgress;
        }
    }

    private void Start()
    {
        if (houseTemplate != null)
            houseTemplate.gameObject.SetActive(false);

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
        progressText.text = compactText
            ? supplied + "/" + total
            : supplied + "/" + total + " edificações";

        float progress = 0f;

        if (total > 0)
        {
            progress = (float)supplied / total;
        }

        if (progressFill != null)
            progressFill.fillAmount = progress;

        UpdateHouses(supplied, total);
    }

    private void UpdateHouses(int supplied, int total)
    {
        if (houseTemplate == null)
            return;

        while (houses.Count < total)
        {
            Image house = Instantiate(houseTemplate, houseTemplate.transform.parent);
            house.gameObject.name = "House " + (houses.Count + 1);
            house.gameObject.SetActive(true);
            houses.Add(house);
        }

        for (int i = 0; i < houses.Count; i++)
        {
            houses[i].gameObject.SetActive(i < total);
            houses[i].color = i < supplied ? suppliedHouseColor : missingHouseColor;
        }
    }
}
