using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private Transform spawnPoint;

    private GameObject spawnedPlayer;
    public Transform SpawnedPlayerTransform => spawnedPlayer != null
        ? spawnedPlayer.transform
        : null;

    //リスポーン先として使えるように公開（DeathZoneが参照する）
    public Transform SpawnPoint => spawnPoint;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefabが設定されていません。", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Pointが設定されていません。", this);
            return;
        }

        if (spawnedPlayer != null)
        {
            return;
        }

        spawnedPlayer = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}
