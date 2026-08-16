using System.Collections;
using GenericSceneManagement;
using UnityEngine;

public class AdditiveUiSceneManager : MonoBehaviour
{
    [Header("UI Scenes")]
    [SerializeField] private string pauseSceneName = "PauseMenu";
    [SerializeField] private string settingsSceneName = "Settings";

    [Header("Pause Control")]
    [SerializeField] private bool pauseGameplayWithPauseScene = true;
    [SerializeField] private bool allowEscapeToggle = true;
    [SerializeField] private KeyCode pauseToggleKey = KeyCode.Escape;

    private bool _transitionRunning;

    public bool IsPauseMenuOpen => SceneLoader.IsLoaded(pauseSceneName);
    public bool IsSettingsOpen => SceneLoader.IsLoaded(settingsSceneName);

    private void Update()
    {
        if (!allowEscapeToggle || _transitionRunning)
            return;

        if (Input.GetKeyDown(pauseToggleKey) && !IsPauseMenuOpen)
            OpenPauseMenu();
    }

    public void TogglePauseMenu()
    {
        if (IsPauseMenuOpen)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    public void OpenPauseMenu()
    {
        if (_transitionRunning || string.IsNullOrWhiteSpace(pauseSceneName) || IsPauseMenuOpen)
            return;

        StartCoroutine(OpenPauseRoutine());
    }

    public void ClosePauseMenu()
    {
        if (_transitionRunning || string.IsNullOrWhiteSpace(pauseSceneName) || !IsPauseMenuOpen)
            return;

        StartCoroutine(ClosePauseRoutine());
    }

    public void OpenSettingsMenu()
    {
        if (_transitionRunning || string.IsNullOrWhiteSpace(settingsSceneName) || IsSettingsOpen)
            return;

        StartCoroutine(OpenSettingsRoutine());
    }

    public void CloseSettingsMenu()
    {
        if (_transitionRunning || string.IsNullOrWhiteSpace(settingsSceneName) || !IsSettingsOpen)
            return;

        StartCoroutine(CloseSettingsRoutine());
    }

    public void CloseAllUi()
    {
        if (_transitionRunning)
            return;

        StartCoroutine(CloseAllRoutine());
    }

    private IEnumerator OpenPauseRoutine()
    {
        _transitionRunning = true;

        if (pauseGameplayWithPauseScene)
            Time.timeScale = 0f;

        var op = SceneLoader.LoadAdditive(pauseSceneName);
        if (op != null)
            yield return op;

        _transitionRunning = false;
    }

    private IEnumerator ClosePauseRoutine()
    {
        _transitionRunning = true;

        if (IsSettingsOpen)
        {
            var settingsUnload = SceneLoader.Unload(settingsSceneName);
            if (settingsUnload != null)
                yield return settingsUnload;
        }

        var op = SceneLoader.Unload(pauseSceneName);
        if (op != null)
            yield return op;

        if (pauseGameplayWithPauseScene)
            Time.timeScale = 1f;

        _transitionRunning = false;
    }

    private IEnumerator OpenSettingsRoutine()
    {
        _transitionRunning = true;

        var op = SceneLoader.LoadAdditive(settingsSceneName);
        if (op != null)
            yield return op;

        _transitionRunning = false;
    }

    private IEnumerator CloseSettingsRoutine()
    {
        _transitionRunning = true;

        var op = SceneLoader.Unload(settingsSceneName);
        if (op != null)
            yield return op;

        _transitionRunning = false;
    }

    private IEnumerator CloseAllRoutine()
    {
        _transitionRunning = true;

        if (IsSettingsOpen)
        {
            var settingsUnload = SceneLoader.Unload(settingsSceneName);
            if (settingsUnload != null)
                yield return settingsUnload;
        }

        if (IsPauseMenuOpen)
        {
            var pauseUnload = SceneLoader.Unload(pauseSceneName);
            if (pauseUnload != null)
                yield return pauseUnload;
        }

        if (pauseGameplayWithPauseScene)
            Time.timeScale = 1f;

        _transitionRunning = false;
    }
}