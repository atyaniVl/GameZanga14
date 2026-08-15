using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DronePatrol : EnemyHealth
{
    private enum PatrolState
    {
        Moving,
        Idle,
        Dead
    }

    // ============================================================
    // PATROL
    // ============================================================

    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private bool invertDirection = false;
    [SerializeField] private float idleDuration = 1f;


    // ============================================================
    // MOVEMENT
    // ============================================================

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 8f;

    public bool IsMoving =>
    currentState == PatrolState.Moving;

    public bool IsDead =>
        isDead;
    // ============================================================
    // VISUAL
    // ============================================================

    [Header("Visual")]
    [SerializeField] private Transform visual;
    [SerializeField] private GameObject hitExplosion;
    [SerializeField] private GameObject groundExplosion;
    [SerializeField] private GameObject visuals;

    [SerializeField] private float maxTiltAngle = 10f;
    [SerializeField] private float tiltSpeed = 8f;


    // ============================================================
    // DEATH
    // ============================================================

    [Header("Death")]
    [SerializeField] private float deathFallForce = 1f;
    [SerializeField] private float deathRotationSpeed = 180f;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float groundCheckDelay = 0.05f;


    // ============================================================
    // TARGET
    // ============================================================

    [Header("Target")]
    [SerializeField] private float targetReachedDistance = 0.05f;


    // ============================================================
    // REFERENCES
    // ============================================================

    private Rigidbody2D rb;

    private Collider2D droneCollider;

    private Vector3 originalVisualScale;


    // ============================================================
    // PATROL DATA
    // ============================================================

    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 currentTarget;

    private PatrolState currentState;

    private float idleTimer;
    private float currentSpeed;

    private bool isDead;
    private float deathTimer;


    // ============================================================
    // AWAKE
    // ============================================================

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();

        droneCollider = GetComponent<Collider2D>();

        if (visual != null)
        {
            originalVisualScale = visual.localScale;
        }

        // Drone starts as a flying object.
        rb.gravityScale = 0f;
    }


    // ============================================================
    // START
    // ============================================================

    private void Start()
    {
        Vector2 startPosition = rb.position;

        if (invertDirection)
        {
            pointA =
                startPosition +
                Vector2.right * patrolDistance;

            pointB = startPosition;
        }
        else
        {
            pointA = startPosition;

            pointB =
                startPosition +
                Vector2.right * patrolDistance;
        }

        currentTarget = pointB;

        currentState = PatrolState.Moving;
    }


    // ============================================================
    // FIXED UPDATE
    // ============================================================

    private void FixedUpdate()
    {
        if (isDead)
        {
            HandleDeathMovement();
            return;
        }

        switch (currentState)
        {
            case PatrolState.Moving:
                MoveTowardsTarget();
                break;

            case PatrolState.Idle:
                HandleIdle();
                break;
        }

        UpdateVisual();
    }


    // ============================================================
    // MOVEMENT
    // ============================================================

    private void MoveTowardsTarget()
    {
        Vector2 toTarget =
            currentTarget - rb.position;

        float distance =
            toTarget.magnitude;

        if (distance <= targetReachedDistance)
        {
            ReachTarget();
            return;
        }

        Vector2 direction =
            toTarget.normalized;

        float brakingSpeed =
            Mathf.Sqrt(
                2f *
                deceleration *
                distance
            );

        float targetSpeed =
            Mathf.Min(
                maxSpeed,
                brakingSpeed
            );

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                (
                    currentSpeed < targetSpeed
                        ? acceleration
                        : deceleration
                ) *
                Time.fixedDeltaTime
            );

        rb.linearVelocity =
            direction * currentSpeed;
    }


    // ============================================================
    // VISUAL
    // ============================================================

    private void UpdateVisual()
    {
        if (visual == null)
            return;

        UpdateVisualFlip();

        UpdateVisualTilt();
    }


    // ============================================================
    // SPRITE FLIP
    // ============================================================

    private void UpdateVisualFlip()
    {
        float horizontalDirection =
            rb.linearVelocity.x;

        if (Mathf.Abs(horizontalDirection) < 0.01f)
            return;

        Vector3 scale =
            originalVisualScale;

        // Sprite's original direction is LEFT.
        //
        // Moving LEFT  → original scale
        // Moving RIGHT → flip X

        if (horizontalDirection < 0f)
        {
            scale.x =
                Mathf.Abs(originalVisualScale.x);
        }
        else
        {
            scale.x =
                -Mathf.Abs(originalVisualScale.x);
        }

        visual.localScale = scale;
    }


    // ============================================================
    // VISUAL TILT
    // ============================================================

    private void UpdateVisualTilt()
    {
        float targetTilt = 0f;

        if (currentState == PatrolState.Moving)
        {
            float direction =
                Mathf.Sign(rb.linearVelocity.x);

            targetTilt =
                -direction * maxTiltAngle;
        }

        Quaternion targetRotation =
            Quaternion.Euler(
                0f,
                0f,
                targetTilt
            );

        visual.localRotation =
            Quaternion.Lerp(
                visual.localRotation,
                targetRotation,
                tiltSpeed *
                Time.fixedDeltaTime
            );
    }


    // ============================================================
    // TARGET REACHED
    // ============================================================

    private void ReachTarget()
    {
        rb.position =
            currentTarget;

        rb.linearVelocity =
            Vector2.zero;

        currentSpeed = 0f;

        currentState =
            PatrolState.Idle;

        idleTimer =
            idleDuration;
    }


    // ============================================================
    // IDLE
    // ============================================================

    private void HandleIdle()
    {
        rb.linearVelocity =
            Vector2.zero;

        idleTimer -=
            Time.fixedDeltaTime;

        if (idleTimer <= 0f)
        {
            SwitchTarget();
        }
    }


    // ============================================================
    // SWITCH TARGET
    // ============================================================

    private void SwitchTarget()
    {
        if (currentTarget == pointA)
        {
            currentTarget = pointB;
        }
        else
        {
            currentTarget = pointA;
        }

        currentSpeed = 0f;

        currentState =
            PatrolState.Moving;
    }


    // ============================================================
    // DAMAGE / DEATH
    // ============================================================

    protected override void Die()
    {
        if (isDead)
            return;

        hitExplosion.gameObject.SetActive(true);

        isDead = true;

        currentState =
            PatrolState.Dead;

        currentSpeed = 0f;

        // Stop patrol movement.
        rb.linearVelocity =
            Vector2.zero;

        // Enable gravity so the drone falls.
        rb.gravityScale = 1f;

        // Allow it to rotate while falling.
        rb.constraints =
            RigidbodyConstraints2D.None;

        // Give it a tiny downward impulse.
        rb.AddForce(
            Vector2.down *
            deathFallForce,
            ForceMode2D.Impulse
        );

        // Rotate while falling.
        rb.angularVelocity =
            deathRotationSpeed;

        deathTimer = 0f;
    }


    // ============================================================
    // DEATH MOVEMENT
    // ============================================================

    private void HandleDeathMovement()
    {
        deathTimer +=
            Time.fixedDeltaTime;

        // Don't allow the normal visual
        // patrol tilt to interfere with death rotation.

        if (visual != null)
        {
            visual.localRotation =
                Quaternion.identity;
        }
    }


    // ============================================================
    // GROUND COLLISION
    // ============================================================

    protected override void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        base.OnCollisionEnter2D(collision);

        if (!isDead)
            return;

        if (deathTimer < groundCheckDelay)
            return;

        int collisionLayer =
            collision.gameObject.layer;

        bool isGround =
            (groundLayer.value &
             (1 << collisionLayer)) != 0;

        if (!isGround)
            return;

        HideDrone();
    }


    // ============================================================
    // HIDE
    // ============================================================

    private void HideDrone()
    {
        if (droneCollider != null)
        {
            droneCollider.enabled = false;
        }
        groundExplosion.gameObject.SetActive(true);
        visuals.SetActive(false);
        Destroy(gameObject, 5);
    }


    // ============================================================
    // GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        Vector2 startPoint;
        Vector2 endPoint;

        if (Application.isPlaying)
        {
            startPoint = pointA;
            endPoint = pointB;
        }
        else
        {
            startPoint =
                transform.position;

            endPoint =
                transform.position +
                Vector3.right *
                patrolDistance;

            if (invertDirection)
            {
                Vector2 temp =
                    startPoint;

                startPoint =
                    endPoint;

                endPoint =
                    temp;
            }
        }

        Gizmos.DrawLine(
            startPoint,
            endPoint
        );

        Gizmos.DrawWireSphere(
            startPoint,
            0.15f
        );

        Gizmos.DrawWireSphere(
            endPoint,
            0.15f
        );
    }
}