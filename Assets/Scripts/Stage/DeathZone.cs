using UnityEngine;

//ステージ下に配置し、落下したプレイヤーを検知する
//ダメージを与えてリスポーンさせる。HPが尽きた場合はPlayerHP側がゲームオーバーにする
public class DeathZone : MonoBehaviour
{
    [Header("落下ペナルティ")]
    [SerializeField, Tooltip("落下時に受けるダメージ")]
    private float fallDamage = 10f;

    [SerializeField, Tooltip("リスポーン先。未設定ならPlayerSpawnerのスポーン地点に戻す")]
    private Transform respawnPoint;

    //リスポーン先が見つからずゲームオーバーにする場合の多重呼び出し防止
    private bool isGameOver;

    private PlayerSpawner playerSpawner;

    private void Start()
    {
        playerSpawner = FindFirstObjectByType<PlayerSpawner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        //リスポーン先がどこにもない場合は従来通りゲームオーバー
        if (!TryGetRespawnPosition(out Vector3 respawnPosition))
        {
            TriggerGameOver();
            return;
        }

        Respawn(other, respawnPosition);
    }

    private bool TryGetRespawnPosition(out Vector3 position)
    {
        if (respawnPoint != null)
        {
            position = respawnPoint.position;
            return true;
        }

        if (playerSpawner != null && playerSpawner.SpawnPoint != null)
        {
            position = playerSpawner.SpawnPoint.position;
            return true;
        }

        position = default;
        return false;
    }

    private void Respawn(Collider2D player, Vector3 respawnPosition)
    {
        //通常の被弾と同じ扱い（無敵中は0ダメージ、被弾後は点滅付き無敵になる）
        if (player.TryGetComponent(out PlayerHP playerHP))
        {
            playerHP.TakeDamage(fallDamage);

            //HPが尽きたらPlayerHP側でゲームオーバーになるのでリスポーンしない
            if (playerHP.CurrentHP <= 0f)
            {
                return;
            }
        }

        player.transform.position = respawnPosition;

        //落下速度を引き継がないようにリセット
        if (player.attachedRigidbody != null)
        {
            player.attachedRigidbody.linearVelocity = Vector2.zero;
        }
    }

    private void TriggerGameOver()
    {
        if (GameOverPanelManager.Instance == null)
        {
            Debug.LogError(
                "シーン内にGameOverPanelManagerが見つかりません。",
                this
            );

            return;
        }

        isGameOver = true;
        GameOverPanelManager.Instance.GameOver();
    }
}
