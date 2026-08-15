using UnityEngine;

public class CollectiblePulse : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float pulseAmount = 0.1f;
    [SerializeField] private float pulseSpeed = 3f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float pulse =
            1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        transform.localScale =
            originalScale * pulse;
    }
}