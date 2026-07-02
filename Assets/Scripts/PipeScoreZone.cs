using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class PipeScoreZone : MonoBehaviour
{
    public event Action OnBirdPassed;

    private bool hasScored;
    private BoxCollider2D scoreCollider;

    private void Awake()
    {
        scoreCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasScored ||
            !other.TryGetComponent<BirdController>(out _) ||
            !scoreCollider.bounds.Contains(other.bounds.center))
        {
            return;
        }

        hasScored = true;
        OnBirdPassed?.Invoke();
    }
}
