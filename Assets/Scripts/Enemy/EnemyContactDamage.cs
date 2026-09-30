using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    private EnemyBase enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DealDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        DealDamage(collision);
    }

    private void DealDamage(Collider2D collision)
    {
        if(enemy == null) return;

        enemy.TryDealContactDamage(collision);
    }
}
