using AudioSystem;
using GenericSceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AdditiveUiSceneManager additiveUiManager;

    [Header("Volume Sliders (0 - 1)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Audio Toggles")]
    [SerializeField] private Toggle masterToggle;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle musicToggle;

    private void Awake()
    {
        if (additiveUiManager == null)
            additiveUiManager = FindFirstObjectByType<AdditiveUiSceneManager>();
    }

    private void Start()
    {
        SyncUiFromSavedValues();
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnMasterEnabledChanged(bool enabled)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterEnabled(enabled);
    }

    public void OnSfxEnabledChanged(bool enabled)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxEnabled(enabled);
    }

    public void OnMusicEnabledChanged(bool enabled)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicEnabled(enabled);
    }

    public void OnCloseClicked()
    {
        if (additiveUiManager != null)
        {
            additiveUiManager.CloseSettingsMenu();
            return;
        }

        // Fallback if no controller exists in scene.
        SceneLoader.Unload(gameObject.scene.name);
    }

    public void SyncUiFromSavedValues()
    {
        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioManager.MasterVolumeKey, 1f));

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioManager.SfxVolumeKey, 1f));

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioManager.MusicVolumeKey, 1f));

        if (masterToggle != null)
            masterToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(AudioManager.MasterEnabledKey, 1) == 1);

        if (sfxToggle != null)
            sfxToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(AudioManager.SfxEnabledKey, 1) == 1);

        if (musicToggle != null)
            musicToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(AudioManager.MusicEnabledKey, 1) == 1);
    }
}