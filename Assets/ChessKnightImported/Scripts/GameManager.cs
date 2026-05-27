using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public BoardManager boardManager;
    public Knight knight;
    public ComputerRoomNavigator computerRoomNavigator;

    [Header("Game Mode Settings")]
    public int goalsNeededToWin = 3;
    public bool randomGoalPlacement = true;
    [Min(1)]
    public int minimumKnightMovesToGoal = 2;
    public bool disallowCenterGoal = true;

    [Header("Completion Reward")]
    public bool hideMiniGameWhenSolved = true;
    public ComputerRoomMiniGameIcon iconToReplaceWithNumber;
    public string firstDoorCodeNumber = "1";

    [Header("Default Settings (If not random)")]
    public Vector2Int knightStartPos = new Vector2Int(0, 0);
    public Vector2Int goalPosition = new Vector2Int(2, 2);

    [Header("Theme")]
    public BoardTheme currentTheme;

    private bool isGameOver;
    private int currentGoalsReached;
    private readonly List<Vector2Int> usedGoalPositions = new List<Vector2Int>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void StartGame()
    {
        if (boardManager == null || knight == null)
        {
            Debug.LogWarning("Knight mini-game thieu BoardManager hoac Knight.");
            return;
        }

        isGameOver = false;
        currentGoalsReached = 0;
        usedGoalPositions.Clear();

        if (currentTheme != null)
        {
            ApplyTheme();
        }

        boardManager.GenerateBoard();
        knight.Initialize(knightStartPos.x, knightStartPos.y);
        EnsureKnightVisualOrder();

        if (randomGoalPlacement)
        {
            GenerateRandomGoal();
        }
        else
        {
            RememberGoalPosition(goalPosition);
            boardManager.SetGoal(goalPosition.x, goalPosition.y);
        }

        DeselectKnight();
    }

    private void GenerateRandomGoal()
    {
        List<Vector2Int> validGoalTiles = new List<Vector2Int>();
        Vector2Int knightPosition = new Vector2Int(knight.currentX, knight.currentY);

        for (int x = 0; x < boardManager.width; x++)
        {
            for (int y = 0; y < boardManager.height; y++)
            {
                Vector2Int candidate = new Vector2Int(x, y);
                if (usedGoalPositions.Contains(candidate))
                {
                    continue;
                }

                if (disallowCenterGoal && IsCenterTile(candidate))
                {
                    continue;
                }

                int moveDistance = GetKnightMoveDistance(knightPosition, candidate);
                if (moveDistance >= minimumKnightMovesToGoal)
                {
                    validGoalTiles.Add(candidate);
                }
            }
        }

        if (validGoalTiles.Count == 0)
        {
            Debug.LogWarning("Knight mini-game: Khong con o moi hop le de dat bai co theo dieu kien hien tai.");
            return;
        }

        int rnd = Random.Range(0, validGoalTiles.Count);
        goalPosition = validGoalTiles[rnd];
        RememberGoalPosition(goalPosition);
        boardManager.SetGoal(goalPosition.x, goalPosition.y);
    }

    private bool IsCenterTile(Vector2Int position)
    {
        return position.x == boardManager.width / 2 && position.y == boardManager.height / 2;
    }

    private int GetKnightMoveDistance(Vector2Int start, Vector2Int target)
    {
        if (start == target)
        {
            return 0;
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();
        queue.Enqueue(start);
        distances[start] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDistance = distances[current];
            List<Vector2Int> nextMoves = GetValidKnightMovesFrom(current);

            for (int i = 0; i < nextMoves.Count; i++)
            {
                Vector2Int next = nextMoves[i];
                if (distances.ContainsKey(next))
                {
                    continue;
                }

                int nextDistance = currentDistance + 1;
                if (next == target)
                {
                    return nextDistance;
                }

                distances[next] = nextDistance;
                queue.Enqueue(next);
            }
        }

        return -1;
    }

    private List<Vector2Int> GetValidKnightMovesFrom(Vector2Int position)
    {
        int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
        int[] dy = { 2, 1, -1, -2, -2, -1, 1, 2 };
        List<Vector2Int> moves = new List<Vector2Int>();

        for (int i = 0; i < dx.Length; i++)
        {
            int x = position.x + dx[i];
            int y = position.y + dy[i];
            if (boardManager.IsValidPosition(x, y))
            {
                moves.Add(new Vector2Int(x, y));
            }
        }

        return moves;
    }

    public bool isKnightSelected = false;

    public void SelectKnight()
    {
        if (isGameOver)
        {
            return;
        }

        isKnightSelected = true;
        boardManager.ClearHighlights();

        List<Vector2Int> validMoves = knight.GetValidMoves();
        for (int i = 0; i < validMoves.Count; i++)
        {
            boardManager.HighlightTile(validMoves[i].x, validMoves[i].y);
        }
    }

    public void DeselectKnight()
    {
        isKnightSelected = false;
        if (boardManager != null)
        {
            boardManager.ClearHighlights();
        }
    }

    public void OnTileClicked(int x, int y)
    {
        if (isGameOver || !isKnightSelected)
        {
            return;
        }

        if (knight.CanMoveTo(x, y))
        {
            DeselectKnight();
            knight.MoveTo(x, y, true);
            return;
        }

        DeselectKnight();
    }

    public void CheckWinCondition(int x, int y)
    {
        if (x != goalPosition.x || y != goalPosition.y)
        {
            return;
        }

        currentGoalsReached++;
        boardManager.ClearGoal(goalPosition.x, goalPosition.y);

        if (currentGoalsReached >= goalsNeededToWin)
        {
            Debug.Log("WIN! Hoan thanh mini-game quan ma.");
            isGameOver = true;

            ShowCompletionNumber();

            if (computerRoomNavigator != null)
            {
                computerRoomNavigator.OnComputerPuzzleSolved();
            }

            if (hideMiniGameWhenSolved)
            {
                gameObject.SetActive(false);
            }

            return;
        }

        Debug.Log("Da an duoc " + currentGoalsReached + "/" + goalsNeededToWin + " bai co.");
        if (randomGoalPlacement)
        {
            GenerateRandomGoal();
        }
    }

    public void PreviewInEditor()
    {
        if (boardManager == null || knight == null)
        {
            return;
        }

        boardManager.ClearBoard();
        usedGoalPositions.Clear();
        if (currentTheme != null)
        {
            ApplyTheme();
        }

        boardManager.GenerateBoard();
        knight.Initialize(knightStartPos.x, knightStartPos.y);
        EnsureKnightVisualOrder();

        if (randomGoalPlacement)
        {
            GenerateRandomGoal();
        }
        else
        {
            RememberGoalPosition(goalPosition);
            boardManager.SetGoal(goalPosition.x, goalPosition.y);
        }
    }

    private void RememberGoalPosition(Vector2Int position)
    {
        if (!usedGoalPositions.Contains(position))
        {
            usedGoalPositions.Add(position);
        }
    }

    public void ApplyTheme()
    {
        if (currentTheme == null || boardManager == null)
        {
            return;
        }

        boardManager.customBoardSprite = currentTheme.customBoardSprite;
        boardManager.hideTileColorsToSeeBackground = currentTheme.hideTileColorsToSeeBackground;

        if (currentTheme.goalSprite != null)
        {
            boardManager.goalSprite = currentTheme.goalSprite;
            boardManager.goalScale = currentTheme.goalScale;
        }

        boardManager.lightTileColor = currentTheme.lightTileColor;
        boardManager.darkTileColor = currentTheme.darkTileColor;

        if (currentTheme.defaultTileSprite != null)
        {
            boardManager.defaultTileSprite = currentTheme.defaultTileSprite;
        }

        if (currentTheme.knightSprite != null && knight != null)
        {
            SpriteRenderer knightSr = knight.GetComponent<SpriteRenderer>();
            if (knightSr != null)
            {
                knightSr.sprite = currentTheme.knightSprite;
                knightSr.color = Color.white;
            }
        }
    }

    private void EnsureKnightVisualOrder()
    {
        if (knight == null)
        {
            return;
        }

        SpriteRenderer knightRenderer = knight.GetComponent<SpriteRenderer>();
        if (knightRenderer != null)
        {
            int sortingBase = boardManager != null ? boardManager.sortingOrderBase : 2000;
            knightRenderer.sortingOrder = sortingBase + 40;
        }
    }

    private void ShowCompletionNumber()
    {
        if (iconToReplaceWithNumber == null)
        {
            return;
        }

        iconToReplaceWithNumber.ShowResultNumber(firstDoorCodeNumber);
    }

    private void Update()
    {
        if (isGameOver || boardManager == null || knight == null || knight.IsMoving())
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }

        if (cam == null)
        {
            return;
        }

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int? closestTile = boardManager.GetClosestTile(mousePos);
            if (closestTile.HasValue && closestTile.Value.x == knight.currentX && closestTile.Value.y == knight.currentY)
            {
                SelectKnight();
                knight.StartDrag();
            }
            else if (closestTile.HasValue)
            {
                OnTileClicked(closestTile.Value.x, closestTile.Value.y);
            }
            else
            {
                DeselectKnight();
            }
        }

        if (Input.GetMouseButtonUp(0) && knight.IsDragging())
        {
            knight.StopDrag(mousePos);
        }
    }

    public void ResetGame()
    {
        if (boardManager != null)
        {
            boardManager.ClearBoard();
        }

        StartGame();
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GameManager))]
public class GameManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager manager = (GameManager)target;

        UnityEditor.EditorGUILayout.Space();
        if (UnityEngine.GUILayout.Button("Preview Scene In Editor", UnityEngine.GUILayout.Height(40)))
        {
            manager.PreviewInEditor();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        }
    }
}
#endif
