using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private GameTimer gameTimer;
    //[SerializeField] private RepairableManager repairObjectManager;
    //[SerializeField] private EnemyRepairable repairEnemyManager;


    //[Header("UI")]
    //[SerializeField] private TMP_Text scoreText;

    [Header("スコア設定")]
    [SerializeField] private int maxTimeScore = 500;
    [SerializeField] private int timeScorePerSecond = 5;
    [SerializeField] private int scoreCoin = 200;
    [SerializeField] private int scoreHp = 2;
    //[SerializeField] private int scorePerRepairObject = 100;
    //[SerializeField] private int scorePerRepairEnemy = 150;
    // 計算に必要
    private int timeScore;
    private int coinCount;
    private int currentHp;
    //private int repairedObjectCount;
    //private int repairedEnemyCount;

    private int currentScore;

    public int CurrentScore => currentScore;

    public int TimeScore => timeScore;
    public int CoinCount => coinCount;
    public int CoinScore => coinCount * scoreCoin;
    public int CurrentHp => currentHp;
    public int HpScore => currentHp * scoreHp;
    //public int RepairedObjectCount => repairedObjectCount;
    //public int RepairObjectScore => repairedObjectCount * scorePerRepairObject;
    //public int RepairedEnemyCount => repairedEnemyCount;
    //public int RepairEnemyScore => repairedEnemyCount * scorePerRepairEnemy;

    //private void Update()
    //{
    //    CalculateScore();
    //    UpdateScoreText();
    //}

   

    public int CalculateScore()
    {
        if (gameTimer == null //|| repairObjectManager == null
           )
        {
            return 0;
        }

        // 時間
        timeScore = Mathf.Max(
            0,
            maxTimeScore - Mathf.FloorToInt(gameTimer.ElapsedTime) * timeScorePerSecond
        );

        // コイン
        coinCount = CoinManager.Instance != null
            ? CoinManager.Instance.GetCoinCount()
            : 0;

        // HP
        if (PlayerHP.Instance == null)
        {
            Debug.LogError("PlayerHPが見つかりません。");
            return 0;
        }
        currentHp = Mathf.FloorToInt(PlayerHP.Instance.CurrentHP);

        // 修理した物体
        //repairedObjectCount = repairObjectManager.RepairedCount;

        //// 浄化した敵
        //repairedEnemyCount = EnemyRepairable.GetDefeatedCount();


        // 合計スコア
        currentScore =
            timeScore +
            coinCount * scoreCoin +
            currentHp * scoreHp;
            //+ repairedObjectCount * scorePerRepairObject +
            //repairedEnemyCount * scorePerRepairEnemy;

        //UpdateScoreText();

        return currentScore;
    }

    //private void UpdateScoreText()
    //{
    //    if (scoreText != null)
    //    {
    //        scoreText.text = $"SCORE {currentScore}";
    //    }
    //}
    
}
