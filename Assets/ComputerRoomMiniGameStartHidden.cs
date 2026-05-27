using UnityEngine;

/// <summary>
/// Cho phep mini-game hien trong Edit Mode de can chinh, nhung tu an khi bam Play.
/// Icon mini-game se bat object nay len lai.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Mini Game Start Hidden")]
public class ComputerRoomMiniGameStartHidden : MonoBehaviour
{
    public bool hideOnPlay = true;

    private void Awake()
    {
        if (Application.isPlaying && hideOnPlay)
        {
            gameObject.SetActive(false);
        }
    }
}
