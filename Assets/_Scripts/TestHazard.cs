using UnityEngine;

public class TestHazard : MonoBehaviour
{
    [SerializeField] private int damageAmount = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            health.TakeDamage(damageAmount);
        }
    }
}
