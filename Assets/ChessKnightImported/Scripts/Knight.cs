using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knight : MonoBehaviour
{
    public int currentX { get; private set; }
    public int currentY { get; private set; }

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private readonly int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
    private readonly int[] dy = { 2, 1, -1, -2, -2, -1, 1, 2 };

    private bool isDragging;
    private bool isMoving;
    private Vector2 startDragPos;

    public void Initialize(int startX, int startY)
    {
        StopAllCoroutines();
        isDragging = false;
        isMoving = false;
        currentX = startX;
        currentY = startY;
        UpdatePosition(false);
    }

    public void MoveTo(int x, int y, bool animate = true)
    {
        if (isMoving)
        {
            return;
        }

        currentX = x;
        currentY = y;
        UpdatePosition(animate);
        GameManager.Instance.CheckWinCondition(x, y);
    }

    private void UpdatePosition(bool animate)
    {
        Vector2 targetPos = GameManager.Instance.boardManager.GetWorldPosition(currentX, currentY);

        if (animate)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateMove(targetPos));
        }
        else
        {
            transform.position = targetPos;
        }
    }

    private IEnumerator AnimateMove(Vector2 targetPos)
    {
        isMoving = true;
        while ((Vector2)transform.position != targetPos)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    public List<Vector2Int> GetValidMoves()
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();

        for (int i = 0; i < 8; i++)
        {
            int newX = currentX + dx[i];
            int newY = currentY + dy[i];

            if (GameManager.Instance.boardManager.IsValidPosition(newX, newY))
            {
                validMoves.Add(new Vector2Int(newX, newY));
            }
        }

        return validMoves;
    }

    public bool CanMoveTo(int x, int y)
    {
        return GetValidMoves().Contains(new Vector2Int(x, y));
    }

    public void StartDrag()
    {
        if (isMoving)
        {
            return;
        }

        isDragging = true;
        startDragPos = transform.position;
    }

    public bool IsDragging()
    {
        return isDragging;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    private void Update()
    {
        if (!isDragging)
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

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    public void StopDrag(Vector2 dropPos)
    {
        if (isMoving)
        {
            return;
        }

        isDragging = false;
        float dragDistance = Vector2.Distance(startDragPos, dropPos);

        if (dragDistance < 0.2f)
        {
            transform.position = startDragPos;
            return;
        }

        Vector2Int? closestTile = GameManager.Instance.boardManager.GetClosestTile(dropPos);
        if (closestTile.HasValue)
        {
            int tX = closestTile.Value.x;
            int tY = closestTile.Value.y;

            if (tX == currentX && tY == currentY)
            {
                transform.position = startDragPos;
                return;
            }

            if (CanMoveTo(tX, tY))
            {
                GameManager.Instance.DeselectKnight();
                MoveTo(tX, tY, false);
                return;
            }
        }

        GameManager.Instance.DeselectKnight();
        StopAllCoroutines();
        StartCoroutine(AnimateMove(startDragPos));
    }
}
