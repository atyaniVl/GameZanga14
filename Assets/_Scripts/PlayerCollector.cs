using System;
using AudioSystem;
using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShootController shootController;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Pickup Values")]
    [SerializeField] private int rocksPerPickup = 5;
    [SerializeField] private float healthPerPickup = 30f;
    [SerializeField] private ParticleSystem heartPickupParticles;
    [SerializeField] private ParticleSystem keyPickupParticles;
    [SerializeField] private ParticleSystem rocksPickupParticles;

    // ============================================================
    // COLLECTED DATA
    // ============================================================

    public int KeysCollected { get; private set; } = 0;

    public int RocksCollected { get; private set; } = 0;

    // ============================================================
    // EVENTS
    // ============================================================

    public event Action<int> OnKeyCollected;
    public event Action<int> OnRocksCollected;
    public event Action<float> OnHealthPickup;


    // ============================================================
    // INITIALIZATION
    // ============================================================

    private void Awake()
    {
        if (shootController == null)
        {
            shootController =
                GetComponent<ShootController>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                GetComponent<PlayerHealth>();
        }
    }


    // ============================================================
    // COLLECTION
    // ============================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Key"))
        {
            CollectKey(other);
            keyPickupParticles.gameObject.transform.position = other.transform.position;
            keyPickupParticles.Play();
        }
        else if (other.CompareTag("Rock"))
        {
            CollectRocks(other);
            rocksPickupParticles.gameObject.transform.position = other.transform.position;
            rocksPickupParticles.Play();
        }
        else if (other.CompareTag("Heart"))
        {
            CollectHeart(other);
            heartPickupParticles.gameObject.transform.position = other.transform.position;
            heartPickupParticles.Play();
        }
    }


    // ============================================================
    // KEY
    // ============================================================

    private void CollectKey(Collider2D pickup)
    {
        KeysCollected++;

        AudioManager.Instance?.PlaySfxAtPosition("PickupKey", pickup.transform.position);

        OnKeyCollected?.Invoke(
            KeysCollected
        );

        Destroy(pickup.gameObject);
    }


    // ============================================================
    // ROCKS
    // ============================================================

    private void CollectRocks(Collider2D pickup)
    {
        if (shootController == null)
        {
            Debug.LogWarning(
                "PlayerCollector: ShootController is not assigned."
            );

            return;
        }

        shootController.AddRocks(
            rocksPerPickup
        );

        RocksCollected += rocksPerPickup;

        AudioManager.Instance?.PlaySfxAtPositionRandomPitch("PickupRock", pickup.transform.position, 0.94f, 1.06f);

        OnRocksCollected?.Invoke(
            RocksCollected
        );

        Destroy(pickup.gameObject);
    }


    // ============================================================
    // HEART
    // ============================================================

    private void CollectHeart(Collider2D pickup)
    {
        if (playerHealth == null)
        {
            Debug.LogWarning(
                "PlayerCollector: PlayerHealth is not assigned."
            );

            return;
        }

        playerHealth.Heal(
            healthPerPickup
        );

        AudioManager.Instance?.PlaySfxAtPosition("PickupHealth", pickup.transform.position);

        OnHealthPickup?.Invoke(
            healthPerPickup
        );

        Destroy(pickup.gameObject);
    }
}
