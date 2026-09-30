using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int coinCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddCoin(int amount = 1)
    {
        coinCount += amount;

        Debug.Log("Coin : " + coinCount);
    }

    public int GetCoinCount()
    {
        return coinCount;
    }
}