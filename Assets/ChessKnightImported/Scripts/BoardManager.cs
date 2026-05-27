using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 3;
    public int height = 3;
    public float tileSize = 1f;
    public float spacing = 0f;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject highlightDotPrefab;

    [Header("Interaction Settings")]
    [Range(0.1f, 1.5f)]
    public float clickRadiusMultiplier = 0.7f;

    [Header("Debug")]
    public bool showDebugGrid = true;
    public int sortingOrderBase = 2000;

    [Header("Colors & Visuals")]
    public Color lightTileColor = new Color(0.93f, 0.93f, 0.82f);
    public Color darkTileColor = new Color(0.46f, 0.58f, 0.33f);
    public Sprite defaultTileSprite;
    public Sprite goalSprite;
    public float goalScale = 0.7f;

    [Header("Custom Board Asset")]
    public Sprite customBoardSprite;
    public bool hideTileColorsToSeeBackground = false;

    [Header("Board Container")]
    public Transform boardContainer;
    public bool centerCameraOnBoard = false;

    private Tile[,] tiles;
    private GameObject backgroundVisualObj;

    public void GenerateBoard()
    {
        ClearBoard();
        tiles = new Tile[width, height];

        Transform parentTransform = boardContainer != null ? boardContainer : transform;
        float totalWidth = (width - 1) * (tileSize + spacing);
        float totalHeight = (height - 1) * (tileSize + spacing);

        if (backgroundVisualObj != null)
        {
            Destroy(backgroundVisualObj);
        }

        if (customBoardSprite != null)
        {
            backgroundVisualObj = new GameObject("BoardBackgroundVisual");
            backgroundVisualObj.transform.SetParent(parentTransform);
            backgroundVisualObj.transform.localPosition = Vector3.zero;

            SpriteRenderer bgSr = backgroundVisualObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = customBoardSprite;
            bgSr.sortingOrder = sortingOrderBase;

            float totalPhysicalWidth = width * tileSize + (width - 1) * spacing;
            float totalPhysicalHeight = height * tileSize + (height - 1) * spacing;
            float spriteWidth = customBoardSprite.bounds.size.x;
            float spriteHeight = customBoardSprite.bounds.size.y;
            if (spriteWidth > 0f && spriteHeight > 0f)
            {
                backgroundVisualObj.transform.localScale = new Vector3(totalPhysicalWidth / spriteWidth, totalPhysicalHeight / spriteHeight, 1f);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tileObj = Instantiate(tilePrefab, parentTransform);
                tileObj.name = $"Tile_{x}_{y}";

                float posX = x * (tileSize + spacing) - (totalWidth / 2f);
                float posY = y * (tileSize + spacing) - (totalHeight / 2f);
                tileObj.transform.localPosition = new Vector3(posX, posY, 0f);
                FitTileVisualToTileSize(tileObj);

                if (defaultTileSprite != null)
                {
                    SpriteRenderer spriteRenderer = tileObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = defaultTileSprite;
                        FitTileVisualToTileSize(tileObj);
                    }
                }

                SpriteRenderer tileRenderer = tileObj.GetComponent<SpriteRenderer>();
                if (tileRenderer != null)
                {
                    tileRenderer.sortingOrder = sortingOrderBase + 10;
                }

                Tile tile = tileObj.GetComponent<Tile>();
                bool isLight = (x + y) % 2 != 0;
                Color baseColor = isLight ? lightTileColor : darkTileColor;
                Color finalColor = hideTileColorsToSeeBackground ? new Color(baseColor.r, baseColor.g, baseColor.b, 0.28f) : baseColor;
                tile.Initialize(x, y, finalColor);
                tile.SetupHighlightDot(highlightDotPrefab);

                tiles[x, y] = tile;
            }
        }

        if (centerCameraOnBoard && Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(parentTransform.position.x, parentTransform.position.y, -10f);
        }
    }

    public void ClearBoard()
    {
        Transform parentTransform = boardContainer != null ? boardContainer : transform;
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            GameObject child = parentTransform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        tiles = null;
    }

    private void FitTileVisualToTileSize(GameObject tileObj)
    {
        if (tileObj == null)
        {
            return;
        }

        SpriteRenderer renderer = tileObj.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
        {
            tileObj.transform.localScale = Vector3.one;
            return;
        }

        Vector2 spriteSize = renderer.sprite.bounds.size;
        float spriteMaxSize = Mathf.Max(spriteSize.x, spriteSize.y);
        if (spriteMaxSize <= 0f)
        {
            return;
        }

        float scale = tileSize / spriteMaxSize;
        tileObj.transform.localScale = new Vector3(scale, scale, 1f);

        BoxCollider2D collider = tileObj.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = renderer.sprite.bounds.size;
            collider.offset = Vector2.zero;
        }
    }

    public void SetGoal(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            tiles[x, y].SetAsGoal(goalSprite, goalScale);
        }
    }

    public void ClearGoal(int x, int y)
    {
        if (IsValidPosition(x, y) && tiles[x, y] != null)
        {
            tiles[x, y].RemoveGoal();
        }
    }

    public void HighlightTile(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            tiles[x, y].SetHighlight(true);
        }
    }

    public void ClearHighlights()
    {
        if (tiles == null)
        {
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].SetHighlight(false);
                }
            }
        }
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        if (tiles != null && tiles[x, y] != null)
        {
            return tiles[x, y].transform.position;
        }

        float totalWidth = (width - 1) * (tileSize + spacing);
        float totalHeight = (height - 1) * (tileSize + spacing);
        float posX = x * (tileSize + spacing) - (totalWidth / 2f);
        float posY = y * (tileSize + spacing) - (totalHeight / 2f);

        Transform parentTransform = boardContainer != null ? boardContainer : transform;
        return parentTransform.TransformPoint(new Vector3(posX, posY, 0f));
    }

    public Vector2Int? GetClosestTile(Vector2 worldPos)
    {
        float minDistance = float.MaxValue;
        Vector2Int? closest = null;
        float clickRadius = GetWorldClickRadius();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 tilePos = GetWorldPosition(x, y);
                float dist = Vector2.Distance(worldPos, tilePos);
                if (dist < minDistance && dist <= clickRadius)
                {
                    minDistance = dist;
                    closest = new Vector2Int(x, y);
                }
            }
        }

        return closest;
    }

    private float GetWorldClickRadius()
    {
        Transform parentTransform = boardContainer != null ? boardContainer : transform;
        Vector3 scale = parentTransform.lossyScale;
        float worldScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        if (worldScale <= 0f)
        {
            worldScale = 1f;
        }

        return tileSize * worldScale * clickRadiusMultiplier;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGrid)
        {
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = GetWorldPosition(x, y);
                Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
                Gizmos.DrawWireCube(pos, new Vector3(tileSize, tileSize, 0.1f));
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                Gizmos.DrawWireSphere(pos, GetWorldClickRadius());
            }
        }
    }
}
