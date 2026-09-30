using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool IsAnyPaused { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainPausePanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject volumePanel;
    //[SerializeField] private GameObject titleConfirmPanel;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "StartScene";
    [SerializeField] private string nowSceneName = "Stage01";

    private bool isPaused;

    public bool IsPaused => isPaused;

    private void Start()
    {
        ResumeGame();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PlayUIButtonSE()
    {
        SoundManager.Instance?.PlayUIButtonSE();
    }

    public void PauseGame()
    {
        bool wasPaused = isPaused;

        isPaused = true;
        IsAnyPaused = true;

        if (!wasPaused)
        {
            SoundManager.Instance?.PlayPauseOpenSE();
        }

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        ShowMainPausePanel();
    }

    public void ResumeGame()
    {
        bool wasPaused = isPaused;

        isPaused = false;
        IsAnyPaused = false;
        Time.timeScale = 1f;

        if (wasPaused)
        {
            SoundManager.Instance?.PlayUIButtonSE();
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        HideSubPanels();
    }

    // 通常のポーズメニューを表示
    public void ShowMainPausePanel()
    {
        if (isPaused)
        {
            Time.timeScale = 0f;
            IsAnyPaused = true;

            if (volumePanel != null && volumePanel.activeSelf)
            {
                SoundManager.Instance?.PlayUIButtonSE();
            }
        }

        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(true);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (volumePanel != null)
        {
            volumePanel.SetActive(false);
        }

        //if (titleConfirmPanel != null)
        //{
        //    titleConfirmPanel.SetActive(false);
        //}
    }

    public void LoadNowStage()
    {
        SoundManager.Instance?.PlayUIButtonSE();

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

    // 操作方法を表示
    public void ShowHowToPlay()
    {
        SoundManager.Instance?.PlayUIButtonSE();

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
    }

    public void ShowVolumePanel()
    {
        isPaused = true;
        IsAnyPaused = true;
        Time.timeScale = 0f;
        SoundManager.Instance?.PlayUIButtonSE();

        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (volumePanel != null)
        {
            volumePanel.SetActive(true);
        }
    }

    // タイトルへ戻る確認画面を表示
    //public void ShowTitleConfirm()
    //{
    //    if (mainPausePanel != null)
    //    {
    //        mainPausePanel.SetActive(false);
    //    }

    //    if (titleConfirmPanel != null)
    //    {
    //        titleConfirmPanel.SetActive(true);
    //    }
    //}

    // タイトルSceneへ移動
    public void BackToTitle()
    {
        SoundManager.Instance?.PlayUIButtonSE();

        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    private void HideSubPanels()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (volumePanel != null)
        {
            volumePanel.SetActive(false);
        }

        //if (titleConfirmPanel != null)
        //{
        //    titleConfirmPanel.SetActive(false);
        //}
    }

    private void OnDestroy()
    {
        IsAnyPaused = false;
        Time.timeScale = 1f;
    }
}
