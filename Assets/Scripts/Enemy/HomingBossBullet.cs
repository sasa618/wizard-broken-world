using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingBossBullet : BossBullet
{
    private Transform target;
    private float speed;
    private float homingDuration;
    private float turnSpeed;
    private float homingTimer;
    private Vector2 currentDirection;

    public void Shoot(
        Vector2 direction,
        float newSpeed,
        float newDamage,
        Transform newTarget,
        float newHomingDuration,
        float newTurnSpeed
    )
    {
        SetDamage(newDamage);

        target = newTarget;
        speed = newSpeed;
        homingDuration = newHomingDuration;
        turnSpeed = newTurnSpeed;
        currentDirection = direction.normalized;

        rb.gravityScale = 0f;
        rb.linearVelocity = currentDirection * speed;
    }

    private void FixedUpdate()
    {
        if (target != null && homingTimer < homingDuration)
        {
            Vector2 desiredDirection = ((Vector2)target.position - rb.position).normalized;
            currentDirection = RotateTowards(
                currentDirection,
                desiredDirection,
                turnSpeed * Time.fixedDeltaTime
            );

            homingTimer += Time.fixedDeltaTime;
        }

        rb.linearVelocity = currentDirection * speed;

        if (currentDirection.sqrMagnitude > 0f)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, currentDirection);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (TryDamageRepairedRepairable(collision))
        {
            Destroy(gameObject);
            return;
        }

        base.OnTriggerEnter2D(collision);
    }

    private static Vector2 RotateTowards(Vector2 current, Vector2 targetDirection, float maxDegreesDelta)
    {
        if (current.sqrMagnitude <= 0f)
        {
            return targetDirection.normalized;
        }

        if (targetDirection.sqrMagnitude <= 0f)
        {
            return current.normalized;
        }

        float currentAngle = Mathf.Atan2(current.y, current.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxDegreesDelta);
        float radians = nextAngle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }
}
