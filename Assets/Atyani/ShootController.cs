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

    private float pullStartTime;
    private float currentShootSpeed;

    private static readonly int AttackPull = Animator.StringToHash("Pull Trigger");
    private static readonly int AttackRelease = Animator.StringToHash("Release Trigger");

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartPull();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Release();
        }
    }

    private void StartPull()
    {
        pullStartTime = Time.time;

        animator.SetTrigger(AttackPull);
    }

    private void Release()
    {
        float holdTime = Time.time - pullStartTime;

        holdTime = Mathf.Clamp(holdTime, 0f, maxHoldTime);

        float chargePercent = holdTime / maxHoldTime;

        currentShootSpeed = Mathf.Lerp(
            minSpeed,
            maxSpeed,
            chargePercent
        );

        animator.SetTrigger(AttackRelease);
    }

    // Called by Animation Event during Attack_Release
    public void Shoot()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouseScreenPosition.x,
                mouseScreenPosition.y,
                -mainCamera.transform.position.z
            )
        );

        Vector2 direction =
            (mouseWorldPosition - firePoint.position).normalized;

        RockProjectile rock = Instantiate(
            rockPrefab,
            firePoint.position,
            Quaternion.identity
        );

        rock.Initialize(direction, currentShootSpeed);
    }
}