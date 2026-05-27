using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UIResizer : MonoBehaviour
{
    [Header("🎯 TRÌNH THU PHÓNG ẢNH TỰ ĐỘNG")]
    [Tooltip("Kéo thanh này để bóp nhỏ hoặc phóng to ảnh mà không sợ méo!")]
    [Range(0.05f, 5f)]
    public float TyLeKichThuoc = 1f;

    private float lastScale = -1f;
    private Sprite lastSprite;

    private void Update()
    {
        Image img = GetComponent<Image>();
        if (img == null || img.sprite == null) return;

        // Chỉ cập nhật khi mày kéo thanh trượt hoặc đổi ảnh mới
        if (TyLeKichThuoc != lastScale || img.sprite != lastSprite)
        {
            lastScale = TyLeKichThuoc;
            lastSprite = img.sprite;

            float nativeWidth = img.sprite.rect.width;
            float nativeHeight = img.sprite.rect.height;
            
            RectTransform rt = GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nativeWidth * TyLeKichThuoc);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, nativeHeight * TyLeKichThuoc);
        }
    }
}
