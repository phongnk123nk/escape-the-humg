using UnityEngine;

/// <summary>
/// Gan vao sprite o khoa trong Frame50_DoorCloseView.
/// Click vao o khoa se mo bang nhap 2 so.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Door Lock Puzzle Trigger")]
public class ComputerRoomDoorLockPuzzleTrigger : MonoBehaviour
{
    public ComputerRoomNavigator navigator;

    private void Reset()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        collider2D.isTrigger = true;
    }

    private void Awake()
    {
        CacheNavigator();
    }

    private void OnMouseDown()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        CacheNavigator();

        if (navigator != null)
        {
            navigator.OpenDoorLockPuzzle();
        }
    }

    private void CacheNavigator()
    {
        if (navigator != null)
        {
            return;
        }

        navigator = GetComponentInParent<ComputerRoomNavigator>();
        if (navigator == null)
        {
            navigator = FindFirstObjectByType<ComputerRoomNavigator>();
        }
    }
}
