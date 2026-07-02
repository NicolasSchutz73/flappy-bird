using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScoreManager))]
[RequireComponent(typeof(GameManager))]
public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private Sprite[] digitSprites;
    [SerializeField, Range(0.25f, 1f)] private float panelDigitScale = 0.5f;
    [SerializeField] private RectTransform digitsRoot;
    [SerializeField] private RectTransform finalScoreRoot;
    [SerializeField] private RectTransform bestScoreRoot;
    [SerializeField] private Image messageImage;
    [SerializeField] private Image gameOverImage;
    [SerializeField] private Image scorePanelImage;
    [SerializeField] private Image startButtonImage;

    private readonly List<Image> digitImages = new();
    private readonly List<Image> finalScoreImages = new();
    private readonly List<Image> bestScoreImages = new();
    private ScoreManager scoreManager;
    private GameManager gameManager;
    private int displayedScore = -1;
    private int displayedBestScore = -1;

    private void Awake()
    {
        scoreManager = GetComponent<ScoreManager>();
        gameManager = GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        if (gameManager == null)
        {
            return;
        }

        gameManager.OnWaiting += HandleWaiting;
        gameManager.OnGameStarted += HandleGameStarted;
        gameManager.OnGameOver += HandleGameOver;
        gameManager.OnWorldTransitionStarted += HandleWorldTransitionStarted;
        gameManager.OnWorldTransitionCompleted += HandleWorldTransitionCompleted;
    }

    private void Start()
    {
        RefreshScores();
        ApplyState(gameManager != null ? gameManager.State : GameState.WaitingToStart);
    }

    private void OnDisable()
    {
        if (gameManager == null)
        {
            return;
        }

        gameManager.OnWaiting -= HandleWaiting;
        gameManager.OnGameStarted -= HandleGameStarted;
        gameManager.OnGameOver -= HandleGameOver;
        gameManager.OnWorldTransitionStarted -= HandleWorldTransitionStarted;
        gameManager.OnWorldTransitionCompleted -= HandleWorldTransitionCompleted;
    }

    private void Update()
    {
        if (displayedScore != scoreManager.Score || displayedBestScore != scoreManager.BestScore)
        {
            RefreshScores();
        }
    }

    private void RefreshScores()
    {
        displayedScore = Mathf.Max(0, scoreManager.Score);
        displayedBestScore = Mathf.Max(0, scoreManager.BestScore);
        RefreshDigits(digitImages, digitsRoot, displayedScore, 1f);
        RefreshDigits(finalScoreImages, finalScoreRoot, displayedScore, panelDigitScale);
        RefreshDigits(bestScoreImages, bestScoreRoot, displayedBestScore, panelDigitScale);
    }

    private void RefreshDigits(List<Image> images, RectTransform root, int score, float scale)
    {
        string value = score.ToString();

        while (images.Count < value.Length)
        {
            images.Add(CreateDigitImage(root, images.Count));
        }

        while (images.Count > value.Length)
        {
            int lastIndex = images.Count - 1;
            Destroy(images[lastIndex].gameObject);
            images.RemoveAt(lastIndex);
        }

        for (int i = 0; i < value.Length; i++)
        {
            int digit = value[i] - '0';
            Image image = images[i];
            image.name = $"Digit_{digit}";
            image.sprite = GetDigitSprite(digit);

            if (image.sprite != null)
            {
                image.rectTransform.sizeDelta = image.sprite.rect.size * scale;
            }
        }
    }

    private static Image CreateDigitImage(RectTransform parent, int index)
    {
        GameObject digitObject = new($"Digit_{index}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        digitObject.transform.SetParent(parent, false);

        Image image = digitObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private Sprite GetDigitSprite(int digit)
    {
        if (digitSprites == null || digitSprites.Length != 10)
        {
            Debug.LogError("UIManager requires the ten digit sprites in order from 0 to 9.", this);
            return null;
        }

        return digitSprites[digit];
    }

    private void HandleWaiting() => ApplyState(GameState.WaitingToStart);

    private void HandleGameStarted() => ApplyState(GameState.Playing);

    private void HandleGameOver() => ApplyState(GameState.GameOver);

    private void HandleWorldTransitionStarted() => messageImage.gameObject.SetActive(true);

    private void HandleWorldTransitionCompleted()
    {
        if (gameManager.State == GameState.Playing)
        {
            messageImage.gameObject.SetActive(false);
        }
    }

    private void ApplyState(GameState state)
    {
        bool isWaiting = state == GameState.WaitingToStart;
        bool isPlaying = state == GameState.Playing;
        bool isGameOver = state == GameState.GameOver;

        messageImage.gameObject.SetActive(isWaiting);
        startButtonImage.gameObject.SetActive(isGameOver);
        digitsRoot.gameObject.SetActive(isPlaying);
        gameOverImage.gameObject.SetActive(isGameOver);
        scorePanelImage.gameObject.SetActive(isGameOver);
    }
}
