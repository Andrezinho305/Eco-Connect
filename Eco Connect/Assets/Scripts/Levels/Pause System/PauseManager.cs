using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsPanel;

    [Header("Referências")]
    [SerializeField] private LevelManager levelManager;

    public bool IsPaused { get; private set; }

    private void Start()
    {
        Time.timeScale = 1f;

        IsPaused = false;

        pausePanel.SetActive(false);
        pauseMenu.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
    }

    private void HandleEscape()
    {
        // Janelas abertas (card de elemento, Legenda) fecham primeiro
        if (!IsPaused && OverlayEscape.TryConsume())
            return;

        if (!IsPaused)
        {
            PauseGame();
            return;
        }

        if (settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        ResumeGame();
    }

    public void PauseGame()
    {
        if (IsPaused)
            return;

        if (levelManager != null &&
            levelManager.IsLevelCompleted)
            return;

        IsPaused = true;

        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        pauseMenu.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        IsPaused = false;

        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        pauseMenu.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (!IsPaused)
            return;

        pauseMenu.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
