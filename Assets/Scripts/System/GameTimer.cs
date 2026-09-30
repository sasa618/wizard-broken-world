using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    private float elapsedTime;
    private bool isRunning;

    // 他のスクリプトから現在の時間を取得するため
    public float ElapsedTime => elapsedTime;

    // 現在タイマーが動いているか
    public bool IsRunning => isRunning;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        StartTimer();
    }

    // Update is called once per frame
    private void Update()
    {
        //テスト用
        if (Keyboard.current != null &&
        Keyboard.current.sKey.wasPressedThisFrame)
        {
            StopTimer();
        }

        if (!isRunning)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    
    /// タイマーを0秒から開始する
        public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
        UpdateTimerText();
    }

    /// 現在の時間でタイマーを停止する
    public void StopTimer()
    {
        isRunning = false;
        UpdateTimerText();
    }

    /// 停止しているタイマーを再開する
    public void ResumeTimer()
    {
        isRunning = true;
    }

    /// タイマーを0秒へ戻して停止する
    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = false;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int centiseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        timerText.text =
            $"TIME {minutes:00}:{seconds:00}.{centiseconds:00}";
    }

}
