using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private bool isCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        isCollected = true;

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoin(value);
        }

        PlayCoinSound();

        Destroy(gameObject);
    }

    private void PlayCoinSound()
    {
        SoundManager.Instance?.PlaySE(SEType.Coin);
    }
}
