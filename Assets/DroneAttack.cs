using UnityEngine;
using AudioSystem;

public class DroneAttack : EnemyAttack
{
    [Header("References")]
    [SerializeField] private DronePatrol dronePatrol;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private DroneDropProjectile projectilePrefab;

    [Header("Attack")]
    [SerializeField] private float dropInterval = 1.5f;

    private float dropTimer;

    private void Awake()
    {
        if (dronePatrol == null)
        {
            dronePatrol = GetComponent<DronePatrol>();
        }
    }

    private void Update()
    {
        if (!attackEnabled)
            return;

        if (dronePatrol == null)
            return;

        // Only attack while the drone is moving.
        if (!dronePatrol.IsMoving)
        {
            dropTimer = 0f;
            return;
        }

        dropTimer += Time.deltaTime;

        if (dropTimer >= dropInterval)
        {
            dropTimer = 0f;

            DropProjectile();
        }
    }

    private void DropProjectile()
    {
        if (projectilePrefab == null)
            return;

        Transform spawnPoint =
            dropPoint != null
                ? dropPoint
                : transform;

        Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        AudioManager.Instance?.PlaySfxAtPosition("DroneDrop", spawnPoint.position);
    }
}
