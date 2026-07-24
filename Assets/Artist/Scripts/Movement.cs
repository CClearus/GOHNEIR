using UnityEngine;

public class Movement : MonoBehaviour
{
    public Rigidbody body;

    [Header("Base Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundMask;

    [Header("Wall Mechanics")]
    [SerializeField] private float wallCheckDistance = 0.8f;
    [SerializeField] private float wallSlideMaxFallSpeed = 2f;
    [SerializeField] private float wallSlideSpeedMultiplier = 0.5f;
    [SerializeField] private float wallJumpForce = 14f;
    [SerializeField] private float wallJumpLockDuration = 0.25f;
    [SerializeField] private int maxWallJumps = 2; // Maximum wall jumps before landing
    [SerializeField] private LayerMask wallMask;

    public bool IsWallSliding { get; private set; }
    public bool CanWallJump => IsWallSliding && wallJumpsRemaining > 0;

    private Vector3 lastWallNormal;
    private float controlLockTimer = 0f;
    private int wallJumpsRemaining;

    private void Start()
    {
        wallJumpsRemaining = maxWallJumps;
    }

    private void Update()
    {
        // Countdown the input lock timer
        if (controlLockTimer > 0)
        {
            controlLockTimer -= Time.deltaTime;
        }

        // Reset wall jump charges when grounded
        if (IsGrounded())
        {
            wallJumpsRemaining = maxWallJumps;
        }
    }

    public void Move(Vector3 inputDir)
    {
        if (controlLockTimer > 0) return;

        HandleWallSlide(inputDir);

        float currentSpeed = speed;
        if (IsWallSliding)
        {
            currentSpeed *= wallSlideSpeedMultiplier;
        }

        Vector3 currentVertical = Vector3.up * body.linearVelocity.y;
        Vector3 targetHorizontal = inputDir * currentSpeed;

        body.linearVelocity = targetHorizontal + currentVertical;
    }

    private void HandleWallSlide(Vector3 inputDir)
    {
        bool isFalling = body.linearVelocity.y < 0;
        bool isPressingWASD = inputDir.sqrMagnitude > 0.01f;

        // Player can only slide if they have wall jump charges left
        if (isFalling && isPressingWASD && wallJumpsRemaining > 0 && IsPushingIntoWall(inputDir))
        {
            IsWallSliding = true;

            if (body.linearVelocity.y < -wallSlideMaxFallSpeed)
            {
                body.linearVelocity = new Vector3(
                    body.linearVelocity.x, 
                    -wallSlideMaxFallSpeed, 
                    body.linearVelocity.z
                );
            }
        }
        else
        {
            IsWallSliding = false;
        }
    }

    private bool IsPushingIntoWall(Vector3 inputDir)
    {
        if (Physics.Raycast(transform.position, inputDir.normalized, out RaycastHit wallHit, wallCheckDistance, wallMask))
        {
            bool isWall = Vector3.Angle(wallHit.normal, Vector3.up) > 60f;
            bool isPushingAgainstFace = Vector3.Dot(inputDir.normalized, wallHit.normal) < -0.3f;

            if (isWall && isPushingAgainstFace)
            {
                lastWallNormal = wallHit.normal;
                return true;
            }
        }

        return false;
    }

    public void Jump()
    {
        if (IsWallSliding && wallJumpsRemaining > 0)
        {
            // Consume one wall jump charge
            wallJumpsRemaining--;

            Vector3 wallJumpDir = (lastWallNormal + Vector3.up).normalized;
            body.linearVelocity = Vector3.zero;
            body.AddForce(wallJumpDir * wallJumpForce, ForceMode.Impulse);

            IsWallSliding = false;
            controlLockTimer = wallJumpLockDuration; 
        }
        else if (IsGrounded())
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }
}