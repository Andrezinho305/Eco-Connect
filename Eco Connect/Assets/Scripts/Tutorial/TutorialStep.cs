using System;
using System.Collections.Generic;
using UnityEngine;

// O que faz uma etapa do tutorial avançar
public enum TutorialCondition
{
    // Avança ao clicar em "Continuar"
    PressContinue,

    // Avança quando a ferramenta "requiredTool" for selecionada
    SelectTool,

    // Avança quando a casa em "targetBuilding" receber "requiredNetwork"
    BuildingSupplied,

    // Avança quando o jogador apagar qualquer rede
    EraseAnyNetwork,

    // Explica sobreposição. Destaca as células com mais de uma rede
    // e é pulada se não houver nenhuma.
    OverlapInfo,

    // Avança quando a fase for concluída
    LevelCompleted
}

// Elementos de interface que podem ser destacados
public enum TutorialUITarget
{
    None,
    WaterButton,
    ElectricityButton,
    SewageButton,
    EraserButton,
    BudgetPanel,
    ProgressPanel,
    StarPanel
}

[Serializable]
public class TutorialStep
{
    [TextArea(3, 8)]
    public string message;

    public TutorialCondition condition = TutorialCondition.PressContinue;

    [Tooltip("Ferramentas liberadas durante esta etapa")]
    public ToolMask allowedTools = ToolMask.None;

    [Tooltip("Texto do botão Continuar (etapas PressContinue/OverlapInfo)")]
    public string continueLabel = "Continuar";

    [Header("Condição: SelectTool")]
    public ToolMask requiredTool = ToolMask.None;

    [Header("Condição: BuildingSupplied")]
    public Vector2Int targetBuilding;
    public NetworkType requiredNetwork = NetworkType.None;

    [Header("Destaques")]
    public List<Vector2Int> highlightCells = new List<Vector2Int>();
    public List<TutorialUITarget> highlightUI = new List<TutorialUITarget>();


    // Roteiro padrão do Level00_Tutorial.
    // Posições: caixa d'água (2,2), usina (8,1), esgoto (8,8),
    // casa de água (9,7), casa de energia (2,3), casa de esgoto (9,1).
    public static List<TutorialStep> CreateDefaultSteps()
    {
        const ToolMask WaterAndPower =
            ToolMask.Water | ToolMask.Electricity;

        return new List<TutorialStep>
        {
            new TutorialStep
            {
                message =
                    "Bem-vindo ao Eco Connect!\n\n" +
                    "Sua missão é levar água, energia e esgoto a todas " +
                    "as casas, gastando o mínimo possível do orçamento."
            },

            new TutorialStep
            {
                message =
                    "Estas são as fontes de cada serviço:\n\n" +
                    "• Caixa d'água: fornece água\n" +
                    "• Usina: fornece energia\n" +
                    "• Estação de tratamento: recebe o esgoto",
                highlightCells =
                {
                    new Vector2Int(2, 2),
                    new Vector2Int(8, 1),
                    new Vector2Int(8, 8)
                }
            },

            new TutorialStep
            {
                message =
                    "Estas são as casas. Os indicadores mostram o " +
                    "serviço que cada uma precisa:\n\n" +
                    "• Preto: o serviço ainda não chegou\n" +
                    "• Verde: casa abastecida!",
                highlightCells =
                {
                    new Vector2Int(9, 7),
                    new Vector2Int(2, 3),
                    new Vector2Int(9, 1)
                }
            },

            new TutorialStep
            {
                message =
                    "Selecione a ferramenta Água clicando no botão " +
                    "ou apertando a tecla 1.",
                condition = TutorialCondition.SelectTool,
                allowedTools = ToolMask.Water,
                requiredTool = ToolMask.Water,
                highlightUI = { TutorialUITarget.WaterButton }
            },

            new TutorialStep
            {
                message =
                    "Clique e arraste pelo tabuleiro para construir " +
                    "canos da caixa d'água até a casa que precisa " +
                    "de água.",
                condition = TutorialCondition.BuildingSupplied,
                allowedTools = ToolMask.Water,
                targetBuilding = new Vector2Int(9, 7),
                requiredNetwork = NetworkType.Water,
                highlightCells =
                {
                    new Vector2Int(2, 2),
                    new Vector2Int(9, 7)
                }
            },

            new TutorialStep
            {
                message =
                    "Muito bem!\n\n" +
                    "Cada cano custa dinheiro. O custo aparece ao lado " +
                    "do cursor e o saldo disponível fica no topo da tela.",
                allowedTools = ToolMask.Water,
                highlightUI = { TutorialUITarget.BudgetPanel }
            },

            new TutorialStep
            {
                message =
                    "Agora selecione Eletricidade (tecla 2) e leve " +
                    "energia da usina até a casa à esquerda.",
                condition = TutorialCondition.BuildingSupplied,
                allowedTools = WaterAndPower,
                targetBuilding = new Vector2Int(2, 3),
                requiredNetwork = NetworkType.Electricity,
                highlightCells =
                {
                    new Vector2Int(8, 1),
                    new Vector2Int(2, 3)
                },
                highlightUI = { TutorialUITarget.ElectricityButton }
            },

            new TutorialStep
            {
                message =
                    "Você cruzou duas redes na mesma célula!\n\n" +
                    "Isso é permitido, mas células com mais de uma " +
                    "rede custam mais caro.",
                condition = TutorialCondition.OverlapInfo,
                allowedTools = WaterAndPower
            },

            new TutorialStep
            {
                message =
                    "Errou o caminho? Selecione a Borracha (tecla 4) " +
                    "e apague um cano. O dinheiro gasto nele é devolvido.",
                condition = TutorialCondition.EraseAnyNetwork,
                allowedTools = WaterAndPower | ToolMask.Eraser,
                highlightUI = { TutorialUITarget.EraserButton }
            },

            new TutorialStep
            {
                message =
                    "Agora é com você! Selecione Esgoto (tecla 3) e " +
                    "ligue a última casa à estação de tratamento.\n\n" +
                    "Se apagou algum cano, reconstrua-o.",
                condition = TutorialCondition.LevelCompleted,
                allowedTools = ToolMask.All,
                highlightCells =
                {
                    new Vector2Int(8, 8),
                    new Vector2Int(9, 1)
                },
                highlightUI = { TutorialUITarget.SewageButton }
            },

            new TutorialStep
            {
                message =
                    "Fase concluída!\n\n" +
                    "Sua nota depende do saldo que sobrou: quanto menos " +
                    "gastar, mais estrelas você ganha. Boa sorte nas " +
                    "próximas fases!",
                allowedTools = ToolMask.All,
                continueLabel = "Concluir",
                highlightUI = { TutorialUITarget.StarPanel }
            }
        };
    }
}
