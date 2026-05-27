using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
[AddComponentMenu("Lab/Lab Machine Puzzle Trigger")]
public class LabMachinePuzzleTrigger : MonoBehaviour
{
    public GameObject puzzlePanel;
    public LabEquationPuzzleManager puzzleManager;
    public LabSceneNavigator sceneNavigator;
    public string requiredViewName = "MachineView";

    private void Awake()
    {
        EnsureCollider();

        if (sceneNavigator == null)
        {
            sceneNavigator = FindFirstObjectByType<LabSceneNavigator>();
        }

        if (puzzleManager == null)
        {
            puzzleManager = FindFirstObjectByType<LabEquationPuzzleManager>();
        }
    }

    private void OnValidate()
    {
        EnsureCollider();
    }

    private void EnsureCollider()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = false;
        LabBoxColliderPreview.Attach(
            boxCollider,
            new Color(0.15f, 1f, 0.55f, 0.18f),
            new Color(0.15f, 1f, 0.55f, 0.95f),
            "Machine Puzzle Click");
    }

    private void OnMouseDown()
    {
        if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
        {
            return;
        }

        TryOpenPuzzle();
    }

    public void TryOpenPuzzle()
    {
        if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
        {
            return;
        }

        if (sceneNavigator != null && !string.Equals(sceneNavigator.CurrentScreenName, requiredViewName, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (puzzlePanel == null)
        {
            Debug.LogError("PuzzlePanel is missing", this);
            return;
        }

        if (puzzleManager == null)
        {
            Debug.LogError("PuzzleManager is missing", this);
            return;
        }

        puzzlePanel.SetActive(true);
        puzzleManager.OpenPuzzle();
    }
}
