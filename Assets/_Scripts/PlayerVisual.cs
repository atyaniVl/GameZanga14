using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHealth health;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
            health.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (animator == null || movement == null) return;

        animator.SetFloat("Speed", Mathf.Abs(movement.CurrentSpeed));
        animator.SetBool("IsGrounded", movement.IsGrounded);
        animator.SetBool("IsClimbing", movement.IsClimbing);
    }

    private void HandleDamaged()
    {
        if (animator != null) animator.SetTrigger("Hurt");
    }

    private void HandleDeath()
    {
        if (animator != null) animator.SetTrigger("Death");
    }
}