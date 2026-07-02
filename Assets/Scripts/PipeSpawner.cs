using UnityEngine;

public sealed class PipeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject pipePairPrefab;
    [SerializeField, Min(0.1f)] private float spawnInterval = 3f;
    [SerializeField] private float minimumY = -2.5f;
    [SerializeField] private float maximumY = 5f;
    [SerializeField, Min(0f)] private float horizontalOffset = 2f;

    private Camera mainCamera;
    private float spawnTimer;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        SpawnPipePair();
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
        {
            return;
        }

        SpawnPipePair();
        spawnTimer += spawnInterval;
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

        Instantiate(pipePairPrefab, spawnPosition, Quaternion.identity);
    }

    private void OnValidate()
    {
        if (maximumY < minimumY)
        {
            (minimumY, maximumY) = (maximumY, minimumY);
        }
    }
}
