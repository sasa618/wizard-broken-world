using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class BossJump : MonoBehaviour
{
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpInterval = 3f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float jumpTimer;
    private bool jumpStarted;
    private bool hasLeftGround;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }

        jumpTimer = jumpInterval;
    }

    public void Tick()
    {
        jumpTimer += Time.deltaTime;

        if (jumpTimer < jumpInterval || !IsGrounded())
        {
            return;
        }

        jumpTimer = 0f;
        BeginJump();
    }

    public void BeginJump()
    {
        if (!IsGrounded())
        {
            jumpStarted = false;
            hasLeftGround = false;
            return;
        }

        jumpStarted = true;
        hasLeftGround = false;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void TickJumpState()
    {
        if (!jumpStarted)
        {
            return;
        }

        if (!IsGrounded())
        {
            hasLeftGround = true;
        }
    }

    public bool IsJumpFinished()
    {
        return jumpStarted && hasLeftGround && IsGrounded() && rb.linearVelocity.y <= 0.01f;
    }

    public void ResetJumpState()
    {
        jumpTimer = jumpInterval;
        jumpStarted = false;
        hasLeftGround = false;
    }

    public bool IsGrounded()
    {
        Vector2 checkPosition = groundCheck != null
            ? groundCheck.position
            : transform.position;

        return Physics2D.OverlapCircle(
            checkPosition,
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 checkPosition = groundCheck != null
            ? groundCheck.position
            : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
    }
}
