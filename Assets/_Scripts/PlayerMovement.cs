using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using MoreMountains.Feedbacks; // استدعاء حزمة Feel مباشرة

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // ============================================================
    // MOVEMENT
    // ============================================================

    [Header("Movement")]
    [SerializeField] private float maxMoveSpeed = 5f;

    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float groundDeceleration = 70f;

    [SerializeField] private float airAcceleration = 35f;
    [SerializeField] private float airDeceleration = 20f;

    [SerializeField] private float airControl = 1f;


    // ============================================================
    // JUMP
    // ============================================================

    [Header("Jump")]
    [SerializeField] private float jumpForce = 11f;

    [Tooltip("Time after leaving the ground during which the player can still jump.")]
    [SerializeField] private float coyoteTime = 0.12f;

    [Tooltip("Time before landing during which a jump input is remembered.")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Tooltip("How strongly releasing jump early cuts the jump.")]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [SerializeField] private KeyCode jumpKey = KeyCode.Space;


    // ============================================================
    // LADDER
    // ============================================================

    [Header("Ladder")]
    [SerializeField] private float climbSpeed = 4f;

    [SerializeField] private LayerMask ladderLayer;

    [Tooltip("Area used to detect if the player is touching a ladder.")]
    [SerializeField] private Vector2 ladderCheckSize = new Vector2(0.45f, 1.0f);

    [Tooltip("Horizontal input required to leave the ladder.")]
    [SerializeField] private float ladderExitInput = 0.5f;

    [Tooltip("Distance moved on ladder before triggering a climb step feedback.")]
    [SerializeField] private float climbStepInterval = 0.5f;


    // ============================================================
    // GROUND CHECK
    // ============================================================

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;

    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private LayerMask groundLayer;


    // ============================================================
    // VISUAL
    // ============================================================

    [Header("Visual")]
    [SerializeField] private Transform visual;


    // ============================================================
    // FEEL FEEDBACKS
    // ============================================================

    [Header("Feel Feedbacks - Jump & Air")]

    [Tooltip("Played when the player jumps.")]
    [SerializeField] private MMF_Player jumpFeedback;

    [Tooltip("Played when the player cuts their jump early.")]
    [SerializeField] private MMF_Player jumpCutFeedback;

    [Tooltip("Played when the player lands on the ground.")]
    [SerializeField] private MMF_Player landFeedback;

    [Header("Feel Feedbacks - Movement")]

    [Tooltip("Played continuously or looped while walking on ground.")]
    [SerializeField] private MMF_Player walkFeedback;

    [Tooltip("Played once when the player comes to a complete stop on ground.")]
    [SerializeField] private MMF_Player stopFeedback;

    [Tooltip("Played when the player changes movement direction.")]
    [SerializeField] private MMF_Player turnFeedback;

    [Header("Feel Feedbacks - Ladder")]

    [Tooltip("Played when the player starts climbing.")]
    [SerializeField] private MMF_Player climbStartFeedback;

    [Tooltip("Played when the player stops climbing.")]
    [SerializeField] private MMF_Player climbStopFeedback;

    [Tooltip("Played periodically while moving up/down a ladder.")]
    [SerializeField] private MMF_Player climbStepFeedback;


    // ============================================================
    // REFERENCES & STATE
    // ============================================================

    private Rigidbody2D rb;

    private float moveInput;
    private float verticalInput;

    private bool jumpRequested;
    private bool jumpHeld;

    private bool isGrounded;
    private bool wasGrounded;

    private bool isTouchingLadder;
    private bool isClimbing;

    private float originalGravity;
    private float lastYVelocity;
    private float climbDistanceTracker;

    private float coyoteTimer;
    private float jumpBufferTimer;


    // ============================================================
    // PUBLIC PROPERTIES
    // ============================================================

    public bool FacingRight { get; private set; } = true;

    public bool IsGrounded => isGrounded;

    public bool IsClimbing => isClimbing;

    public float CurrentSpeed => rb != null ? rb.linearVelocity.x : 0f;

    public float MoveInput => moveInput;


    // ============================================================
    // AWAKE
    // ============================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        originalGravity = rb.gravityScale;

        if (visual == null)
        {
            visual = transform.Find("Visual");
        }
    }


    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        HandleInput();

        CheckGrounded();

        CheckLadder();

        UpdateJumpTimers();

        HandleJumpRelease();

        HandleMovementFeedbacks();
    }


    // ============================================================
    // INPUT
    // ============================================================

    private void HandleInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        bool jumpPressed = false;

        // ========================================================
        // NEW INPUT SYSTEM
        // ========================================================

