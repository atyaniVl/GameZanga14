using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Slider slider;

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateBar;
    }

    private void Start()
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = playerHealth != null ? playerHealth.MaxHealth : 100f;
        slider.value = playerHealth != null ? playerHealth.CurrentHealth : 100f;
    }

    private void UpdateBar(float current, float max)
    {
        if (slider == null) return;
        slider.maxValue = max;
        slider.value = current;
    }
}