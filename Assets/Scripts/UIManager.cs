using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScoreManager))]
public sealed class UIManager : MonoBehaviour
{
    [SerializeField] private Sprite[] digitSprites;
    [SerializeField] private Sprite messageSprite;
    [SerializeField] private Sprite gameOverSprite;
    [SerializeField] private Sprite scorePanelSprite;
    [SerializeField] private Sprite startButtonSprite;
    [SerializeField] private Vector2 referenceResolution = new(288f, 512f);
    [SerializeField] private float topOffset = 48f;
    [SerializeField] private float digitSpacing = 2f;
    [SerializeField] private float gameOverTopOffset = 120f;
    [SerializeField] private float scorePanelTopOffset = 168f;
    [SerializeField] private float startButtonTopOffset = 280f;
    [SerializeField, Range(0.25f, 1f)] private float panelDigitScale = 0.5f;

    private readonly List<Image> digitImages = new();
    private readonly List<Image> finalScoreImages = new();
    private readonly List<Image> bestScoreImages = new();
    private ScoreManager scoreManager;
    private GameManager gameManager;
    private RectTransform digitsRoot;
    private RectTransform finalScoreRoot;
    private RectTransform bestScoreRoot;
    private Image messageImage;
    private Image gameOverImage;
    private Image scorePanelImage;
    private Image startButtonImage;
    private int displayedScore = -1;
    private int displayedBestScore = -1;

    private void Awake()
    {
        scoreManager = GetComponent<ScoreManager>();
        gameManager = GetComponent<GameManager>();
        CreateGameCanvas();
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

    private void CreateGameCanvas()
    {
        GameObject canvasObject = new("GameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 16f;

        messageImage = CreateOverlayImage("Message", canvasObject.transform, messageSprite);
        AnchorImage(messageImage, new Vector2(0.5f, 0.5f), Vector2.zero);

        startButtonImage = CreateOverlayImage("StartButton", canvasObject.transform, startButtonSprite);
        AnchorImage(startButtonImage, new Vector2(0.5f, 1f), new Vector2(0f, -startButtonTopOffset), new Vector2(0.5f, 1f));

        gameOverImage = CreateOverlayImage("GameOver", canvasObject.transform, gameOverSprite);
        AnchorImage(gameOverImage, new Vector2(0.5f, 1f), new Vector2(0f, -gameOverTopOffset), new Vector2(0.5f, 1f));

        scorePanelImage = CreateOverlayImage("ScorePanel", canvasObject.transform, scorePanelSprite);
        AnchorImage(scorePanelImage, new Vector2(0.5f, 1f), new Vector2(0f, -scorePanelTopOffset), new Vector2(0.5f, 1f));

        finalScoreRoot = CreateDigitsRoot("FinalScoreDigits", scorePanelImage.transform, TextAnchor.UpperRight);
        AnchorRoot(finalScoreRoot, new Vector2(1f, 1f), new Vector2(-16f, -38f), new Vector2(1f, 1f));

        bestScoreRoot = CreateDigitsRoot("BestScoreDigits", scorePanelImage.transform, TextAnchor.UpperRight);
        AnchorRoot(bestScoreRoot, new Vector2(1f, 1f), new Vector2(-16f, -80f), new Vector2(1f, 1f));

        digitsRoot = CreateDigitsRoot("ScoreDigits", canvasObject.transform, TextAnchor.MiddleCenter);
        AnchorRoot(digitsRoot, new Vector2(0.5f, 1f), new Vector2(0f, -topOffset), new Vector2(0.5f, 1f));
    }

    private Image CreateOverlayImage(string objectName, Transform parent, Sprite sprite)
    {
        GameObject imageObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        if (sprite != null)
        {
            image.rectTransform.sizeDelta = sprite.rect.size;
        }

        return image;
    }

    private RectTransform CreateDigitsRoot(string objectName, Transform parent, TextAnchor alignment)
    {
        GameObject digitsObject = new(objectName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        digitsObject.transform.SetParent(parent, false);

        HorizontalLayoutGroup layout = digitsObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = digitSpacing;
        layout.childAlignment = alignment;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = digitsObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return digitsObject.GetComponent<RectTransform>();
    }

    private static void AnchorImage(Image image, Vector2 anchor, Vector2 position, Vector2? pivot = null)
    {
        RectTransform rect = image.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
    }

    private static void AnchorRoot(RectTransform root, Vector2 anchor, Vector2 position, Vector2 pivot)
    {
        root.anchorMin = anchor;
        root.anchorMax = anchor;
        root.pivot = pivot;
        root.anchoredPosition = position;
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
