using System.Collections;
using AudioSystem;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Mine : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    [Header("Explosion")]
    [SerializeField] private float explosionDelay = 1f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int damage = 15;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual")]
    [SerializeField] private GameObject explosionVisual;
    [SerializeField] private GameObject mineVisual;

    private bool isTriggered;
    private bool hasExploded;

    private Collider2D mineCollider;

    private void Awake()
    {
        mineCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered || hasExploded)
            return;

        if (!other.CompareTag(playerTag))
            return;

        TriggerMine();
    }

    private void TriggerMine()
    {
        isTriggered = true;

        AudioManager.Instance?.PlaySfxAtPosition("MineWarning", transform.position);

        StartCoroutine(ExplosionCountdown());
    }

    private IEnumerator ExplosionCountdown()
    {
        yield return new WaitForSeconds(explosionDelay);

        Explode();
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        AudioManager.Instance?.PlaySfxAtPosition("MineExplosion", transform.position);

        DamagePlayer();

        HideMine();
    }

    private void DamagePlayer()
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

    private void HideMine()
    {
        if (mineCollider != null)
        {
            mineCollider.enabled = false;
        }

        if (explosionVisual != null)
        {
            explosionVisual.SetActive(true);
        }

        mineVisual.SetActive(false);
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
