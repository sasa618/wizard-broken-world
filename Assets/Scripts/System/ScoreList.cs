using UnityEngine;

public class ScoreList : MonoBehaviour
{
    [SerializeField]
    private GameObject _scorePrefab;

    private GameObject[] _scoreRanks;

    private static int _displayListSize = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _scoreRanks = new GameObject[_displayListSize];

        UpdateScoreList();
    }

    private void Update()
    {
        if (DataManager.Instance.UpdateScore)
        {
            UpdateScoreList();
            DataManager.Instance.UpdateScore = false;
        }
    }

    public void UpdateScoreList()
    {
        for (int i = 0; i < _displayListSize; i++)
        {
            _scoreRanks[i] = Instantiate(_scorePrefab, this.transform);

            var scoreBoard = _scoreRanks[i].GetComponent<SetScoreBoard>();

            //Debug.Log("ListSize: " + DataManager.Instance.GetScoreListSize());
            if (i >= DataManager.Instance.GetScoreListSize())
            {
                scoreBoard.SetScore(i + 1, -1);
            }
            else
            {
                scoreBoard.SetScore(i + 1, DataManager.Instance.GetScore(i));
            }
        }
    }
}