#if ENABLE_INPUT_SYSTEM

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal = -1f;
            }
            else if (Keyboard.current.dKey.isPressed ||
                     Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal = 1f;
            }

            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
            {
                vertical = 1f;
            }
            else if (Keyboard.current.sKey.isPressed ||
                     Keyboard.current.downArrowKey.isPressed)
            {
                vertical = -1f;
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpPressed = true;
            }

            jumpHeld = Keyboard.current.spaceKey.isPressed;
        }

#endif

        // ========================================================
        // LEGACY INPUT
        // ========================================================

#if ENABLE_LEGACY_INPUT_MANAGER

        float legacyHorizontal = Input.GetAxisRaw("Horizontal");

        if (legacyHorizontal != 0f)
        {
            horizontal = legacyHorizontal;
        }

        float legacyVertical = Input.GetAxisRaw("Vertical");

        if (legacyVertical != 0f)
        {
            vertical = legacyVertical;
        }

        if (Input.GetKeyDown(jumpKey) ||
            Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }

        if (Input.GetKey(jumpKey) ||
            Input.GetButton("Jump"))
        {
            jumpHeld = true;
        }

#endif

        SetMoveInput(horizontal);

        verticalInput = Mathf.Clamp(vertical, -1f, 1f);

        if (jumpPressed)
        {
            RequestJump();
        }
    }


    // ============================================================
    // FIXED UPDATE
    // ============================================================

    private void FixedUpdate()
    {
        lastYVelocity = rb.linearVelocity.y;

        HandleClimbingState();

        if (isClimbing)
        {
            HandleClimbingMovement();
        }
        else
        {
            HandleNormalMovement();

            HandleJump();
        }

        HandleFacing();

        wasGrounded = isGrounded;
    }


    // ============================================================
    // NORMAL MOVEMENT
    // ============================================================

    private void HandleNormalMovement()
    {
        rb.gravityScale = originalGravity;

        float targetSpeed = moveInput * maxMoveSpeed;

        float acceleration;

        bool hasInput = Mathf.Abs(moveInput) > 0.01f;

        if (isGrounded)
        {
            acceleration = hasInput
                ? groundAcceleration
                : groundDeceleration;
        }
        else
        {
            acceleration = hasInput
                ? airAcceleration
                : airDeceleration;

            acceleration *= airControl;
        }

        float newX = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newX,
            rb.linearVelocity.y
        );
    }


    // ============================================================
    // JUMP
    // ============================================================

    private void HandleJump()
    {
        if (!jumpRequested)
            return;

        if (jumpBufferTimer <= 0f)
            return;

        if (coyoteTimer <= 0f)
            return;

        if (isClimbing)
            return;

        // Execute jump
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        // Reset jump state
        jumpRequested = false;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;

        // Feel
        PlayFeedback(jumpFeedback);
    }


    // ============================================================
    // JUMP RELEASE
    // ============================================================

    private void HandleJumpRelease()
    {
        if (!jumpHeld &&
            rb.linearVelocity.y > 0f &&
            !isClimbing)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );

            PlayFeedback(jumpCutFeedback);
        }
    }


    // ============================================================
    // JUMP TIMERS
    // ============================================================

    private void UpdateJumpTimers()
    {
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpRequested)
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }


    // ============================================================
    // GROUND CHECK
    // ============================================================

    private void CheckGrounded()
    {
        if (groundCheckPoint == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        ) != null;

        // LANDING DETECTION
        if (isGrounded && !wasGrounded)
        {
            PlayFeedback(landFeedback);
        }
    }


    // ============================================================
    // LADDER CHECK
    // ============================================================

    private void CheckLadder()
    {
        Collider2D ladderHit = Physics2D.OverlapBox(
            transform.position,
            ladderCheckSize,
            0f,
            ladderLayer
        );

        isTouchingLadder = ladderHit != null;

        if (isClimbing && !isTouchingLadder)
        {
            StopClimbing();
        }
    }


    // ============================================================
    // CLIMBING STATE
    // ============================================================

    private void HandleClimbingState()
    {
        if (!isClimbing &&
            isTouchingLadder &&
            Mathf.Abs(verticalInput) > 0.1f)
        {
            StartClimbing();
            return;
        }

        if (isClimbing &&
            isGrounded &&
            Mathf.Abs(moveInput) >= ladderExitInput)
        {
            StopClimbing();
        }
    }


    // ============================================================
    // START CLIMBING
    // ============================================================

    private void StartClimbing()
    {
        if (isClimbing)
            return;

        isClimbing = true;

        rb.gravityScale = 0f;

        rb.linearVelocity = Vector2.zero;

        climbDistanceTracker = 0f;

        PlayFeedback(climbStartFeedback);
    }


    // ============================================================
    // STOP CLIMBING
    // ============================================================

    private void StopClimbing()
    {
        if (!isClimbing)
            return;

        isClimbing = false;

        rb.gravityScale = originalGravity;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            rb.linearVelocity.y
        );

        PlayFeedback(climbStopFeedback);
    }


    // ============================================================
    // CLIMBING MOVEMENT
    // ============================================================

    private void HandleClimbingMovement()
    {
        rb.gravityScale = 0f;

        if (isGrounded && Mathf.Abs(moveInput) >= ladderExitInput)
        {
            StopClimbing();

            HandleNormalMovement();

            return;
        }

        rb.linearVelocity = new Vector2(
            0f,
            verticalInput * climbSpeed
        );

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            climbDistanceTracker += climbSpeed * Time.fixedDeltaTime;

            if (climbDistanceTracker >= climbStepInterval)
            {
                climbDistanceTracker = 0f;

                PlayFeedback(climbStepFeedback);
            }
        }

        jumpRequested = false;
        jumpBufferTimer = 0f;
    }


    // ============================================================
    // FACING
    // ============================================================

    private void HandleFacing()
    {
        if (isClimbing && !isGrounded)
            return;

        if (moveInput > 0.01f && !FacingRight)
        {
            Flip();
        }
        else if (moveInput < -0.01f && FacingRight)
        {
            Flip();
        }
    }


    // ============================================================
    // FLIP
    // ============================================================

    private void Flip()
    {
        FacingRight = !FacingRight;

        if (visual == null)
            return;

        Vector3 scale = visual.localScale;

        scale.x *= -1f;

        visual.localScale = scale;

        PlayFeedback(turnFeedback);
    }


    // ============================================================
    // MOVEMENT & STOP FEEDBACKS
    // ============================================================

    private void HandleMovementFeedbacks()
    {
        bool isMovingHorizontally = Mathf.Abs(rb.linearVelocity.x) > 0.1f && Mathf.Abs(moveInput) > 0.01f;

        if (isGrounded && isMovingHorizontally && !isClimbing)
        {
            if (walkFeedback != null && !walkFeedback.IsPlaying)
            {
                walkFeedback.PlayFeedbacks(transform.position);
            }
        }
        else
        {
            if (walkFeedback != null && walkFeedback.IsPlaying)
            {
                walkFeedback.StopFeedbacks();
            }
        }

        if (isGrounded && wasGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.05f && Mathf.Abs(moveInput) < 0.01f)
        {
            if (stopFeedback != null && !stopFeedback.IsPlaying && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            {
                PlayFeedback(stopFeedback);
            }
        }
    }


    // ============================================================
    // PUBLIC METHODS
    // ============================================================

    public void SetMoveInput(float direction)
    {
        moveInput = Mathf.Clamp(
            direction,
            -1f,
            1f
        );
    }


    public void RequestJump()
    {
        jumpRequested = true;
    }


    // ============================================================
    // FEEL
    // ============================================================

    private void PlayFeedback(MMF_Player feedback)
    {
        if (feedback == null)
            return;

        feedback.PlayFeedbacks(
            transform.position
        );
    }


    // ============================================================
    // GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                groundCheckPoint.position,
                groundCheckRadius
            );
        }

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(
            transform.position,
            ladderCheckSize
        );
    }
}