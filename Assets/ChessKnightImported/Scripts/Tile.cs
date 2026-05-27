using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    public int xBoard { get; private set; }
    public int yBoard { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private GameObject highlightDot;
    private bool isGoal;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int x, int y, Color color)
    {
        xBoard = x;
        yBoard = y;
        baseColor = color;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.color = baseColor;
    }

    // Dat o nay thanh bai co muc tieu.
    public void SetAsGoal(Sprite goalSprite, float scale = 0.7f)
    {
        isGoal = true;

        Transform oldGoal = transform.Find("GoalVisual");
        if (oldGoal != null)
        {
            if (Application.isPlaying)
            {
                Destroy(oldGoal.gameObject);
            }
            else
            {
                DestroyImmediate(oldGoal.gameObject);
            }
        }

        if (goalSprite == null)
        {
            return;
        }

        GameObject goalObj = new GameObject("GoalVisual");
        goalObj.transform.SetParent(transform);
        goalObj.transform.localPosition = Vector3.zero;
        goalObj.transform.localRotation = Quaternion.identity;

        SpriteRenderer sr = goalObj.AddComponent<SpriteRenderer>();
        sr.sprite = goalSprite;
        int sortingBase = 2000;
        if (GameManager.Instance != null && GameManager.Instance.boardManager != null)
        {
            sortingBase = GameManager.Instance.boardManager.sortingOrderBase;
        }

        sr.sortingOrder = sortingBase + 35;

        float spriteSize = Mathf.Max(goalSprite.bounds.size.x, goalSprite.bounds.size.y);
        if (spriteSize > 0f)
        {
            float realTileSize = 1f;
            if (GameManager.Instance != null && GameManager.Instance.boardManager != null)
            {
                realTileSize = GameManager.Instance.boardManager.tileSize;
            }

            float targetSize = realTileSize * scale;
            float finalScale = targetSize / spriteSize;
            Vector3 parentScale = transform.lossyScale;
            float parentScaleX = Mathf.Approximately(parentScale.x, 0f) ? 1f : Mathf.Abs(parentScale.x);
            float parentScaleY = Mathf.Approximately(parentScale.y, 0f) ? 1f : Mathf.Abs(parentScale.y);
            goalObj.transform.localScale = new Vector3(finalScale / parentScaleX, finalScale / parentScaleY, 1f);
        }
    }

    public void RemoveGoal()
    {
        isGoal = false;

        Transform goalTransform = transform.Find("GoalVisual");
        if (goalTransform == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(goalTransform.gameObject);
        }
        else
        {
            DestroyImmediate(goalTransform.gameObject);
        }
    }

    public void SetupHighlightDot(GameObject dotPrefab)
    {
        if (dotPrefab == null)
        {
            return;
        }

        highlightDot = Instantiate(dotPrefab, transform);
        highlightDot.transform.localPosition = Vector3.zero;
        highlightDot.SetActive(false);

        SpriteRenderer dotSr = highlightDot.GetComponent<SpriteRenderer>();
        if (dotSr != null)
        {
            int sortingBase = 2000;
            if (GameManager.Instance != null && GameManager.Instance.boardManager != null)
            {
                sortingBase = GameManager.Instance.boardManager.sortingOrderBase;
            }

            dotSr.sortingOrder = sortingBase + 30;
        }
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (highlightDot != null)
        {
            highlightDot.SetActive(isHighlighted);
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.color = isHighlighted ? Color.yellow : baseColor;
    }
}
