using Unity.Cinemachine;
using UnityEngine;

public class CameraAimOffset : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform crosshair;
    [SerializeField] private CinemachinePositionComposer positionComposer;

    [Header("Screen Position")]
    [SerializeField] private float leftScreenPosition = -0.23f;
    [SerializeField] private float rightScreenPosition = 0.23f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 5f;

    private float currentX;

    private void Start()
    {
        currentX = positionComposer.Composition.ScreenPosition.x;
    }

    private void LateUpdate()
    {
        if (mainCamera == null ||
            crosshair == null ||
            positionComposer == null)
        {
            return;
        }

        UpdateScreenPosition();
    }

    private void UpdateScreenPosition()
    {
        // Convert crosshair world position
        // into normalized screen coordinates.
        Vector3 viewportPosition =
            mainCamera.WorldToViewportPoint(
                crosshair.position
            );

        // 0 = left edge
        // 0.5 = center
        // 1 = right edge
        float normalizedX =
            Mathf.Clamp01(viewportPosition.x);

        // Convert 0..1 into our camera range.
        float targetX =
            Mathf.Lerp(
                leftScreenPosition,
                rightScreenPosition,
                normalizedX
            );

        // Smooth toward target.
        currentX = Mathf.Lerp(
            currentX,
            targetX,
            smoothSpeed * Time.deltaTime
        );

        Vector2 screenPosition =
            positionComposer.Composition.ScreenPosition;

        screenPosition.x = currentX;

        positionComposer.Composition.ScreenPosition =
            screenPosition;
    }
}