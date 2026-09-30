using UnityEngine;

public class EnemyWallChecker : MonoBehaviour
{
    [SerializeField] private Transform enemy;
    [SerializeField] private float flipCooldown = 1.0f;

    private float lastFlipTime = -999f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(enemy == null) return;

        if (Time.time - lastFlipTime < flipCooldown) return;

            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            enemy.localScale = new Vector2(-enemy.localScale.x, enemy.localScale.y);
            lastFlipTime = Time.time;
            Debug.Log("壁チェッカーで反転");
        }
    }
}
