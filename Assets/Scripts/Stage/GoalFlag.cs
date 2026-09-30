using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalFlag : MonoBehaviour
{
    //[SerializeField]
    //private string nextSceneName;

    private bool isLoading;
    [Header("タイマー")]
    [SerializeField] private GameTimer gameTimer;

    [Header("リザルト")]
    [SerializeField] private ResultManager resultManager;

    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        //if (string.IsNullOrWhiteSpace(nextSceneName))
        //{
        //    Debug.LogWarning("次のシーン名が設定されていません。", this);
        //    return;
        //}

        //isLoading = true;
        //SceneManager.LoadScene(nextSceneName);

        if (gameTimer == null)
        {
            Debug.LogError(
                "GameTimerが設定されていません。",
                this
            );

            return;
        }

        if (resultManager == null)
        {
            Debug.LogError(
                "ResultManagerが設定されていません。",
                this
            );

            return;
        }

        isLoading = true;

        // タイマー停止
        gameTimer.StopTimer();
        SoundManager.Instance?.PlaySE(SEType.Goal);

        // クリア時間を取得
        float clearTime = gameTimer.ElapsedTime;

        // 結果パネルを表示
        resultManager.ShowResult(clearTime);

    }
}
