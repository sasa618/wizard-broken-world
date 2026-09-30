using UnityEngine;

public abstract class SlimeEnemyBase : EnemyBase
{
    [SerializeField] private float cliffCheckOffset = 0.6f;
    [SerializeField] private float cliffCheckDistance = 1f;
    [SerializeField] private float wallCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundMask;

    private Collider2D bodyCollider;

    protected override bool CanTurnOnEnemyContact => true;

    protected override void Start()
    {
        base.Start();
        bodyCollider = GetComponent<Collider2D>();

        if(groundMask == 0)
        {
            groundMask = LayerMask.GetMask("Ground");
        }
    }

    protected void MoveWithTurn()
    {
        if(ShouldTurnAround())
        {
            TurnAroundForEnemyContact();
        }

        Move();
    }

    protected void TickFireTimer()
    {
        fireTimer += Time.deltaTime;
        if(fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Shoot();
        }
    }

    private bool ShouldTurnAround()
    {
        float directionX = FacingDirection.x;
        return !GroundAhead(directionX) || WallAhead(directionX);
    }

    private bool GroundAhead(float directionX)
    {
        Bounds bounds = bodyCollider != null
            ? bodyCollider.bounds
            : new Bounds(transform.position, Vector3.one);

        Vector2 origin = new Vector2(
            bounds.center.x + directionX * cliffCheckOffset,
            bounds.min.y + 0.02f
        );

        return Physics2D.Raycast(origin, Vector2.down, cliffCheckDistance, groundMask);
    }

    private bool WallAhead(float directionX)
    {
        Bounds bounds = bodyCollider != null
            ? bodyCollider.bounds
            : new Bounds(transform.position, Vector3.one);

        Vector2 origin = new Vector2(
            bounds.center.x + bounds.extents.x * directionX,
            bounds.center.y
        );

        return Physics2D.Raycast(origin, Vector2.right * directionX, wallCheckDistance, groundMask);
    }

    protected override void TurnAroundForEnemyContact()
    {
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }
}
