using UnityEngine;
using UnityEngine.InputSystem;

public class AimCursorController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SpriteRenderer cursorRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pullSprite;

    [Header("Charge Colors")]
    [SerializeField] private Gradient chargeGradient;

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                mouseScreenPosition.x,
                mouseScreenPosition.y,
                -mainCamera.transform.position.z
            )
        );

        mouseWorldPosition.z = 0f;

        transform.position = mouseWorldPosition;
    }

    public void SetNormal()
    {
        cursorRenderer.sprite = normalSprite;
        cursorRenderer.color = Color.white;
    }

    public void SetPull()
    {
        cursorRenderer.sprite = pullSprite;
        cursorRenderer.color = chargeGradient.Evaluate(0f);
    }

    public void SetCharge(float chargePercent)
    {
        chargePercent = Mathf.Clamp01(chargePercent);

        cursorRenderer.color =
            chargeGradient.Evaluate(chargePercent);
    }
}