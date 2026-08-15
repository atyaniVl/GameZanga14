using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ShootController shootController;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text rocksText;


    private void Start()
    {
        SetupHealthUI();
        SetupRocksUI();

        playerHealth.OnHealthChanged += UpdateHealthUI;
        shootController.OnRocksChanged += UpdateRocksUI;
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
}