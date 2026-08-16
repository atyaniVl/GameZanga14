using UnityEngine;

public class CannonAttack : EnemyAttack
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private CannonProjectile projectilePrefab;
    [SerializeField] private ParticleSystem shootParticle;

    [Header("Direction")]
    [SerializeField] private bool shootRight = true;

    [Header("Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionDistance = 10f;

    [Header("Attack")]
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float projectileSpeed = 8f;

    [Header("Detection Visual")]
    [SerializeField] private LineRenderer detectionLaser;
    [SerializeField] private bool showDetectionLaser = true;
    [SerializeField] private LayerMask detectionBlockerLayer;

    private float fireTimer;

    private void Start()
    {
        fireTimer = fireInterval;
    }

    private void Update()
    {
        if (!attackEnabled)
            return;

        UpdateDetectionLaser();

        fireTimer -= Time.deltaTime;

        if (fireTimer > 0f)
            return;

        if (CanSeePlayer())
        {
            Fire();

            fireTimer = fireInterval;
        }
    }

    private void UpdateDetectionLaser()
    {
        if (detectionLaser == null || firePoint == null)
            return;

        detectionLaser.enabled = showDetectionLaser;

        if (!showDetectionLaser)
            return;

        Vector2 direction = shootRight
            ? Vector2.right
            : Vector2.left;

        Vector2 startPosition = firePoint.position;

        RaycastHit2D hit = Physics2D.Raycast(
            startPosition,
            direction,
            detectionDistance,
            detectionBlockerLayer
        );

        Vector2 endPosition;

        if (hit.collider != null)
        {
            endPosition = hit.point;
        }
        else
        {
            endPosition =
                startPosition +
                direction * detectionDistance;
        }

        detectionLaser.SetPosition(
            0,
            startPosition
        );

        detectionLaser.SetPosition(
            1,
            endPosition
        );
    }
    bool CanSeePlayer()
    {
        if (firePoint == null)
            return false;

        Vector2 direction =
            shootRight
                ? Vector2.right
                : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            direction,
            detectionDistance,
            playerLayer
        );

        return hit.collider != null;
    }

    private void Fire()
    {
        if (firePoint == null)
        {
            Debug.LogError(
                $"{name}: Fire Point is NOT assigned!"
            );

            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError(
                $"{name}: Projectile Prefab is NOT assigned!"
            );

            return;
        }

        Vector2 direction =
            shootRight
                ? Vector2.right
                : Vector2.left;

        // Muzzle flash / shooting particle
        if (shootParticle != null)
        {
            shootParticle.Play();
        }

        CannonProjectile projectile =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

        projectile.Initialize(
            direction,
            projectileSpeed
        );
    }


    private void OnDrawGizmosSelected()
    {
        if (firePoint == null)
            return;

        Vector3 direction = shootRight
            ? Vector3.right
            : Vector3.left;

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            firePoint.position,
            direction * detectionDistance
        );

        // Optional: show the detection endpoint
        Gizmos.DrawWireSphere(
            firePoint.position + direction * detectionDistance,
            0.05f
        );
    }
}