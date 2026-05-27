using UnityEngine;

/// <summary>
/// Chuyen trigger cua xe ve DeliveryOrderMiniGameManager.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Delivery Order Car Trigger")]
public class DeliveryOrderCarTrigger : MonoBehaviour
{
    public DeliveryOrderMiniGameManager manager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (manager != null)
        {
            manager.HandleCarTrigger(other);
        }
    }
}
