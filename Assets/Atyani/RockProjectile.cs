using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    private float speed;
    private Vector2 direction;

    public void Initialize(Vector2 shootDirection, float shootSpeed)
    {
        direction = shootDirection.normalized;
        speed = shootSpeed;
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
    }
}