using UnityEngine;

public class BamLogoBang : MonoBehaviour
{
    public QuanLyXepHinh quanLyXepHinh;

    private void OnMouseDown()
    {
        if (quanLyXepHinh != null)
        {
            quanLyXepHinh.BamVaoLogoMo();
        }
        else
        {
            Debug.LogWarning("Chưa kéo QuanLyXepHinh vào script BamLogoBang.");
        }
    }
}