using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GravityBossBullet : BossBullet
{
    public void Shoot(Vector2 velocity, float gravityScale, float newDamage)
    {
        SetDamage(newDamage);
        rb.gravityScale = gravityScale;
        rb.linearVelocity = velocity;

        if (velocity.sqrMagnitude > 0f)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, velocity.normalized);
        }
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
