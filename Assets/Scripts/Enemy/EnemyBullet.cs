using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 10f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Shoot(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;

        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RepairableObject repairableObject =
            collision.GetComponentInParent<RepairableObject>();

        if (repairableObject != null && repairableObject.ReduceRepairValue(damage))
        {
            Destroy(gameObject);
            return;
        }

        if(collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent(out PlayerHP playerHP))
            {
                playerHP.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else if(collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
