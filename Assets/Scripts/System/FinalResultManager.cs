using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalResultManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject finalResultPanel;
    [SerializeField] private TMP_Text totalScoreText;
    [SerializeField] private TMP_Text totalTimeText;
    [SerializeField] private TMP_Text totalCoinText;
    
    [Header("コイン")]
    [SerializeField] private int maxTotalCoin = 2;

    private bool hasShownFinalResult;




    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void ShowFinalResult()
    {
        if (finalResultPanel == null)
        {
            Debug.LogError("FinalResultPanelが設定されていません。", this);
            return;
        }

        if (totalScoreText == null || totalTimeText == null|| totalCoinText == null)
        {
            Debug.LogError("最終Result用のTextが設定されていません。", this);
            return;
        }

        totalScoreText.text =
            $"TOTAL SCORE　　{TotalResultData.TotalScore}";

        totalTimeText.text =
            $"TOTAL TIME　　{FormatTime(TotalResultData.TotalTime)}";

        totalCoinText.text =
            $"TOTAL GetCoin　{TotalResultData.TotalCoin} / {maxTotalCoin}";

        finalResultPanel.SetActive(true);

        if (!hasShownFinalResult)
        {
            SoundManager.Instance?.PlaySE(SEType.FinalResult);
            hasShownFinalResult = true;
        }

        // スコアをセーブ
        if (DataManager.Instance != null)
        {
            DataManager.Instance.AddScoreAndSort(TotalResultData.TotalScore);
            DataManager.Instance.SaveScores();
            DataManager.Instance.UpdateScore = true;
        }

        // TODO: 「スコアをセーブしました」みたいな表示が欲しいね
    }


    public void BackToTitle()
    {
        // 次のプレイに備えてリセット
        TotalResultData.Reset();

        Time.timeScale = 1f;

        SceneManager.LoadScene("StartScene");
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
