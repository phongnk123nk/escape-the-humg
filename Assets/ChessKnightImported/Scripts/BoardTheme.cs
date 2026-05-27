using UnityEngine;

[CreateAssetMenu(fileName = "New Board Theme", menuName = "Knight Game/Board Theme")]
public class BoardTheme : ScriptableObject
{
    [Header("Board Background")]
    [Tooltip("Ảnh bàn cờ hoàn chỉnh của bạn")]
    public Sprite customBoardSprite;
    
    [Tooltip("Tích vào đây để làm trong suốt các ô cờ, giúp thấy rõ ảnh bàn cờ ở dưới")]
    public bool hideTileColorsToSeeBackground = false;

    [Header("Knight Piece")]
    [Tooltip("Ảnh quân mã của bạn")]
    public Sprite knightSprite;

    [Header("Goal")]
    [Tooltip("Ảnh đích đến (ví dụ: vương miện, cờ)")]
    public Sprite goalSprite;
    
    [Range(0.1f, 2.0f)]
    [Tooltip("Tỷ lệ kích thước ảnh goal so với ô cờ (1.0 = vừa khít)")]
    public float goalScale = 0.7f;

    [Header("Tile Settings (Dùng khi không có ảnh bàn cờ)")]
    public Color lightTileColor = new Color(0.93f, 0.93f, 0.82f);
    public Color darkTileColor = new Color(0.46f, 0.58f, 0.33f);
    public Sprite defaultTileSprite;
}
