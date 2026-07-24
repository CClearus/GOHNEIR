using UnityEngine;

public class Movement : MonoBehaviour
{
    public Rigidbody body;

    [Header("Base Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Wall Slide Settings")]
    [SerializeField] private float wallCheckDistance = 0.8f;      // How far to check for walls around player
    [SerializeField] private float wallSlideMaxFallSpeed = 2f;    // Maximum descent speed while sliding
    [SerializeField] private float wallSlideSpeedMultiplier = 0.5f; // Slower movement while on wall
    [SerializeField] private LayerMask wallMask;

    public bool IsWallSliding { get; private set; }

    private void FixedUpdate()
    {
        HandleWallSlide();
    }

    public void Move(Vector3 dir)
    {
        float currentSpeed = speed;

        // Slow down horizontal movement speed if hugging a wall
        if (IsWallSliding)
        {
            currentSpeed *= wallSlideSpeedMultiplier;
        }

        Vector3 currentVertical = Vector3.up * body.linearVelocity.y;
        Vector3 targetHorizontal = dir * currentSpeed;

        body.linearVelocity = targetHorizontal + currentVertical;
    }

    private void HandleWallSlide()
    {
        bool isTouchingWall = CheckForWall(out RaycastHit wallHit);
        bool isFalling = body.linearVelocity.y < 0;

        // Trigger wall slide when falling against a vertical wall surface
        if (isTouchingWall && isFalling)
        {
            IsWallSliding = true;

            // Cap downward fall speed
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

    private bool CheckForWall(out RaycastHit wallHit)
    {
        // Cast rays in 4 directions around player (Forward, Back, Left, Right)
        Vector3[] checkDirections = { transform.forward, -transform.forward, transform.right, -transform.right };

        foreach (Vector3 dir in checkDirections)
        {
            if (Physics.Raycast(transform.position, dir, out wallHit, wallCheckDistance, wallMask))
            {
                // Verify the hit surface is steep enough to be considered a wall (Angle > 60°)
                if (Vector3.Angle(wallHit.normal, Vector3.up) > 60f)
                {
                    return true;
                }
            }
        }

        wallHit = default;
        return false;
    }

    public void Jump()
    {
        body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}