using UnityEngine;

public class EnemyCliffChecker : MonoBehaviour
{
    [SerializeField] private Transform enemy;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(enemy == null) return;

        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            enemy.localScale = new Vector2(-enemy.localScale.x, enemy.localScale.y);
            Debug.Log("崖チェッカーで反転");
        }
    }
}
