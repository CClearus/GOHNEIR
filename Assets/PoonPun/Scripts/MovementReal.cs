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
    [Tooltip("How fast you speed up toward your wish direction while grounded.")]
    [SerializeField] float groundAccelerate = 10f;
    [Tooltip("How fast you slow down toward 0 while grounded and not pressing a movement key.")]
    [SerializeField] float groundFriction = 6f;
    [Tooltip("How fast Accelerate() can add speed per second while airborne. Higher = faster to reach airMaxSpeed and faster strafe-jump momentum gain.")]
    [SerializeField] float airAccelerate = 60f;
    [Tooltip("The wishspeed cap used by AirAccelerate (NOT your overall air speed cap - that's maxMomentumSpeed below). Keep this low (1-10) so strafing gains speed gradually via repeated small accel ticks instead of snapping straight to top speed.")]
    [SerializeField] float airMaxSpeed = 1f;
    [Tooltip("How freely velocity direction turns toward input while airborne, without losing speed. 0 = no air control, 1 = instant turn.")]
    [SerializeField] float airTurnRate = 6f;
    [Tooltip("Holding Jump auto-hops the instant you land, like Ultrakill.")]
    [SerializeField] bool autoBhop = true;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;
    [Tooltip("Hard cap on horizontal speed gained from bhopping/strafing.")]
    [SerializeField] float maxMomentumSpeed = 20f;

    [Header("Recoil / Knockback")]
    [Tooltip("How fast a recoil-granted speed boost above maxMomentumSpeed bleeds away (units/sec). Lets gun recoil punch past the normal speed cap temporarily, like slide exceed.")]
    [SerializeField] float recoilExceedDecay = 10f;

    [Header("Crouch")]
    [SerializeField] KeyCode crouchKey = KeyCode.C;
    [SerializeField] KeyCode crouchKeyAlt = KeyCode.LeftControl;

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
    float recoilExceedSpeed;
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

    /// Applies an instant velocity impulse (e.g. gun recoil/knockback). Horizontal
    /// component is allowed to punch past maxMomentumSpeed temporarily (decaying via
    /// recoilExceedDecay) instead of being clamped away next frame - lets tricks like
    /// shooting backward to bhop, or shooting the ground to boost upward, actually work.
    public void ApplyRecoil(Vector3 worldImpulse)
    {
        horizontalVelocity += new Vector3(worldImpulse.x, 0f, worldImpulse.z);
        velocity.y += worldImpulse.y;

        float overCap = horizontalVelocity.magnitude - maxMomentumSpeed;
        if (overCap > recoilExceedSpeed) recoilExceedSpeed = overCap;
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
        // the slide bypasses the normal bhop hard cap entirely. A recoil-granted
        // speed boost gets the same treatment temporarily (see ApplyRecoil), so
        // shooting doesn't just get insta-clamped away the next frame.
        float allowedSpeed = maxMomentumSpeed + recoilExceedSpeed;
        if (!isSliding && horizontalVelocity.magnitude > allowedSpeed)
            horizontalVelocity = horizontalVelocity.normalized * allowedSpeed;

        recoilExceedSpeed = Mathf.Max(0f, recoilExceedSpeed - recoilExceedDecay * Time.deltaTime);

        controller.Move(horizontalVelocity * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (velocityText != null)
            velocityText.text = $"Speed: {horizontalVelocity.magnitude:F1}";
    }

    void UpdateCrouch()
    {
        bool crouchHeld = Input.GetKey(crouchKey) || Input.GetKey(crouchKeyAlt);
        if (!crouchHeld)
            suppressCrouchHold = false;

        bool crouchPressed = Input.GetKeyDown(crouchKey) || Input.GetKeyDown(crouchKeyAlt);
        if (crouchPressed && isGrounded && !isSliding && horizontalVelocity.magnitude >= slideMinSpeed)
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
