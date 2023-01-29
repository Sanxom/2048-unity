using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOver;
    public TextMeshProUGUI scoreText;

    [SerializeField] private TextMeshProUGUI _hiscoreText;

    private int _score;

    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        SetScore(0);
        _hiscoreText.text = LoadHiscore().ToString();

        gameOver.alpha = 0;
        gameOver.interactable = false;
        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;

        StartCoroutine(Fade(gameOver, 1, 1));
    }

    public void IncreaseScore(int points)
    {
        SetScore(_score + points);
    }

    public void SaveHiscore()
    {
        int hiscore = LoadHiscore();

        if (_score > hiscore)
        {
            PlayerPrefs.SetInt("hiscore", _score);
        }
    }

    private void SetScore(int score)
    {
        _score = score;
        scoreText.text = score.ToString();

        SaveHiscore();
    }

    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        _hiscoreText.text = LoadHiscore().ToString();
        yield return new WaitForSeconds(delay);

        float elapsed = 0;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}