using UnityEngine;
using UnityEngine.InputSystem;

public class AimController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform aimPivot;
    [SerializeField] private Transform playerVisuals;

    public bool FacingRight { get; private set; } = true;

    private void Update()
    {
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

        mouseWorldPosition.z = 0f;

        Vector2 direction =
            mouseWorldPosition - aimPivot.position;

        Aim(direction);
        HandleFlip(direction);
    }

    private void Aim(Vector2 direction)
    {
        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        aimPivot.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleFlip(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) < 0.01f)
            return;

        bool shouldFaceRight = direction.x > 0f;

        if (shouldFaceRight == FacingRight)
            return;

        FacingRight = shouldFaceRight;

        Vector3 visualScale =
            playerVisuals.localScale;

        visualScale.x =
            shouldFaceRight
                ? Mathf.Abs(visualScale.x)
                : -Mathf.Abs(visualScale.x);

        playerVisuals.localScale = visualScale;

        Vector3 aimScale =
            aimPivot.localScale;

        aimScale.y =
            shouldFaceRight
                ? Mathf.Abs(aimScale.y)
                : -Mathf.Abs(aimScale.y);

        aimPivot.localScale = aimScale;
    }
}