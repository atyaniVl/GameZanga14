using UnityEngine;
using GenericSceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameplayScene = "Core Game";
    [SerializeField] private string storyScene = "Story";
    [SerializeField] private string settingsScene = "Settings";

    [Header("Load Mode")]
    [SerializeField] private bool gameplayLoadsSingle = true;
    [SerializeField] private bool storyLoadsSingle = true;


    // ============================================================
    // BUTTON METHODS
    // ============================================================

    public void OnPlayClicked()
    {
        LoadScene(gameplayScene, gameplayLoadsSingle);
    }

    public void OnStoryClicked()
    {
        LoadScene(storyScene, storyLoadsSingle);
    }

    public void OnSettingsClicked()
    {
        LoadScene(settingsScene, false);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }


    // ============================================================
    // SCENE LOADING
    // ============================================================

    private void LoadScene(string sceneName, bool loadSingle)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning(
                "MainMenuManager: Scene name is empty."
            );

            return;
        }

        if (loadSingle)
        {
            SceneLoader.Load(sceneName);
            return;
        }

        SceneLoader.LoadAdditive(sceneName);
    }
}