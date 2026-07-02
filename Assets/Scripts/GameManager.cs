using System;
using System.Collections;
using UnityEngine;

public enum GameState
{
    WaitingToStart,
    Playing,
    GameOver
}

[RequireComponent(typeof(PipeSpawner))]
[RequireComponent(typeof(ScoreManager))]
[RequireComponent(typeof(AudioManager))]
[RequireComponent(typeof(UIManager))]
public sealed class GameManager : MonoBehaviour
{
    public event Action OnWaiting;
    public event Action OnGameStarted;
    public event Action OnGameOver;
    public event Action OnWorldTransitionStarted;
    public event Action OnWorldTransitionCompleted;

    [SerializeField] private BirdController bird;
    [SerializeField, Min(0f)] private float restartInputDelay = 0.75f;
    [SerializeField, Min(1)] private int modeSwitchInterval = 20;
    [SerializeField, Min(0f)] private float worldTransitionDuration = 1.25f;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Sprite dayBackground;
    [SerializeField] private Sprite nightBackground;

    private PipeSpawner pipeSpawner;
    private ScoreManager scoreManager;
    private AudioManager audioManager;
    private float restartAvailableAt;
    private int currentModeTier;
    private Coroutine worldTransitionRoutine;
    private bool pendingWorldInverted;
    private bool transitionReadyForInput;

    public GameState State { get; private set; } = GameState.WaitingToStart;

    private void Awake()
    {
        pipeSpawner = GetComponent<PipeSpawner>();
        scoreManager = GetComponent<ScoreManager>();
        audioManager = GetComponent<AudioManager>();
    }

    private void OnEnable()
    {
        PipeScoreZone.OnBirdPassed += HandleBirdPassed;

        if (bird != null)
        {
            bird.OnPrimaryAction += HandlePrimaryAction;
            bird.OnFlapped += HandleBirdFlapped;
            bird.OnDied += HandleBirdDied;
        }
    }

    private void Start()
    {
        EnterWaitingState();
    }

    private void OnDisable()
    {
        PipeScoreZone.OnBirdPassed -= HandleBirdPassed;

        if (bird != null)
        {
            bird.OnPrimaryAction -= HandlePrimaryAction;
            bird.OnFlapped -= HandleBirdFlapped;
            bird.OnDied -= HandleBirdDied;
        }
    }

    private void HandlePrimaryAction()
    {
        switch (State)
        {
            case GameState.WaitingToStart:
                StartGame();
                break;
            case GameState.Playing when transitionReadyForInput:
                CompleteWorldTransition();
                break;
            case GameState.GameOver when Time.unscaledTime >= restartAvailableAt:
                EnterWaitingState();
                break;
        }
    }

    private void StartGame()
    {
        CancelWorldTransition();
        scoreManager.Reset();
        pipeSpawner.ResetPipes();
        ResetWorldMode();
        bird.StartPlaying();
        pipeSpawner.StartSpawning();
        State = GameState.Playing;
        OnGameStarted?.Invoke();
    }

    private void EnterWaitingState()
    {
        CancelWorldTransition();
        State = GameState.WaitingToStart;
        scoreManager.Reset();
        pipeSpawner.ResetPipes();

        if (bird != null)
        {
            bird.PrepareForWaiting();
        }

        ResetWorldMode();

        OnWaiting?.Invoke();
    }

    private void HandleBirdDied()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        CancelWorldTransition();
        State = GameState.GameOver;
        pipeSpawner.StopSpawning();
        audioManager.PlayDeathSequence();
        restartAvailableAt = Time.unscaledTime + restartInputDelay;
        OnGameOver?.Invoke();
    }

    private void HandleBirdPassed()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        scoreManager.IncrementScore();
        scoreManager.TrySaveBestScore();
        audioManager.PlayPoint();
        UpdateWorldMode();
    }

    private void HandleBirdFlapped()
    {
        if (State == GameState.Playing)
        {
            audioManager.PlayWing();
        }
    }

    private void UpdateWorldMode()
    {
        int nextModeTier = scoreManager.Score / Mathf.Max(1, modeSwitchInterval);
        if (nextModeTier == currentModeTier)
        {
            return;
        }

        currentModeTier = nextModeTier;
        BeginWorldTransition(currentModeTier % 2 == 1);
    }

    private void ResetWorldMode()
    {
        currentModeTier = 0;
        ApplyWorldMode(false);
    }

    private void ApplyWorldMode(bool inverted)
    {
        if (bird != null)
        {
            bird.SetGravityInverted(inverted);
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = inverted ? nightBackground : dayBackground;
        }
    }

    private void BeginWorldTransition(bool inverted)
    {
        CancelWorldTransition();
        pendingWorldInverted = inverted;
        worldTransitionRoutine = StartCoroutine(WorldTransitionRoutine(inverted));
    }

    private IEnumerator WorldTransitionRoutine(bool inverted)
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = inverted ? nightBackground : dayBackground;
        }

        pipeSpawner.ResetPipes();
        bird.SetTransitionPaused(true);
        OnWorldTransitionStarted?.Invoke();

        yield return new WaitForSeconds(worldTransitionDuration);

        if (State == GameState.Playing)
        {
            transitionReadyForInput = true;
        }

        worldTransitionRoutine = null;
    }

    private void CompleteWorldTransition()
    {
        transitionReadyForInput = false;
        bird.SetGravityInverted(pendingWorldInverted);
        bird.SetTransitionPaused(false);
        pipeSpawner.StartSpawning();
        OnWorldTransitionCompleted?.Invoke();
    }

    private void CancelWorldTransition()
    {
        bool wasTransitioning = worldTransitionRoutine != null || transitionReadyForInput;

        if (worldTransitionRoutine != null)
        {
            StopCoroutine(worldTransitionRoutine);
            worldTransitionRoutine = null;
        }

        transitionReadyForInput = false;

        if (wasTransitioning)
        {
            OnWorldTransitionCompleted?.Invoke();
        }
    }
}
