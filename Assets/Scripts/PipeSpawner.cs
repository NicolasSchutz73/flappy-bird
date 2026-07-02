using System.Collections.Generic;
using UnityEngine;

public sealed class PipeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject pipePairPrefab;
    [SerializeField, Min(0.1f)] private float spawnInterval = 1.8f;
    [SerializeField] private float minimumY = -2.5f;
    [SerializeField] private float maximumY = 5f;
    [SerializeField, Min(0f)] private float horizontalOffset = 2f;

    private readonly List<PipeMovement> spawnedPipes = new();
    private Camera mainCamera;
    private float spawnTimer;
    private bool isSpawning;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!isSpawning)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
        {
            return;
        }

        SpawnPipePair();
        spawnTimer += spawnInterval;
    }

    public void StartSpawning()
    {
        if (isSpawning)
        {
            return;
        }

        isSpawning = true;
        SpawnPipePair();
        spawnTimer = spawnInterval;
    }

    public void StopSpawning()
    {
        isSpawning = false;

        SetSpawnedPipesMoving(false);
    }

    private void SetSpawnedPipesMoving(bool shouldMove)
    {
        foreach (PipeMovement pipe in spawnedPipes)
        {
            if (pipe != null)
            {
                pipe.SetMoving(shouldMove);
            }
        }
    }

    public void ResetPipes()
    {
        StopSpawning();

        foreach (PipeMovement pipe in spawnedPipes)
        {
            if (pipe != null)
            {
                Destroy(pipe.gameObject);
            }
        }

        spawnedPipes.Clear();
        spawnTimer = 0f;
    }

    private void SpawnPipePair()
    {
        if (pipePairPrefab == null || mainCamera == null)
        {
            return;
        }

        float spawnX = mainCamera.ViewportToWorldPoint(Vector3.right).x + horizontalOffset;
        float spawnY = Random.Range(minimumY, maximumY);
        Vector3 spawnPosition = new(spawnX, spawnY, 0f);
        GameObject pipeObject = Instantiate(pipePairPrefab, spawnPosition, Quaternion.identity);

        if (pipeObject.TryGetComponent(out PipeMovement movement))
        {
            movement.SetMoving(true);
            spawnedPipes.Add(movement);
        }
    }

    private void OnValidate()
    {
        if (maximumY < minimumY)
        {
            (minimumY, maximumY) = (maximumY, minimumY);
        }
    }
}
