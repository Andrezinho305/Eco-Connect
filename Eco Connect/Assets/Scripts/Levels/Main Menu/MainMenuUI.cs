using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuUI : MonoBehaviour
{
    [Header("Cenas")]
    [FormerlySerializedAs("firstLevelSceneName")]
    [SerializeField] private string tutorialSceneName = "Level00_Tutorial";

    // Cena aberta pelo "Jogar" depois que o tutorial foi concluído
    [SerializeField] private string levelAfterTutorialSceneName = "Level01";

    public void PlayGame()
    {
        Time.timeScale = 1f;

        string sceneToLoad = TutorialManager.IsTutorialCompleted
            ? levelAfterTutorialSceneName
            : tutorialSceneName;

        SceneManager.LoadScene(sceneToLoad);
    }

    // Abre o tutorial mesmo que ele já tenha sido concluído
    // (pode ser ligado a um botão "Tutorial" no menu)
    public void PlayTutorial()
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
