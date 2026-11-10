using UnityEngine;

public class NetworkBuilder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GridRenderer gridRenderer;

    private NetworkType selectedNetwork = NetworkType.Water;

    private bool eraseMode = false;


    private void Update()
    {
        HandleToolSelection();
        HandleConstruction();
    }


    private void HandleToolSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedNetwork = NetworkType.Water;
            eraseMode = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedNetwork = NetworkType.Electricity;
            eraseMode = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedNetwork = NetworkType.Sewage;
            eraseMode = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            eraseMode = true;
        }
    }

    private void HandleConstruction()
    {
        if (!Input.GetMouseButton(0))
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z =
            gridManager.transform.position.z -
            mainCamera.transform.position.z;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2Int gridPosition =
            gridManager.WorldToGridPosition(worldPosition);

        GridCell cell = gridManager.GetCell(gridPosition);

        if (cell == null)
            return;

        if (eraseMode)
        {
            if (cell.Networks == NetworkType.None)
                return;

            cell.ClearNetworks();
        }
        else
        {
            if (cell.HasNetwork(selectedNetwork))
                return;

            cell.AddNetwork(selectedNetwork);
        }

        gridRenderer.RefreshCell(gridPosition);
    }


}
