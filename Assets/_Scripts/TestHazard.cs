using UnityEngine;

public class TestHazard : MonoBehaviour
{
    [SerializeField] private float damageAmount = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            health.TakeDamage(damageAmount);
        }
    }
}