using UnityEngine;

public class BuildingStatusView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer waterStatus;
    [SerializeField] private SpriteRenderer electricityStatus;
    [SerializeField] private SpriteRenderer sewageStatus;

    [SerializeField] private Color suppliedColor = Color.green;
    [SerializeField] private Color missingColor = Color.red;

    public void UpdateStatus(
        GridCell cell,
        NetworkConnectivity connectivity)
    {
        UpdateService(
            waterStatus,
            cell,
            connectivity,
            NetworkType.Water
        );

        UpdateService(
            electricityStatus,
            cell,
            connectivity,
            NetworkType.Electricity
        );

        UpdateService(
            sewageStatus,
            cell,
            connectivity,
            NetworkType.Sewage
        );
    }


    private void UpdateService(
        SpriteRenderer indicator,
        GridCell cell,
        NetworkConnectivity connectivity,
        NetworkType service)
    {
        bool required =
            (cell.RequiredServices & service) != 0;

        indicator.enabled = required;

        if (!required)
            return;

        bool supplied = connectivity.IsBuildingSupplied(
            cell.Position,
            service
        );

        indicator.color =
            supplied ? suppliedColor : missingColor;
    }

}

