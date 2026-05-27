using UnityEngine;

/// <summary>
/// Tao nen trang mo hinh chu nhat cho mini-game xe.
/// Scale cua GameObject nay chinh la kich thuoc vung choi.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Delivery Playable Area Visual")]
public class DeliveryPlayableAreaVisual : MonoBehaviour
{
    public Color backgroundColor = new Color(1f, 1f, 1f, 0.38f);
    public int sortingOrder = 2190;

    private static Texture2D whiteTexture;
    private Sprite generatedSprite;

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    private void Apply()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetWhiteSprite();
        spriteRenderer.color = backgroundColor;
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.offset = Vector2.zero;
        boxCollider.size = Vector2.one;
    }

    private Sprite GetWhiteSprite()
    {
        if (generatedSprite != null)
        {
            return generatedSprite;
        }

        if (whiteTexture == null)
        {
            whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            whiteTexture.hideFlags = HideFlags.HideAndDontSave;
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }

        generatedSprite = Sprite.Create(whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        generatedSprite.hideFlags = HideFlags.HideAndDontSave;
        return generatedSprite;
    }
}
