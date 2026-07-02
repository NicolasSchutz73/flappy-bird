using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class BirdController : MonoBehaviour
{
    public event Action OnPrimaryAction;
    public event Action OnFlapped;
    public event Action OnDied;

    [SerializeField] private float flapVelocity = 15.5f;
    [SerializeField] private float riseAngle = 25f;
    [SerializeField] private float fallAngle = -70f;
    [SerializeField] private float riseRotationSpeed = 360f;
    [SerializeField] private float fallRotationSpeed = 60f;

    private Rigidbody2D body;
    private Animator animator;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float gameplayGravityScale;
    private bool canFlap;
    private bool isPlaying;
    private bool gravityInverted;
    private bool flapRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        gameplayGravityScale = Mathf.Abs(body.gravityScale);
    }

    private void Update()
    {
        if (Mouse.current?.leftButton.wasPressedThisFrame == true)
        {
            OnPrimaryAction?.Invoke();

            if (canFlap)
            {
                flapRequested = true;
            }
        }

        if (isPlaying)
        {
            UpdateRotation();
        }
    }

    private void FixedUpdate()
    {
        if (!canFlap || !flapRequested)
        {
            return;
        }

        body.linearVelocityY = gravityInverted ? -flapVelocity : flapVelocity;
        flapRequested = false;
        OnFlapped?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isPlaying)
        {
            return;
        }

        Die();
        OnDied?.Invoke();
    }

    public void PrepareForWaiting()
    {
        canFlap = false;
        isPlaying = false;
        flapRequested = false;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.position = initialPosition;
        body.rotation = initialRotation.eulerAngles.z;
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    public void StartPlaying()
    {
        body.gravityScale = gravityInverted ? -gameplayGravityScale : gameplayGravityScale;
        canFlap = true;
        isPlaying = true;

        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    public void Die()
    {
        canFlap = false;
        isPlaying = false;
        flapRequested = false;
        gravityInverted = false;
        body.gravityScale = gameplayGravityScale;

        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    public void SetGravityInverted(bool inverted)
    {
        if (gravityInverted == inverted)
        {
            return;
        }

        gravityInverted = inverted;
        flapRequested = false;
        body.linearVelocityY = 0f;

        if (isPlaying)
        {
            body.gravityScale = gravityInverted ? -gameplayGravityScale : gameplayGravityScale;
        }
    }

    public void SetTransitionPaused(bool paused)
    {
        if (!isPlaying)
        {
            return;
        }

        canFlap = !paused;
        flapRequested = false;
        body.linearVelocity = Vector2.zero;
        body.gravityScale = paused
            ? 0f
            : gravityInverted ? -gameplayGravityScale : gameplayGravityScale;
    }

    private void UpdateRotation()
    {
        bool isRising = flapRequested ? !gravityInverted : body.linearVelocityY > 0f;
        float targetAngle = isRising ? riseAngle : fallAngle;
        float rotationSpeed = isRising ? riseRotationSpeed : fallRotationSpeed;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }
}
