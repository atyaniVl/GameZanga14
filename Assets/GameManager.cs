using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ShootController shootController;
    [SerializeField] private PlayerCollector playerCollector;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text rocksText;
    [SerializeField] private TMP_Text keysText;


    private void Start()
    {
        SetupHealthUI();
        SetupRocksUI();
        SetupKeysUI();

        playerHealth.OnHealthChanged += UpdateHealthUI;
        shootController.OnRocksChanged += UpdateRocksUI;
        playerCollector.OnKeyCollected += UpdateKeysUI;
    }


    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -=
                UpdateHealthUI;
        }

        if (shootController != null)
        {
            shootController.OnRocksChanged -=
                UpdateRocksUI;
        }

        if (playerCollector != null)
        {
            playerCollector.OnKeyCollected -=
                UpdateKeysUI;
        }
    }


    // ============================================================
    // HEALTH
    // ============================================================

    private void SetupHealthUI()
    {
        if (playerHealth == null ||
            healthSlider == null)
            return;

        healthSlider.minValue = 0f;
        healthSlider.maxValue =
            playerHealth.MaxHealth;

        healthSlider.value =
            playerHealth.CurrentHealth;
    }


    private void UpdateHealthUI(
        float currentHealth,
        float maxHealth
    )
    {
        if (healthSlider == null)
            return;

        healthSlider.maxValue =
            maxHealth;

        healthSlider.value =
            currentHealth;
    }


    // ============================================================
    // ROCKS
    // ============================================================

    private void SetupRocksUI()
    {
        if (shootController == null)
            return;

        UpdateRocksUI(
            shootController.CurrentRocks
        );
    }


    private void UpdateRocksUI(int rocks)
    {
        if (rocksText == null)
            return;

        rocksText.text =
            rocks.ToString();
    }


    // ============================================================
    // KEYS
    // ============================================================

    private void SetupKeysUI()
    {
        if (playerCollector == null)
            return;

        UpdateKeysUI(
            playerCollector.KeysCollected
        );
    }


    private void UpdateKeysUI(int keys)
    {
        if (keysText == null)
            return;

        keysText.text =
            keys.ToString();
    }
}