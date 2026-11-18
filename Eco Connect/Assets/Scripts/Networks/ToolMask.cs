// Conjunto de ferramentas de construção.
// Os valores de Water, Electricity e Sewage coincidem com NetworkType.
[System.Flags]
public enum ToolMask
{
    None = 0,
    Water = 1,
    Electricity = 2,
    Sewage = 4,
    Eraser = 8,
    All = Water | Electricity | Sewage | Eraser
}
