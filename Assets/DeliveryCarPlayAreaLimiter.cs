using UnityEngine;

/// <summary>
/// Giu xe delivery khong chay ra ngoai vung nen choi.
/// Keo BoxCollider2D cua DeliveryPlayableAreaBackground vao playArea neu can gan tay.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Delivery Car Play Area Limiter")]
public class DeliveryCarPlayAreaLimiter : MonoBehaviour
{
    public BoxCollider2D playArea;
    public float edgePadding = 0.05f;

    private Rigidbody2D cachedRigidbody;

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        ClampToPlayArea();
    }

    private void LateUpdate()
    {
        ClampToPlayArea();
    }

    private void ClampToPlayArea()
    {
        if (playArea == null)
        {
            return;
        }

        Bounds bounds = playArea.bounds;
        Vector2 halfSize = GetObjectHalfSize();
        Vector3 position = transform.position;
        Vector3 clampedPosition = position;
        clampedPosition.x = Mathf.Clamp(position.x, bounds.min.x + halfSize.x + edgePadding, bounds.max.x - halfSize.x - edgePadding);
        clampedPosition.y = Mathf.Clamp(position.y, bounds.min.y + halfSize.y + edgePadding, bounds.max.y - halfSize.y - edgePadding);

        if (cachedRigidbody != null)
        {
            cachedRigidbody.position = new Vector2(clampedPosition.x, clampedPosition.y);
            if (position != clampedPosition)
            {
                cachedRigidbody.linearVelocity = Vector2.zero;
                cachedRigidbody.angularVelocity = 0f;
            }
        }

        transform.position = clampedPosition;
    }

    private Vector2 GetObjectHalfSize()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Bounds spriteBounds = spriteRenderer.bounds;
            return new Vector2(spriteBounds.extents.x, spriteBounds.extents.y);
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Bounds colliderBounds = collider.bounds;
            return new Vector2(colliderBounds.extents.x, colliderBounds.extents.y);
        }

        return Vector2.zero;
    }
}
