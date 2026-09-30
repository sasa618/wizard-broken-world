using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("UI")]
    [SerializeField] private GameObject resultPanel;

    [SerializeField] private TMP_Text clearTimeText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text hpText;
    //[SerializeField] private TMP_Text repairObjectText;
    //[SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text scoreText;

    [Header("演出")]
    [SerializeField] private float displayInterval = 0.5f;

    [Header("次のステージ")]
    [SerializeField] private string nextSceneName;

    private bool resultAdded = false;




    public void ShowResult(float clearTime)
    {
        if (resultPanel == null||scoreManager == null)
        {
            Debug.LogError(
                "ResultPanelが設定されていません。",
                this
            );

            return;
        }

        if (clearTimeText == null || scoreText == null)
        {
            Debug.LogError("Result用Textが設定されていません。", this);
            return;
        }

        // このステージのスコアを確定
        int stageScore = scoreManager.CalculateScore();

        // 累計に追加
        if (!resultAdded)
        {
            TotalResultData.AddStageResult(stageScore, clearTime, scoreManager.CoinCount);
            resultAdded = true;
        }

        resultPanel.SetActive(true);
        // リザルト表示中はゲームを停止
        Time.timeScale = 0f;

        // 順番に表示開始
        StartCoroutine(ShowResultSequence(clearTime));

        
    }

    //表示演出
    private IEnumerator ShowResultSequence(float clearTime)
    {
        // 最初は全部非表示
        clearTimeText.gameObject.SetActive(false);
        coinText.gameObject.SetActive(false);
        hpText.gameObject.SetActive(false);
        //repairObjectText.gameObject.SetActive(false);
        //enemyText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);

        // TIME
        clearTimeText.text =
            $"TIME {FormatTime(clearTime)}     +{scoreManager.TimeScore}";

        clearTimeText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(displayInterval);

            // COIN
            coinText.text =
            $"COIN × {scoreManager.CoinCount}         +{scoreManager.CoinScore}";

            coinText.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(displayInterval);
        

        // HP
        hpText.text =
            $"HP {scoreManager.CurrentHp}             +{scoreManager.HpScore}";

        hpText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(displayInterval);

        // 修理したオブジェクト
        //repairObjectText.text =
        //    $"REPAIR × {scoreManager.RepairedObjectCount}       +{scoreManager.RepairObjectScore}";

        //repairObjectText.gameObject.SetActive(true);

        //yield return new WaitForSecondsRealtime(displayInterval);

        //// 浄化した敵
        //enemyText.text =
        //    $"ENEMY × {scoreManager.RepairedEnemyCount}      +{scoreManager.RepairEnemyScore}";

        //enemyText.gameObject.SetActive(true);

        //yield return new WaitForSecondsRealtime(displayInterval);

        // 最終スコア
        scoreText.text =
            $"SCORE   {scoreManager.CurrentScore}";

        scoreText.gameObject.SetActive(true);
    }

    public void LoadNextStage()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning(
                "次のシーン名が設定されていません。",
                this
            );

            return;
        }

        // 停止状態を解除してからScene移動
        Time.timeScale = 1f;

        SceneManager.LoadScene(nextSceneName);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int centiseconds =
            Mathf.FloorToInt((time * 100f) % 100f);

        return $"{minutes:00}:{seconds:00}.{centiseconds:00}";
    }
}