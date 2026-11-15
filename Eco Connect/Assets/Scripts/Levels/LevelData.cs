
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewLevel",
    menuName = "EcoConnect/Level Data"
)]
public class LevelData : ScriptableObject
{
    [Header("Informações do nível")]
    public string levelName = "Novo Nível";

    [Header("Dimensões do tabuleiro")]
    [Min(1)] public int width = 10;
    [Min(1)] public int height = 10;

    [Header("Orçamento")]
    [Min(0)] public int initialBudget = 5000;

    [Header("Estruturas")]
    public List<StructureData> structures =
        new List<StructureData>();
}

[Serializable]
public class StructureData
{
    public Vector2Int position;

    public StructureType structureType;

    public NetworkType requiredServices;
}
