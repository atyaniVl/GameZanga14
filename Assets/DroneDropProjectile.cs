using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DroneDropProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float fallSpeed = 5f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private int damage = 40;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual")]
    [SerializeField] private GameObject explosionVisual;
    [SerializeField] private GameObject projectileVisual;

    private Rigidbody2D rb;

    private bool exploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Only move downward.
        rb.linearVelocity =
            Vector2.down * fallSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Explode();
    }

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        DamagePlayers();

        HideProjectile();
    }

    private void DamagePlayers()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius,
                playerLayer
            );

        foreach (Collider2D hit in hits)
        {
            IDamageable damageable =
                hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            damageable.TakeDamage(damage);
        }
    }

    private void HideProjectile()
    {
        if (explosionVisual != null)
        {
            explosionVisual.SetActive(true);
        }

        projectileVisual.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 5);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}