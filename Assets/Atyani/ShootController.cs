using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private RockProjectile rockPrefab;
    [SerializeField] private Animator animator;

    [Header("Charge")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float maxHoldTime = 1.5f;

    [Header("Rocks")]
    [SerializeField] private int startingRocks = 60;

    [Header("Sling")]
    [SerializeField] private GameObject slingStringNormal;
    [SerializeField] private GameObject slingStringPull;

    [SerializeField] private AimCursorController aimCursor;

    private float pullStartTime;
    private float currentShootSpeed;

    private int currentRocks;

    public int CurrentRocks => currentRocks;

    public event Action<int> OnRocksChanged;

    private static readonly int AttackPull =
        Animator.StringToHash("Pull Trigger");

    private static readonly int AttackRelease =
        Animator.StringToHash("Release Trigger");


    private void Awake()
    {
        currentRocks = startingRocks;
    }

    private void Start()
    {
        OnRocksChanged?.Invoke(currentRocks);
    }


    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Don't start the attack if there are no rocks.
            if (currentRocks <= 0)
                return;

            StartPull();
        }

        if (Mouse.current.leftButton.isPressed)
        {
            // Only update charge while actually pulling.
            if (currentRocks > 0)
            {
                UpdateChargeVisual();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Release();
        }
    }


    private void UpdateChargeVisual()
    {
        float holdTime =
            Time.time - pullStartTime;

        float chargePercent =
            Mathf.Clamp01(
                holdTime / maxHoldTime
            );

        aimCursor.SetCharge(chargePercent);
    }


    private void StartPull()
    {
        pullStartTime = Time.time;

        animator.SetTrigger(AttackPull);

        slingStringNormal.SetActive(false);
        slingStringPull.SetActive(true);

        aimCursor.SetPull();
    }


    private void Release()
    {
        // Safety check.
        if (currentRocks <= 0)
            return;

        float holdTime =
            Time.time - pullStartTime;

        holdTime =
            Mathf.Clamp(
                holdTime,
                0f,
                maxHoldTime
            );

        float chargePercent =
            holdTime / maxHoldTime;

        currentShootSpeed =
            Mathf.Lerp(
                minSpeed,
                maxSpeed,
                chargePercent
            );

        slingStringNormal.SetActive(true);
        slingStringPull.SetActive(false);

        animator.SetTrigger(AttackRelease);
    }


    // ============================================================
    // ANIMATION EVENT
    // ============================================================

    public void FinishRelease()
    {
        aimCursor.SetNormal();
    }


    // Called by Animation Event during Attack_Release
    public void Shoot()
    {
        // Safety check in case something changed
        // between Release and the animation event.
        if (currentRocks <= 0)
            return;

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    mouseScreenPosition.x,
                    mouseScreenPosition.y,
                    -mainCamera.transform.position.z
                )
            );

        Vector2 direction =
            (
                mouseWorldPosition -
                firePoint.position
            ).normalized;

        RockProjectile rock =
            Instantiate(
                rockPrefab,
                firePoint.position,
                Quaternion.identity
            );

        rock.Initialize(
            direction,
            currentShootSpeed
        );

        // Consume the rock ONLY when the projectile
        // has actually been created.
        currentRocks--;

        OnRocksChanged?.Invoke(
            currentRocks
        );
    }


    // ============================================================
    // ROCK MANAGEMENT
    // ============================================================

    public void AddRocks(int amount)
    {
        if (amount <= 0)
            return;

        currentRocks += amount;

        OnRocksChanged?.Invoke(
            currentRocks
        );
    }

    public bool HasRocks()
    {
        return currentRocks > 0;
    }
}