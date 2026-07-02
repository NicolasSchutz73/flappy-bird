using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip pointClip;
    [SerializeField] private AudioClip wingClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip dieClip;
    [SerializeField, Range(0f, 1f)] private float pointVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float wingVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float dieVolume = 1f;
    [SerializeField, Min(0f)] private float dieDelay = 0.15f;

    private AudioSource audioSource;
    private Coroutine deathSequence;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    public void PlayPoint() => PlayOneShot(pointClip, pointVolume);

    public void PlayWing() => PlayOneShot(wingClip, wingVolume);

    public void PlayHit() => PlayOneShot(hitClip, hitVolume);

    public void PlayDie() => PlayOneShot(dieClip, dieVolume);

    public void PlayDeathSequence()
    {
        if (deathSequence != null)
        {
            StopCoroutine(deathSequence);
        }

        deathSequence = StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        PlayHit();

        if (dieDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(dieDelay);
        }

        PlayDie();
        deathSequence = null;
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
