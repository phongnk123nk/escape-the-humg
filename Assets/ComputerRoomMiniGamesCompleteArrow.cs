using UnityEngine;

/// <summary>
/// Bat mui ten thoat khoi man hinh may tinh khi ca 2 mini-game da hoan thanh.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Mini Games Complete Arrow")]
public class ComputerRoomMiniGamesCompleteArrow : MonoBehaviour
{
    public ComputerRoomMiniGameIcon horseIcon;
    public ComputerRoomMiniGameIcon foodIcon;
    public GameObject arrowToFrame46;

    private void Awake()
    {
        AutoFindReferences();
        UpdateArrowVisibility();
    }

    private void OnEnable()
    {
        AutoFindReferences();
        UpdateArrowVisibility();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UpdateArrowVisibility();
    }

    private void AutoFindReferences()
    {
        if (horseIcon == null)
        {
            Transform horse = transform.Find("MiniGameIcon_Horse");
            if (horse != null)
            {
                horseIcon = horse.GetComponent<ComputerRoomMiniGameIcon>();
            }
        }

        if (foodIcon == null)
        {
            Transform food = transform.Find("MiniGameIcon_Food");
            if (food != null)
            {
                foodIcon = food.GetComponent<ComputerRoomMiniGameIcon>();
            }
        }

        if (arrowToFrame46 == null)
        {
            Transform arrow = transform.Find("GoToFrame46AfterMiniGames");
            if (arrow != null)
            {
                arrowToFrame46 = arrow.gameObject;
            }
        }
    }

    private void UpdateArrowVisibility()
    {
        if (arrowToFrame46 == null)
        {
            return;
        }

        bool bothCompleted = IsIconCompleted(horseIcon) && IsIconCompleted(foodIcon);
        arrowToFrame46.SetActive(bothCompleted);
    }

    private bool IsIconCompleted(ComputerRoomMiniGameIcon icon)
    {
        if (icon == null)
        {
            return false;
        }

        return icon.completed || icon.transform.Find("MiniGameResultNumber") != null;
    }
}
