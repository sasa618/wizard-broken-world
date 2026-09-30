using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class BossMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0.45f, -0.55f);
    [SerializeField] private float groundCheckDistance = 0.25f;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.55f, 0f);
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private int startDirection = -1;
    [SerializeField] private bool spriteFacesRightByDefault;

    private Rigidbody2D rb;
    private Collider2D bossCollider;
    private BossJump bossJump;
    private int direction;

    public int Direction => direction;
    public Collider2D LastWallAheadCollider { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        bossJump = GetComponent<BossJump>();
        direction = startDirection >= 0 ? 1 : -1;
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }

        ApplyFacing();
    }

    public void Tick()
    {
        if (ShouldTurn())
        {
            Turn();
        }

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );
    }

    public void Stop()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void RestoreHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private bool ShouldTurn()
    {
        Bounds bounds = bossCollider != null
            ? bossCollider.bounds
            : new Bounds(transform.position, Vector3.one);

        Vector2 center = bounds.center;
        Vector2 forward = Vector2.right * direction;
        const float raySkin = 0.02f;

        RaycastHit2D wallHit = Physics2D.Raycast(
            center + new Vector2((bounds.extents.x + raySkin) * direction, wallCheckOffset.y),
            forward,
            wallCheckDistance,
            groundLayer
        );
        LastWallAheadCollider = wallHit.collider;

        bool isGrounded = bossJump == null || bossJump.IsGrounded();
        bool hasNoGroundAhead = false;
        if (isGrounded)
        {
            RaycastHit2D groundAheadHit = Physics2D.Raycast(
                new Vector2(
                    center.x + (bounds.extents.x + groundCheckOffset.x) * direction,
                    bounds.min.y + raySkin
                ),
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            hasNoGroundAhead = groundAheadHit.collider == null;
        }

        return wallHit.collider != null || hasNoGroundAhead;
    }

    private void Turn()
    {
        direction *= -1;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        Vector3 scale = transform.localScale;
        float facingSign = spriteFacesRightByDefault ? direction : -direction;
        scale.x = Mathf.Abs(scale.x) * facingSign;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        int gizmoDirection = Application.isPlaying
            ? direction
            : (startDirection >= 0 ? 1 : -1);

        Collider2D selectedCollider = GetComponent<Collider2D>();
        Bounds bounds = selectedCollider != null
            ? selectedCollider.bounds
            : new Bounds(transform.position, Vector3.one);

        Vector2 center = bounds.center;
        const float raySkin = 0.02f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            center + new Vector2((bounds.extents.x + raySkin) * gizmoDirection, wallCheckOffset.y),
            center + new Vector2((bounds.extents.x + raySkin) * gizmoDirection, wallCheckOffset.y)
                + Vector2.right * gizmoDirection * wallCheckDistance
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector2(
                center.x + (bounds.extents.x + groundCheckOffset.x) * gizmoDirection,
                bounds.min.y + raySkin
            ),
            new Vector2(
                center.x + (bounds.extents.x + groundCheckOffset.x) * gizmoDirection,
                bounds.min.y + raySkin
            )
                + Vector2.down * groundCheckDistance
        );
    }

    public bool IsMoving()
    {
        return rb.linearVelocityX != 0 || rb.linearVelocityY != 0;
    }
}
