using UnityEngine;

public sealed class PipeMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [SerializeField, Min(0f)] private float destroyPadding = 0.5f;

    private Camera mainCamera;
    private float halfWidth;

    private void Awake()
    {
        mainCamera = Camera.main;
        halfWidth = CalculateHalfWidth();
    }

    private void Update()
    {
        transform.Translate(Vector3.left * (moveSpeed * Time.deltaTime), Space.World);

        if (mainCamera == null)
        {
            return;
        }

        float leftScreenEdge = mainCamera.ViewportToWorldPoint(Vector3.zero).x;
        float rightPipeEdge = transform.position.x + halfWidth;

        if (rightPipeEdge < leftScreenEdge - destroyPadding)
        {
            Destroy(gameObject);
        }
    }

    private float CalculateHalfWidth()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        if (renderers.Length == 0)
        {
            return 0f;
        }

        Bounds combinedBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        return combinedBounds.extents.x;
    }
}
