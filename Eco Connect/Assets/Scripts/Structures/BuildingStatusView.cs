using UnityEngine;

public class BuildingStatusView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer waterStatus;
    [SerializeField] private SpriteRenderer electricityStatus;
    [SerializeField] private SpriteRenderer sewageStatus;

    [SerializeField] private Color suppliedColor = Color.green;
    [SerializeField] private Color missingColor = Color.red;

    // Serviços abastecidos na última atualização (para tocar som só quando muda)
    private NetworkType lastSupplied;
    private bool hasState;

    public void UpdateStatus(
        GridCell cell,
        NetworkConnectivity connectivity)
    {
        NetworkType supplied = NetworkType.None;

        if (UpdateService(waterStatus, cell, connectivity, NetworkType.Water))
            supplied |= NetworkType.Water;

        if (UpdateService(electricityStatus, cell, connectivity, NetworkType.Electricity))
            supplied |= NetworkType.Electricity;

        if (UpdateService(sewageStatus, cell, connectivity, NetworkType.Sewage))
            supplied |= NetworkType.Sewage;

        PlayFeedback(cell, supplied);
    }


    // Retorna verdadeiro se o serviço é exigido e está abastecido
    private bool UpdateService(
        SpriteRenderer indicator,
        GridCell cell,
        NetworkConnectivity connectivity,
        NetworkType service)
    {
        bool required =
            (cell.RequiredServices & service) != 0;

        indicator.enabled = required;

        if (!required)
            return false;

        bool supplied = connectivity.IsBuildingSupplied(
            cell.Position,
            service
        );

        indicator.color =
            supplied ? suppliedColor : missingColor;

        return supplied;
    }


    private void PlayFeedback(GridCell cell, NetworkType supplied)
    {
        // A primeira atualização (início da fase) não toca som
        if (hasState)
        {
            NetworkType newlySupplied = supplied & ~lastSupplied;

            if (newlySupplied != NetworkType.None)
            {
                bool complete =
                    cell.RequiredServices != NetworkType.None &&
                    supplied == cell.RequiredServices;

                SFXManager.Play(
                    complete ? Sfx.HouseComplete : Sfx.ServiceConnected
                );
            }
        }

        lastSupplied = supplied;
        hasState = true;
    }

}
