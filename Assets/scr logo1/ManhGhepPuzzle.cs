using UnityEngine;

/// <summary>
/// Mảnh ghép của sliding puzzle.
/// - correctIndex: vị trí đúng trong ảnh gốc (không bao giờ đổi)
/// - currentIndex: vị trí hiện tại trên bàn puzzle (có thể đổi)
/// </summary>
public class ManhGhepPuzzle : MonoBehaviour
{
    public QuanLyXepHinh quanLy;
    public int correctIndex;    // Vị trí đúng của mảnh trong ảnh gốc
    public int currentIndex;    // Vị trí hiện tại trên bàn puzzle

    private void OnMouseDown()
    {
        if (quanLy != null)
        {
            quanLy.ThuDiChuyen(this);
        }
    }
}