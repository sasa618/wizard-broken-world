using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GroundWaveBullet : BossBullet
{
    public void Shoot(Vector2 direction, float speed, float newDamage)
    {
        SetDamage(newDamage);
        SetDestroyOnGround(false);
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction.normalized.x * speed, 0f);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<RepairableObject>() != null)
        {
            return;
        }

        base.OnTriggerEnter2D(collision);
    }
}
