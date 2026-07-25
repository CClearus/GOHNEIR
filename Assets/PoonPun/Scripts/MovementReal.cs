using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementdiddy : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text velocityText;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] KeyCode walkKey = KeyCode.LeftShift;

    [SerializeField] float jumpHeight = 1.5f;
    [SerializeField] float gravity = -9.81f;

    [Header("Bhop / Air Strafing")]
    [SerializeField] float groundAccelerate = 10f;
    [SerializeField] float groundFriction = 6f;
    [SerializeField] float airAccelerate = 60f;
    [SerializeField] float airMaxSpeed = 1f;
    [Tooltip("How freely velocity direction turns toward input while airborne, without losing speed. 0 = no air control, 1 = instant turn.")]
    [SerializeField] float airTurnRate = 6f;
    [Tooltip("Holding Jump auto-hops the instant you land, like Ultrakill.")]
    [SerializeField] bool autoBhop = true;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;
    [Tooltip("Hard cap on horizontal speed gained from bhopping/strafing.")]
    [SerializeField] float maxMomentumSpeed = 20f;

    [Header("Crouch")]
    [SerializeField] KeyCode crouchKey = KeyCode.C;

    [Header("Slide")]
    [Tooltip("Minimum ground speed required to start a slide.")]
    [SerializeField] float slideMinSpeed = 4f;
    [Tooltip("Fixed speed the slide's own decaying part starts at (the 'y' in y+exceed -> 0+exceed). Uncapped - exceed bhop momentum stacks on top with no limit.")]
    [SerializeField] float slideMaxSpeed = 12f;
    [Tooltip("How fast the slide's own speed bleeds from slideMaxSpeed down to 0. Does not affect saved exceed momentum.")]
    [SerializeField] float slideFriction = 1.5f;
    [Tooltip("The slide's own speed counts as 'hit 0' once it decays below this.")]
    [SerializeField] float slideEndSpeed = 0.2f;

    float standHeight;
    Vector3 standCenter;
    bool isCrouching;
    bool isSliding;
    bool suppressCrouchHold;
    Vector3 slideDir;
    float slideSpeed;
    float slideExceedSpeed;
    public bool IsCrouching => isCrouching;
    public bool IsSliding => isSliding;

    CharacterController controller;
    Vector3 velocity;
    Vector3 horizontalVelocity;
    bool isGrounded;
    float coyoteTimer;
    float jumpBufferTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        standHeight = controller.height;
        standCenter = controller.center;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        UpdateCrouch();

        coyoteTimer = isGrounded ? coyoteTime : coyoteTimer - Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;
        if (Input.GetButtonDown("Jump") || (autoBhop && Input.GetButton("Jump")))
            jumpBufferTimer = jumpBufferTime;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 wishDir = (transform.right * horizontal + transform.forward * vertical).normalized;
        float wishSpeed = Input.GetButton("Fire3") ? sprintSpeed : moveSpeed;
        if (Input.GetKey(walkKey)) wishSpeed = moveSpeed * 0.5f;

        if (isGrounded)
        {
            if (isSliding)
            {
                ApplySlideFriction();
            }
            else
            {
                ApplyFriction();
                Accelerate(wishDir, wishSpeed, groundAccelerate);
            }
        }
        else
        {
            AirControl(wishDir);
            AirAccelerate(wishDir, wishSpeed);
        }

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isGrounded = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
        }

        // Sliding has no momentum limit - exceed speed carried into/through
        // the slide bypasses the normal bhop hard cap entirely.
        if (!isSliding && horizontalVelocity.magnitude > maxMomentumSpeed)
            horizontalVelocity = horizontalVelocity.normalized * maxMomentumSpeed;

        controller.Move(horizontalVelocity * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (velocityText != null)
            velocityText.text = $"Speed: {horizontalVelocity.magnitude:F1}";
    }

    void UpdateCrouch()
    {
        bool crouchHeld = Input.GetKey(crouchKey);
        if (!crouchHeld)
            suppressCrouchHold = false;

        if (Input.GetKeyDown(crouchKey) && isGrounded && !isSliding && horizontalVelocity.magnitude >= slideMinSpeed)
            StartSlide();

        // Releasing crouch or leaving the ground cancels the slide early -
        // whatever slide speed is left gets folded into the saved exceed
        // momentum instead of being lost.
        if (isSliding && (!crouchHeld || !isGrounded))
            CancelSlide();

        bool wantCrouch = crouchHeld && !suppressCrouchHold;
        if (wantCrouch == isCrouching) return;
        isCrouching = wantCrouch;

        controller.height = wantCrouch ? standHeight * 0.5f : standHeight;
        controller.center = wantCrouch ? standCenter * 0.5f : standCenter;
    }

    void StartSlide()
    {
        isSliding = true;
        slideDir = horizontalVelocity.sqrMagnitude > 0.001f ? horizontalVelocity.normalized : transform.forward;

        // No cap: any momentum above the normal ground top speed (bhop
        // exceed) is saved aside untouched and stacks on top of the slide's
        // fixed start speed, uncapped. Slide runs slideMaxSpeed+exceed -> 0+exceed.
        float baseSpeed = horizontalVelocity.magnitude;
        float normalTopSpeed = Mathf.Max(moveSpeed, sprintSpeed);
        slideExceedSpeed = Mathf.Max(0f, baseSpeed - normalTopSpeed);
        slideSpeed = slideMaxSpeed;

        horizontalVelocity = slideDir * (slideSpeed + slideExceedSpeed);
    }

    void ApplySlideFriction()
    {
        float drop = slideSpeed * slideFriction * Time.deltaTime;
        slideSpeed = Mathf.Max(slideSpeed - drop, 0f);
        horizontalVelocity = slideDir * (slideSpeed + slideExceedSpeed);

        // Slide ran its course - auto stand up, keeping only the saved
        // exceed momentum, which then feeds straight back into bhop.
        if (slideSpeed <= slideEndSpeed)
            EndSlide(forceStand: true);
    }

    void CancelSlide()
    {
        slideExceedSpeed += slideSpeed;
        slideSpeed = 0f;
        horizontalVelocity = slideDir * slideExceedSpeed;
        EndSlide(forceStand: false);
    }

    void EndSlide(bool forceStand)
    {
        isSliding = false;
        if (!forceStand) return;

        isCrouching = false;
        suppressCrouchHold = true;
        controller.height = standHeight;
        controller.center = standCenter;
    }

    // Ultrakill-style easy air control: turns velocity direction toward wishDir
    // while keeping its speed, so changing direction in the air never slows you down.
    void AirControl(Vector3 wishDir)
    {
        if (wishDir.sqrMagnitude < 0.001f) return;

        float speed = horizontalVelocity.magnitude;
        if (speed < 0.001f) return;

        Vector3 currentDir = horizontalVelocity / speed;
        Vector3 newDir = Vector3.Slerp(currentDir, wishDir, airTurnRate * Time.deltaTime).normalized;
        horizontalVelocity = newDir * speed;
    }

    void ApplyFriction()
    {
        float speed = horizontalVelocity.magnitude;
        if (speed < 0.001f)
        {
            horizontalVelocity = Vector3.zero;
            return;
        }

        float drop = speed * groundFriction * Time.deltaTime;
        float newSpeed = Mathf.Max(speed - drop, 0f);
        horizontalVelocity *= newSpeed / speed;
    }

    void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
    {
        float currentSpeed = Vector3.Dot(horizontalVelocity, wishDir);
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0f) return;

        float accelSpeed = Mathf.Min(accel * wishSpeed * Time.deltaTime, addSpeed);
        horizontalVelocity += wishDir * accelSpeed;
    }

    void AirAccelerate(Vector3 wishDir, float wishSpeed)
    {
        // Cap the wishspeed used for air accel (Source clamps this to 30u/s
        // equivalent) so bhopping/strafing gains speed instead of snapping to it.
        float cappedWishSpeed = Mathf.Min(wishSpeed, airMaxSpeed);
        Accelerate(wishDir, cappedWishSpeed, airAccelerate);
    }
}
