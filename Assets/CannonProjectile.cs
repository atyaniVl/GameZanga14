using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class CannonProjectile : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [SerializeField] GameObject hitParticle;
    [SerializeField] GameObject visual;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float speed)
    {
        direction.Normalize();

        rb.linearVelocity =
            direction * speed;

        Debug.Log(
            $"Cannon projectile initialized. " +
            $"Direction: {direction}, Speed: {speed}"
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Hit(other);
    }

    private void Hit(Collider2D target)
    {
        IDamageable damageable =
            target.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        hitParticle.SetActive(true);
        visual.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 3);
    }
}