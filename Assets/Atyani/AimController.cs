using UnityEngine;
using UnityEngine.InputSystem;

public class AimController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform aimPivot;
    [SerializeField] private Transform playerVisuals;

    private void Update()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouseScreenPosition.x,
                mouseScreenPosition.y,
                -mainCamera.transform.position.z
            )
        );

        Vector2 direction = mouseWorldPosition - aimPivot.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        HandleFlip(direction);
    }

    private void HandleFlip(Vector2 direction)
    {
        Vector3 scale = playerVisuals.localScale;

        if (direction.x > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else if (direction.x < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        playerVisuals.localScale = scale;


        Vector3 scale2 = aimPivot.localScale;

        if (direction.x > 0)
        {
            scale2.y = Mathf.Abs(scale2.y);
        }
        else if (direction.x < 0)
        {
            scale2.y = -Mathf.Abs(scale2.y);
        }

        aimPivot.localScale = scale2;
    }
}