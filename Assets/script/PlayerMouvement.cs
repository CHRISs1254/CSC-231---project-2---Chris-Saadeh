using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.5f;
    public float fallMultiplier = 1.6f;

    // DASH settings
    public float dashSpeed = 20f;       // how fast the dash goes
    public float dashDuration = 0.15f;  // how long the dash lasts (seconds)
    public float dashCooldown = 0.5f;   // wait time before you can dash again

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float normalGravity;

    // DASH state
    private bool isDashing;
    private bool canDash = true;        // false after an air dash, until you land
    private float dashTimer;            // counts down while dashing
    private float dashCooldownTimer;    // counts down after a dash
    private float dashDirection;        // 1 = right, -1 = left

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        normalGravity = rb.gravityScale;
    }

    void Update()
    {
        // 1. Read left/right keys
        moveInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput = 1f;

        // 2. Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 3. Coyote timer
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // DASH A. Landing gives the dash back
        if (isGrounded)
            canDash = true;

        // DASH B. Cooldown counts down
        dashCooldownTimer -= Time.deltaTime;

        // DASH C. While dashing, count down and skip everything else
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                EndDash();
            return; // no jumping or turning around in the middle of a dash
        }

        // DASH D. Start a dash
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && canDash && dashCooldownTimer <= 0f)
        {
            StartDash();
            return;
        }

        // 4. Jump buffer
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // 5. Jump
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // 6. Variable jump height
        if (Keyboard.current.spaceKey.wasReleasedThisFrame && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // 7. Face the direction we're moving
        if (moveInput > 0 && !facingRight) Flip();
        if (moveInput < 0 && facingRight) Flip();
    }

    void FixedUpdate()
    {
        // DASH E. During a dash: straight line, no gravity, ignore normal movement
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = normalGravity * fallMultiplier;
        else
            rb.gravityScale = normalGravity;
    }

    // DASH F. Begin the dash
    void StartDash()
    {
        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        dashDirection = facingRight ? 1f : -1f;
        rb.gravityScale = 0f;
    }

    // DASH G. Finish the dash
    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = normalGravity;
        rb.linearVelocity = new Vector2(dashDirection * moveSpeed, 0f); // drop back to normal speed
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}