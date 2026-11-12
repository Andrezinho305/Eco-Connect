using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "Level01";

    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }
}
