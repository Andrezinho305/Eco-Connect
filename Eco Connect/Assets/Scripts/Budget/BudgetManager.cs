using System;
using UnityEngine;

public class BudgetManager : MonoBehaviour
{

    [Header("Orçamento")]
    [SerializeField] private int initialBudget = 5000;

    [Header("Custos das Redes")]
    [Min(0)] [SerializeField] private int waterCost = 100;
    [Min(0)] [SerializeField] private int electricityCost = 150;
    [Min(0)] [SerializeField] private int sewageCost = 200;

    [Header("Custos de Sobreposição")]
    [Min(0)] [SerializeField] private int secondNetworkExtraCost = 75;
    [Min(0)] [SerializeField] private int thirdNetworkExtraCost = 100;

    public int InitialBudget => initialBudget;

    public int CurrentBudget { get; private set; }

    public event Action<int> OnBudgetChanged;

    private void Awake()
    {
        CurrentBudget = initialBudget;
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || CurrentBudget < amount)
            return false;

        CurrentBudget -= amount;
        OnBudgetChanged?.Invoke(CurrentBudget);

        return true;
    }

    public void Refund(int amount)
    {
        if (amount <= 0)
            return;

        CurrentBudget += amount;
        OnBudgetChanged?.Invoke(CurrentBudget);
    }


    public int GetNetworkCost(NetworkType network)
    {
        switch (network)
        {
            case NetworkType.Water:
                return waterCost;

            case NetworkType.Electricity:
                return electricityCost;

            case NetworkType.Sewage:
                return sewageCost;

            default:
                return 0;
        }
    }


    public int GetCellCost(NetworkType networks)
    {
        int cost = 0;
        int networkCount = 0;

        if ((networks & NetworkType.Water) != 0)
        {
            cost += GetNetworkCost(NetworkType.Water);
            networkCount++;
        }

        if ((networks & NetworkType.Electricity) != 0)
        {
            cost += GetNetworkCost(NetworkType.Electricity);
            networkCount++;
        }

        if ((networks & NetworkType.Sewage) != 0)
        {
            cost += GetNetworkCost(NetworkType.Sewage);
            networkCount++;
        }

        if (networkCount >= 2)
            cost += secondNetworkExtraCost;

        if (networkCount == 3)
            cost += thirdNetworkExtraCost;

        return cost;
    }


    public int GetConstructionCost(
        GridCell cell,
        NetworkType network)
    {
        if (cell.HasNetwork(network))
            return 0;

        NetworkType newNetworks = cell.Networks | network;

        int previousCost = GetCellCost(cell.Networks);
        int newCost = GetCellCost(newNetworks);

        return newCost - previousCost;
    }

    [ContextMenu("Mostrar Orçamento")]
    private void ShowBudget()
    {
        Debug.Log("Orçamento atual: R$ " + CurrentBudget);
    }

}
