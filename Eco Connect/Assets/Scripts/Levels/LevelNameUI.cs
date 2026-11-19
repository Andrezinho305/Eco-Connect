using TMPro;
using UnityEngine;

// Mostra o nome da fase (do LevelData) em português: "Level 1" vira "Fase 1"
public class LevelNameUI : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TMP_Text label;

    private void Start()
    {
        if (gridManager == null || label == null)
            return;

        LevelData level = gridManager.CurrentLevel;

        if (level == null || string.IsNullOrWhiteSpace(level.levelName))
            return;

        label.text = level.levelName
            .Replace("Level", "Fase")
            .Replace("Nível", "Fase");
    }
}
