using UnityEngine;

public class NetworkConnectionView : MonoBehaviour
{
    [SerializeField] private GameObject center;
    [SerializeField] private GameObject up;
    [SerializeField] private GameObject down;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;

    public void SetConnections(
        bool hasNetwork,
        bool connectUp,
        bool connectDown,
        bool connectLeft,
        bool connectRight)
    {
        center.SetActive(hasNetwork);

        up.SetActive(hasNetwork && connectUp);
        down.SetActive(hasNetwork && connectDown);
        left.SetActive(hasNetwork && connectLeft);
        right.SetActive(hasNetwork && connectRight);
    }
}
