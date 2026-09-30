using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanelManager : MonoBehaviour
{
    //シーン内のGameOverPanelManagerを共有する参照（インスペクタでのアタッチ不要で呼び出せる）
    private static GameOverPanelManager instance;
    public static GameOverPanelManager Instance
    {
        get
        {
            //未取得、またはシーン遷移で破棄済みの場合は探し直す
            if (instance == null)
            {
                //パネルが非アクティブな状態でも見つけられるように検索する
                instance = FindFirstObjectByType<GameOverPanelManager>(
                    FindObjectsInactive.Include
                );
            }

            return instance;
        }
    }

    [SerializeField] private GameObject GameOverPanel;
    [Header("Scene")]
    [SerializeField] private string titleSceneName = "StartScene";
    [SerializeField] private string nowSceneName = "Stage01";

    //public void ShowGameOverPanel()
    //{
    //    if (GameOverPanel != null)
    //    {
    //        GameOverPanel.SetActive(true);
    //    }

    //}




    public void GameOver()
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;

    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }


    public void RetryStage()
    {
        if (string.IsNullOrWhiteSpace(nowSceneName))
        {
            Debug.LogWarning(
                "シーン名が設定されていません。",
                this
            );

            return;
        }

        // 停止状態を解除してからScene移動
        Time.timeScale = 1f;

        SceneManager.LoadScene(nowSceneName);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
