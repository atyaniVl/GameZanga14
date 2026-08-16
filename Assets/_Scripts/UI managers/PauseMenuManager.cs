using GenericSceneManagement;
using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AdditiveUiSceneManager additiveUiManager;

    [Header("Optional")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private void Awake()
    {
        if (additiveUiManager == null)
            additiveUiManager = FindFirstObjectByType<AdditiveUiSceneManager>();
    }

    public void OnResumeClicked()
    {
        if (additiveUiManager != null)
            additiveUiManager.ClosePauseMenu();
    }

    public void OnSettingsClicked()
    {
        if (additiveUiManager != null)
            additiveUiManager.OpenSettingsMenu();
    }

    public void OnBackToMainMenuClicked()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(mainMenuScene))
            SceneLoader.Load(mainMenuScene);
    }

    public void OnQuitGameClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}