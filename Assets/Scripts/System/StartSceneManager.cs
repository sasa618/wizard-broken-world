using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class StartSceneManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject volumePanel;
    [SerializeField] private GameObject quitButton;
    [Header("Scene")]
    [SerializeField] private string StageSceneName = "Demo_Stage01";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (quitButton != null)
        {
            quitButton.SetActive(false);
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            MoveStage01();
        }
    }

    public void ShowHowToPlay()
    {
        SoundManager.Instance?.PlayUIButtonSE();

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }

        if (volumePanel != null)
        {
            volumePanel.SetActive(false);
        }
    }

    public void ShowVolumePanel()
    {
        SoundManager.Instance?.PlayUIButtonSE();

        if (startPanel != null)
        {
            startPanel.SetActive(false);
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

    public void ShowStartPanel()
    {
        SoundManager.Instance?.PlayUIButtonSE();

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (volumePanel != null)
        {
            volumePanel.SetActive(false);
        }
    }

    private void MoveStage01()
    {
        // 前回プレイの累計スコア・累計時間をリセット
        SoundManager.Instance?.PlayUIButtonSE();
        TotalResultData.Reset();

        Time.timeScale = 1f;
        SceneManager.LoadScene(StageSceneName);
    }

    //ゲームを辞める
    public void QuitGame()
    {
        SoundManager.Instance?.PlayUIButtonSE();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
