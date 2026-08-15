using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Invincibility (i-frames)")]
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("Damage Visual")]
    [SerializeField] private SpriteRenderer[] playerSprites;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.1f;

    private bool isInvincible = false;

    private Color[] originalColors;
    private Coroutine damageFlashCoroutine;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead { get; private set; } = false;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDamaged;
    public event Action OnDeath;


    // ============================================================
    // INITIALIZATION
    // ============================================================

    private void Awake()
    {
        currentHealth = maxHealth;

        // Automatically find all SpriteRenderers
        // belonging to the player and its children.
        if (playerSprites == null || playerSprites.Length == 0)
        {
            playerSprites =
                GetComponentsInChildren<SpriteRenderer>(true);
        }

        // Store each sprite's original color.
        originalColors = new Color[playerSprites.Length];

        for (int i = 0; i < playerSprites.Length; i++)
        {
            originalColors[i] =
                playerSprites[i].color;
        }
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }


    // ============================================================
    // DAMAGE
    // ============================================================

    public void TakeDamage(int amount)
    {
        if (IsDead || isInvincible || amount <= 0)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth - amount,
            0f,
            maxHealth
        );

        OnDamaged?.Invoke();

        PlayDamageFlash();

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            StartCoroutine(
                InvincibilityFrames()
            );
        }
    }


    // ============================================================
    // DAMAGE FLASH
    // ============================================================

    private void PlayDamageFlash()
    {
        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }

        damageFlashCoroutine =
            StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        // Change every player part to damage color.
        for (int i = 0; i < playerSprites.Length; i++)
        {
            if (playerSprites[i] != null)
            {
                playerSprites[i].color =
                    damageColor;
            }
        }

        yield return new WaitForSeconds(
            damageFlashDuration
        );

        // Restore original colors.
        RestoreOriginalColors();

        damageFlashCoroutine = null;
    }

    private void RestoreOriginalColors()
    {
        for (int i = 0; i < playerSprites.Length; i++)
        {
            if (playerSprites[i] != null)
            {
                playerSprites[i].color =
                    originalColors[i];
            }
        }
    }


    // ============================================================
    // HEAL
    // ============================================================

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth + amount,
            0f,
            maxHealth
        );

        OnHealthChanged?.Invoke(
            currentHealth,
            maxHealth
        );
    }


    // ============================================================
    // DEATH
    // ============================================================

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        OnDeath?.Invoke();
    }


    // ============================================================
    // INVINCIBILITY
    // ============================================================

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        yield return new WaitForSeconds(
            invincibilityDuration
        );

        isInvincible = false;
    }
}