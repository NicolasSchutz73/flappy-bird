using UnityEngine;

public sealed class ScoreManager : MonoBehaviour
{
    private const string BestScoreKey = "flappy-bird.best-score";

    private int score;
    private int bestScore;

    public int Score => score;
    public int BestScore => bestScore;

    private void Awake()
    {
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public void IncrementScore()
    {
        score++;
    }

    public void TrySaveBestScore()
    {
        if (score <= bestScore)
        {
            return;
        }

        bestScore = score;
        PlayerPrefs.SetInt(BestScoreKey, bestScore);
        PlayerPrefs.Save();
    }

    public void Reset()
    {
        score = 0;
    }
}
