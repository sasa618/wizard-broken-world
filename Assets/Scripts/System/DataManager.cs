using System.Collections.Generic;
using UnityEngine;
using CI.QuickSave;

/// <summary>
/// データを扱うシングルトン
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    // セーブは1種類だけ
    private const string Root = "SaveRoot";
    private const string WebGLScorePrefsKey = "Scores";

    private QuickSaveSettings _saveSettings;

    // 過去スコアのリスト
    private List<int> _scores;

    // スコアをセーブするキー
    private const string ScoreSaveName = "Scores";

    // HPをシーンをまたいで保持できる
    private int _hp = 0;

    public bool UpdateScore { get; set; }

    public bool Initialized { get; private set; } = false;

    [System.Serializable]
    private class ScoreSaveData
    {
        public List<int> scores = new List<int>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 初期化
        // セーブの設定
        _saveSettings = new QuickSaveSettings
        {
            SecurityMode = SecurityMode.Aes, // 暗号化法
            Password = "<REDACTED>", // パスワード
            CompressionMode = CompressionMode.Gzip // セーブファイルの圧縮
        };
        // セーブファイルの位置
        QuickSaveGlobalSettings.StorageLocation = Application.persistentDataPath;

        if (!LoadScores())
        {
            _scores = new List<int>();
        }


        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialized = true;
    }

    /// <summary>
    /// スコアをリストに追加
    /// </summary>
    /// <param name="newScore"></param>
    private void AddScore(int newScore)
    {
        _scores.Add(newScore);
    }

    /// <summary>
    /// スコアをリストに追加し、降順にソート
    /// </summary>
    /// <param name="newScore"></param>
    public void AddScoreAndSort(int newScore)
    {
        AddScore(newScore);
        _scores.Sort((a, b) => b.CompareTo(a)); // 降順にソート

        //Debug.Log("Scores: " + _scores);
    }

    /// <summary>
    /// 現在のスコア履歴をセーブする
    /// </summary>
    public void SaveScores()
    {
        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ScoreSaveData saveData = new ScoreSaveData
            {
                scores = _scores ?? new List<int>()
            };

            PlayerPrefs.SetString(WebGLScorePrefsKey, JsonUtility.ToJson(saveData));
            PlayerPrefs.Save();
            Debug.Log("Scores Saved! (PlayerPrefs)");
#else
            bool b = QuickSaveWriter.Create(Root, _saveSettings)
                .Write(ScoreSaveName, _scores)
                .TryCommit();

            if (b)
            {
                Debug.Log($"Scores Saved! (FIle at {QuickSaveGlobalSettings.StorageLocation})");
            }
            else
            {
                Debug.LogWarning("Save Failed");
            }
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Save Failed: {e}");
        }
    }

    /// <summary>
    /// スコア履歴をロードする
    /// </summary>
    /// <returns>ロードに成功したか。trueで成功</returns>
    public bool LoadScores()
    {
        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!PlayerPrefs.HasKey(WebGLScorePrefsKey))
            {
                return false;
            }

            string json = PlayerPrefs.GetString(WebGLScorePrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            ScoreSaveData saveData = JsonUtility.FromJson<ScoreSaveData>(json);
            _scores = saveData?.scores ?? new List<int>();
            return true;
#else
            if (!QuickSaveReader.RootExists(Root))
            {
                return false;
            }

            var reader = QuickSaveReader.Create(Root, _saveSettings);

            return reader.TryRead(ScoreSaveName, out _scores);
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Load Failed: {e}");
            return false;
        }
    }

    /// <summary>
    /// スコア履歴をリセットする
    /// </summary>
    public void ResetScore()
    {
        _scores = new List<int>();
    }

    /// <summary>
    /// セーブデータを削除（リセットして上書き）
    /// </summary>
    public void ResetSave()
    {
        ResetScore();
        SaveScores();
    }

    /// <summary>
    /// データがあるか
    /// </summary>
    /// <returns></returns>
    public bool HasSaveData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return PlayerPrefs.HasKey(WebGLScorePrefsKey);
#else
        return QuickSaveReader.RootExists(Root);
#endif
    }
    public bool HasScores()
    {
        return _scores != null;
    }

    /// <summary>
    /// スコア履歴からスコアを取得
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public int GetScore(int index)
    {
        if (_scores == null || index < 0 || index >= _scores.Count)
        {
            return -1;
        }

        return _scores[index];
    }

    /// <summary>
    /// スコア履歴の数を取得
    /// </summary>
    /// <returns></returns>
    public int GetScoreListSize()
    {
        return _scores?.Count ?? 0;
    }

    public void SetHP(int hp)
    {
        _hp = hp;
    }
    public int GetHP()
    {
        return _hp;
    }
}
