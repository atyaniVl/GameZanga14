using UnityEngine;
using AudioSystem;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 1;

    protected int currentHealth;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        Debug.Log($"{gameObject.name} took {damage} damage!");

        if (currentHealth <= 0)
            return;

        currentHealth -= damage;

        AudioManager.Instance?.PlaySfxAtPositionRandomPitch("EnemyHit", transform.position, 0.9f, 1.1f);

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} DIED!");
            AudioManager.Instance?.PlaySfxAtPosition("EnemyDeath", transform.position);
            Die();
        }
    }

    protected virtual void Die()
    {
        // Override in specific enemies.
    }

    // ============================================================
    // PROJECTILE COLLISION
    // ============================================================

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("PlayerProjectile"))
            return;

        TakeDamage(1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerProjectile"))
            return;

        TakeDamage(1);
    }
}
