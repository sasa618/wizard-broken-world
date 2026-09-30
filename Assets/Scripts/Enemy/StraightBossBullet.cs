using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StraightBossBullet : BossBullet
{
    public void Shoot(Vector2 direction, float speed, float newDamage)
    {
        SetDamage(newDamage);
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * speed;

        if (direction.sqrMagnitude > 0f)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
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
}
