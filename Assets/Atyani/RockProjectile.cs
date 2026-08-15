using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    public void Initialize(Vector2 direction, float speed)
    {
        direction.Normalize();

        rb.linearVelocity = direction * speed;

        Destroy(gameObject, 5f);
    }
}