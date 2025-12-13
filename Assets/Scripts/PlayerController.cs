using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 12f;
    public float sprintMultiplier = 1.5f;
    public float jumpForce = 8f;
    public float gravity = 20f;

    [Header("Quake Movement")]
    public float airAcceleration = 100f;    // How fast you accelerate in air
    public float airMaxSpeed = 1.5f;        // Max speed gain per air strafe (relative to moveSpeed)
    public float groundFriction = 6f;       // Ground friction for speed decay
    public float maxBunnySpeed = 30f;       // Max speed from bunny hopping

    [Header("Fall Settings")]
    public float fallGravityMultiplier = 2.5f;  // Faster falling
    public float fallAirControlMultiplier = 0.1f; // Very weak steering when falling

    [Header("Dash")]
    public float dashDistance = 8f;         // How far the dash goes
    public float dashDuration = 0.1f;       // How long the dash takes
    public float dashCooldown = 1f;         // Cooldown between dashes
    public float doubleTapWindow = 0.3f;    // Window for double-tap detection

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 89f;

    [Header("References")]
    public Transform cameraHolder;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;

    // Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool sprintPressed;

    // Bunny hop
    private bool wasGrounded;
    private float bunnyHopWindow = 0.15f;
    private float lastGroundedTime;
    private bool jumpHeld; // Track if jump is being held

    // Double jump
    private int jumpCount = 0;
    private int maxJumps = 2;

    // Rail jump timing
    private float lastJumpPressTime = -999f;
    private float railJumpWindow = 0.15f; // Window for simultaneous jump+shoot bonus

    // Dash
    private float lastShiftPressTime = -999f;
    private float lastDashTime = -999f;
    private bool isDashing = false;
    private Vector3 dashDirection;
    private float dashTimeRemaining;

    // Death state
    private bool isDead = false;
    public bool IsDead => isDead;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraHolder == null)
        {
            cameraHolder = Camera.main?.transform;
        }

        // Don't lock cursor here - MainMenu will handle it
    }

    void Update()
    {
        // Don't process input when game is paused
        if (Time.timeScale == 0f) return;
        if (isDead) return;

        HandleInput();
        HandleMouseLook();
        HandleMovement();
    }

    void HandleInput()
    {
        // Keyboard input
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null || mouse == null) return;

        // Movement
        moveInput = Vector2.zero;
        if (keyboard.wKey.isPressed) moveInput.y += 1;
        if (keyboard.sKey.isPressed) moveInput.y -= 1;
        if (keyboard.aKey.isPressed) moveInput.x -= 1;
        if (keyboard.dKey.isPressed) moveInput.x += 1;

        // Normalize diagonal movement
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();

        // Sprint
        sprintPressed = keyboard.shiftKey.isPressed;

        // Dash - double tap SHIFT
        if (keyboard.shiftKey.wasPressedThisFrame)
        {
            float timeSinceLastShift = Time.time - lastShiftPressTime;

            if (timeSinceLastShift <= doubleTapWindow && CanDash())
            {
                TryDash();
            }

            lastShiftPressTime = Time.time;
        }

        // Jump
        jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
        jumpHeld = keyboard.spaceKey.isPressed;
        if (jumpPressed)
            lastJumpPressTime = Time.time;

        // Mouse look
        lookInput = mouse.delta.ReadValue() * 0.1f;
    }

    void HandleMouseLook()
    {
        // Horizontal rotation - rotate player
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        // Vertical rotation - rotate camera
        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        // Handle dash
        if (isDashing)
        {
            HandleDash();
            return;
        }

        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        // Just landed this frame
        bool justLanded = isGrounded && !wasGrounded;

        // Reset jump count when grounded
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            jumpCount = 0;
        }

        // Calculate wish direction (where player wants to move)
        Vector3 wishDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (wishDir.magnitude > 1f)
            wishDir.Normalize();

        float currentSpeed = moveSpeed;
        if (sprintPressed)
            currentSpeed *= sprintMultiplier;

        if (isGrounded)
        {
            // Bunny hop: if holding jump when landing, skip friction and jump immediately
            bool bunnyHop = justLanded && jumpHeld;

            if (!bunnyHop)
            {
                // Normal ground: apply friction
                ApplyFriction();
            }

            // Ground movement
            GroundAccelerate(wishDir, currentSpeed);

            // Reset vertical velocity
            if (velocity.y < 0)
                velocity.y = -2f;

            // Jump from ground
            if (jumpPressed || bunnyHop)
            {
                velocity.y = jumpForce;
                jumpCount = 1;
            }
        }
        else
        {
            // Different air control based on rising or falling
            bool isFalling = velocity.y <= 0;

            if (isFalling)
            {
                // Falling: very weak air control
                AirAccelerate(wishDir, currentSpeed, fallAirControlMultiplier);
            }
            else
            {
                // Rising: full air control
                AirAccelerate(wishDir, currentSpeed, 1f);
            }

            // Double jump - ONLY if not sprinting
            if (jumpPressed && jumpCount < maxJumps)
            {
                if (sprintPressed)
                {
                    // Sprinting = no double jump allowed
                }
                else
                {
                    velocity.y = jumpForce;
                    jumpCount++;
                }
            }
        }

        // Apply gravity - faster when falling
        if (velocity.y <= 0)
            velocity.y -= gravity * fallGravityMultiplier * Time.deltaTime;
        else
            velocity.y -= gravity * Time.deltaTime;

        // Move
        controller.Move(velocity * Time.deltaTime);
    }

    void ApplyFriction()
    {
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        float speed = horizontalVel.magnitude;

        if (speed < 0.1f) return;

        float drop = speed * groundFriction * Time.deltaTime;
        float newSpeed = Mathf.Max(speed - drop, 0);

        if (speed > 0)
        {
            velocity.x *= newSpeed / speed;
            velocity.z *= newSpeed / speed;
        }
    }

    void GroundAccelerate(Vector3 wishDir, float wishSpeed)
    {
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        float currentSpeed = Vector3.Dot(horizontalVel, wishDir);
        float addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0) return;

        float accel = wishSpeed * 10f * Time.deltaTime;
        if (accel > addSpeed)
            accel = addSpeed;

        velocity.x += wishDir.x * accel;
        velocity.z += wishDir.z * accel;

        // Clamp to max bunny speed
        horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        if (horizontalVel.magnitude > maxBunnySpeed)
        {
            horizontalVel = horizontalVel.normalized * maxBunnySpeed;
            velocity.x = horizontalVel.x;
            velocity.z = horizontalVel.z;
        }
    }

    void AirAccelerate(Vector3 wishDir, float wishSpeed, float controlMultiplier = 1f)
    {
        // Quake air acceleration - only accelerate in wish direction up to a limit
        float maxAirSpeed = moveSpeed * airMaxSpeed;

        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        float currentSpeed = Vector3.Dot(horizontalVel, wishDir);

        // Only accelerate if we're below the air speed cap in wish direction
        float addSpeed = Mathf.Min(maxAirSpeed, wishSpeed) - currentSpeed;
        if (addSpeed <= 0) return;

        float accel = airAcceleration * controlMultiplier * Time.deltaTime;
        if (accel > addSpeed)
            accel = addSpeed;

        velocity.x += wishDir.x * accel;
        velocity.z += wishDir.z * accel;

        // Clamp to max bunny speed
        horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        if (horizontalVel.magnitude > maxBunnySpeed)
        {
            horizontalVel = horizontalVel.normalized * maxBunnySpeed;
            velocity.x = horizontalVel.x;
            velocity.z = horizontalVel.z;
        }
    }

    bool CanDash()
    {
        return Time.time >= lastDashTime + dashCooldown;
    }

    void TryDash()
    {
        // Get dash direction from WASD input
        Vector3 dashDir = transform.right * moveInput.x + transform.forward * moveInput.y;

        // If no input, dash forward
        if (dashDir.magnitude < 0.1f)
            dashDir = transform.forward;
        else
            dashDir.Normalize();

        // Start dash
        isDashing = true;
        dashDirection = dashDir;
        dashTimeRemaining = dashDuration;
        lastDashTime = Time.time;

        // Reset vertical velocity for clean dash
        velocity.y = 0;
    }

    void HandleDash()
    {
        float dashSpeed = dashDistance / dashDuration;

        // Move in dash direction
        controller.Move(dashDirection * dashSpeed * Time.deltaTime);

        dashTimeRemaining -= Time.deltaTime;

        if (dashTimeRemaining <= 0)
        {
            // End dash, keep some momentum
            isDashing = false;
            velocity = dashDirection * dashSpeed * 0.5f;
            velocity.y = -2f;
        }
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        velocity = Vector3.zero;
        controller.enabled = true;
    }

    public void AddVelocity(Vector3 force)
    {
        velocity += force;
        jumpCount = 1; // Allow one more jump after rail jump
    }

    public bool IsGrounded => isGrounded;
    public float JumpForce => jumpForce;
    public Vector3 Velocity => velocity;
    public bool HasUsedAllJumps => jumpCount >= maxJumps;

    public float GetDistanceToGround()
    {
        if (isGrounded) return 0f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
        {
            return hit.distance - controller.height * 0.5f;
        }
        return 100f; // Very high - no ground detected
    }

    public bool WasJumpPressedRecently()
    {
        return Time.time - lastJumpPressTime <= railJumpWindow;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        velocity = Vector3.zero;
        GameManager.Instance?.OnPlayerDeath();
    }

    public void Respawn(Vector3 position)
    {
        isDead = false;
        Teleport(position);
    }
}
