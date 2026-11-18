using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuUI : MonoBehaviour
{
    // Todo jogo novo começa pelo tutorial
    [FormerlySerializedAs("firstLevelSceneName")]
    [SerializeField] private string tutorialSceneName = "Level00_Tutorial";

    public void PlayGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(tutorialSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Encerrando o jogo...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
