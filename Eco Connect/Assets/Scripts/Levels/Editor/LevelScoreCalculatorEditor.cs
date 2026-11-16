
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelScoreCalculator))]
public class LevelScoreCalculatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Mostra os campos normais do componente
        DrawDefaultInspector();

        LevelScoreCalculator calculator =
            (LevelScoreCalculator)target;

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField(
            "Valores de Pontuação",
            EditorStyles.boldLabel
        );

        // Verifica se o BudgetManager foi associado
        if (!calculator.HasBudgetManager)
        {
            EditorGUILayout.HelpBox(
                "Associe o BudgetManager para visualizar os valores.",
                MessageType.Warning
            );

            return;
        }

        // Exibe os valores calculados
        EditorGUILayout.LabelField(
            "Saldo mínimo para 2 estrelas:",
            "R$ " + calculator.TwoStarBudget.ToString("N2")
        );

        EditorGUILayout.LabelField(
            "Saldo mínimo para 3 estrelas:",
            "R$ " + calculator.ThreeStarBudget.ToString("N2")
        );
    }
}


