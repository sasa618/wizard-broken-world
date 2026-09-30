using TMPro;
using UnityEngine;

public class SetScoreBoard : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _rank;
    [SerializeField]
    private TextMeshProUGUI _score;

    public void SetScore(int rank, int score)
    {
        _rank.text = rank.ToString() + ".";
        _score.text = (score >= 0) ? score.ToString() : "---";
    }
}
