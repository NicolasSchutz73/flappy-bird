using UnityEngine;

public sealed class PipeMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float destroyPadding = 0.5f;

    private Camera mainCamera;
    private float halfWidth;
    private bool isMoving = true;

    private void Awake()
    {
        mainCamera = Camera.main;
        halfWidth = CalculateHalfWidth();
    }

    private void Update()
    {
        if (!isMoving)
        {
            return;
        }

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

    public void SetMoving(bool shouldMove)
    {
        isMoving = shouldMove;
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
