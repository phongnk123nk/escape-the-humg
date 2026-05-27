using UnityEngine;

// Gắn script này vào cùng GameObject với logoLon (logo bình thường)
// Script này tự động gọi LogoMorphController khi logoLon được hiện
public class LogoMorphTrigger : MonoBehaviour
{
    [Header("Tham chiếu")]
    [InspectorName("Logo Morph Controller")]
    public LogoMorphController logoMorphController;

    [Header("Thời gian")]
    [InspectorName("Độ Trễ Trước Khi Bắt Đầu")]
    public float delayBeforeStart = 0.5f; // chờ 0.5s sau khi logoLon hiện ra

    private bool daChay = false;

    private void OnEnable()
    {
        // Khi GameObject này được Enable (hiện ra)
        if (!daChay)
        {
            daChay = true;
            
            if (logoMorphController == null)
            {
                logoMorphController = GetComponent<LogoMorphController>();
            }

            if (logoMorphController != null)
            {
                // Gọi StartMorph sau delay
                Invoke(nameof(StartMorphSequence), delayBeforeStart);
            }
            else
            {
                Debug.LogWarning("LogoMorphController chưa được gán trong LogoMorphTrigger");
            }
        }
    }

    private void OnDisable()
    {
        // Hủy Invoke nếu GameObject bị tắt trước khi chạy
        CancelInvoke(nameof(StartMorphSequence));
    }

    private void StartMorphSequence()
    {
        if (logoMorphController != null)
        {
            logoMorphController.StartMorph();
        }
    }
}
