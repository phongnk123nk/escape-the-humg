using UnityEngine;

[AddComponentMenu("Lab/Show Only In TableView")]
public class ShowOnlyInTableView : MonoBehaviour
{
    public string targetViewName = "TableView";
    private LabSceneNavigator sceneNavigator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        sceneNavigator = FindFirstObjectByType<LabSceneNavigator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisibility();
    }

    private void Update()
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (sceneNavigator == null || spriteRenderer == null)
        {
            return;
        }

        bool isTableView = string.Equals(sceneNavigator.CurrentScreenName, targetViewName, System.StringComparison.OrdinalIgnoreCase);
        spriteRenderer.enabled = isTableView;

        // Also disable the collider when not visible
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.enabled = isTableView;
        }
    }
}
