using UnityEngine;

public class MainMenuSettings : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (creditsPanel.activeSelf)
            {
                CloseCredits();
            }
            else if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
        }
    }

    public void OpenSettings()
    {
        creditsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }
}

