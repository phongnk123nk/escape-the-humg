using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("Lab/Lab Key Door Exit")]
public class LabKeyDoorExit : MonoBehaviour
{
    public LabInventorySystem inventoryManager;
    public LabSceneNavigator sceneNavigator;
    public string requiredViewName = "BackView";
    public string requiredItemId = "lab_key";
    public string nextSceneName = "BangXepHinh";
    public bool allowSelectedItemClick = true;

    private void Reset()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.45f, 2.35f);
        LabBoxColliderPreview.Attach(
            box,
            new Color(1f, 0.85f, 0.15f, 0.18f),
            new Color(1f, 0.85f, 0.15f, 0.95f),
            "Key Door Exit");
    }

    private void Awake()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<LabInventorySystem>();
        }

        if (sceneNavigator == null)
        {
            sceneNavigator = FindFirstObjectByType<LabSceneNavigator>();
        }

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            LabBoxColliderPreview.Attach(
                box,
                new Color(1f, 0.85f, 0.15f, 0.18f),
                new Color(1f, 0.85f, 0.15f, 0.95f),
                "Key Door Exit");
        }
    }

    private void OnMouseDown()
    {
        if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
        {
            return;
        }

        if (!allowSelectedItemClick || inventoryManager == null)
        {
            return;
        }

        if (IsCorrectView() && IsRequiredItem(inventoryManager.SelectedItemId))
        {
            LoadNextScene();
        }
    }

    public bool TryUseDraggedItem(InventoryDragItem dragItem)
    {
        if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
        {
            return false;
        }

        if (dragItem == null)
        {
            return false;
        }

        if (!IsCorrectView())
        {
            return false;
        }

        if (!IsRequiredItem(dragItem.itemId))
        {
            Debug.Log("Can dung chia khoa de mo cua.", this);
            return false;
        }

        LoadNextScene();
        return true;
    }

    private bool IsCorrectView()
    {
        if (sceneNavigator == null || string.IsNullOrWhiteSpace(requiredViewName))
        {
            return true;
        }

        return string.Equals(sceneNavigator.CurrentScreenName, requiredViewName, System.StringComparison.OrdinalIgnoreCase);
    }

    private bool IsRequiredItem(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId)
            && string.Equals(itemId, requiredItemId, System.StringComparison.OrdinalIgnoreCase);
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("Next scene name is missing.", this);
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.15f, 0.35f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        Vector3 size = box != null ? new Vector3(box.size.x, box.size.y, 0.05f) : new Vector3(1.45f, 2.35f, 0.05f);
        Gizmos.DrawCube(transform.position, size);
    }

    // Returns true if the given world-space point lies within this door's collider area.
    public bool ContainsWorldPoint(Vector2 worldPoint)
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            return false;
        }

        // Use the collider's world-space bounds to test containment.
        Vector3 testPoint = new Vector3(worldPoint.x, worldPoint.y, transform.position.z);
        return box.bounds.Contains(testPoint);
    }
}
