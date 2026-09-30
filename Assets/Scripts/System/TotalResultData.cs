using UnityEngine;

public class TotalResultData
{
    public static int TotalScore { get; private set; }
    public static float TotalTime { get; private set; }
    public static int TotalCoin { get; private set; }

    public static void AddStageResult(int score, float time, int coinCount)
    {
        TotalScore += score;
        TotalTime += time;
        TotalCoin += coinCount;
    }

    public static void Reset()
    {
        TotalScore = 0;
        TotalTime = 0f;
        TotalCoin = 0;
    }
}
