using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class BossBullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 4f;

    protected Rigidbody2D rb;
    protected float damage = 10f;
    protected bool destroyOnGround = true;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public virtual void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    protected void SetDestroyOnGround(bool shouldDestroy)
    {
        destroyOnGround = shouldDestroy;
    }

    protected bool TryDamageRepairedRepairable(Collider2D collision)
    {
        RepairableObject repairableObject =
            collision.GetComponentInParent<RepairableObject>();

        return repairableObject != null
            && repairableObject.ReduceRepairValue(damage);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent(out PlayerHP playerHP))
            {
                playerHP.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        if (destroyOnGround && collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
