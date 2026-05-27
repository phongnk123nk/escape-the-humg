using UnityEngine;

/// <summary>
/// Dieu chinh nhanh preview mini-game co ngua trong Scene View.
/// Keo object ChessKnightMiniGame roi sua cac gia tri nay trong Inspector.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Chess Knight Preview Controls")]
public class ChessKnightMiniGamePreviewControls : MonoBehaviour
{
    [Header("References")]
    public Transform boardContainer;
    public Transform previewKnight;
    public Transform previewGrass;
    public Transform runtimeKnight;
    public BoardManager boardManager;

    [Header("Scale")]
    [Min(0.1f)] public float boardScale = 1f;
    [Min(0.1f)] public float knightScale = 1f;
    [Min(0.1f)] public float grassScale = 1f;

    [Header("Sorting")]
    public bool forcePuzzleOnTop = true;
    public int sortingOrderBase = 2000;

    private float lastBoardScale;
    private float lastKnightScale;
    private float lastGrassScale;

    private void OnEnable()
    {
        lastBoardScale = boardScale;
        lastKnightScale = knightScale;
        lastGrassScale = grassScale;
    }

    private void OnValidate()
    {
        Apply();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            SyncScaleValuesFromDirectEdits();
            Apply();
        }
    }

    private void LateUpdate()
    {
        if (Application.isPlaying)
        {
            ApplyRuntimeValues();
            ApplySorting();
        }
    }

    public void Apply()
    {
        if (boardContainer != null)
        {
            boardContainer.localScale = Vector3.one * boardScale;
        }

        if (previewKnight != null)
        {
            previewKnight.localScale = Vector3.one * knightScale;
        }

        if (runtimeKnight != null)
        {
            runtimeKnight.localScale = Vector3.one * knightScale;
        }

        if (previewGrass != null)
        {
            previewGrass.localScale = Vector3.one * grassScale;
        }

        ApplyRuntimeValues();
        ApplySorting();
        RememberAppliedScales();
    }

    private void SyncScaleValuesFromDirectEdits()
    {
        if (boardContainer != null)
        {
            float sceneBoardScale = Mathf.Max(0.1f, boardContainer.localScale.x);
            if (!Mathf.Approximately(sceneBoardScale, lastBoardScale))
            {
                boardScale = sceneBoardScale;
            }
        }

        if (previewKnight != null)
        {
            float sceneKnightScale = Mathf.Max(0.1f, previewKnight.localScale.x);
            if (!Mathf.Approximately(sceneKnightScale, lastKnightScale))
            {
                knightScale = sceneKnightScale;
            }
        }

        if (runtimeKnight != null)
        {
            float sceneRuntimeKnightScale = Mathf.Max(0.1f, runtimeKnight.localScale.x);
            if (!Mathf.Approximately(sceneRuntimeKnightScale, lastKnightScale))
            {
                knightScale = sceneRuntimeKnightScale;
            }
        }

        if (previewGrass != null)
        {
            float sceneGrassScale = Mathf.Max(0.1f, previewGrass.localScale.x);
            if (!Mathf.Approximately(sceneGrassScale, lastGrassScale))
            {
                grassScale = sceneGrassScale;
            }
        }
    }

    private void RememberAppliedScales()
    {
        lastBoardScale = boardScale;
        lastKnightScale = knightScale;
        lastGrassScale = grassScale;

        if (boardContainer != null)
        {
            boardContainer.hasChanged = false;
        }

        if (previewKnight != null)
        {
            previewKnight.hasChanged = false;
        }

        if (runtimeKnight != null)
        {
            runtimeKnight.hasChanged = false;
        }

        if (previewGrass != null)
        {
            previewGrass.hasChanged = false;
        }
    }

    private void ApplyRuntimeValues()
    {
        if (runtimeKnight != null)
        {
            runtimeKnight.localScale = Vector3.one * knightScale;
        }

        if (boardManager != null)
        {
            boardManager.sortingOrderBase = sortingOrderBase;
        }

        if (boardManager != null && boardManager.goalSprite != null && boardManager.tileSize > 0f)
        {
            float spriteSize = Mathf.Max(boardManager.goalSprite.bounds.size.x, boardManager.goalSprite.bounds.size.y);
            if (spriteSize > 0f)
            {
                boardManager.goalScale = Mathf.Max(0.1f, grassScale * spriteSize / boardManager.tileSize);
            }
        }
    }

    private void ApplySorting()
    {
        if (!forcePuzzleOnTop)
        {
            return;
        }

        if (boardManager != null)
        {
            boardManager.sortingOrderBase = sortingOrderBase;
        }

        if (boardContainer != null)
        {
            SpriteRenderer[] renderers = boardContainer.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                string objectName = renderer.gameObject.name;

                if (objectName.Contains("BoardBackground"))
                {
                    renderer.sortingOrder = sortingOrderBase;
                }
                else if (objectName.Contains("Tile"))
                {
                    renderer.sortingOrder = sortingOrderBase + 10;
                }
                else if (objectName.Contains("Grass") || objectName.Contains("Goal"))
                {
                    renderer.sortingOrder = sortingOrderBase + 35;
                }
                else if (objectName.Contains("Knight"))
                {
                    renderer.sortingOrder = sortingOrderBase + 40;
                }
                else
                {
                    renderer.sortingOrder = sortingOrderBase + 20;
                }
            }

            MeshRenderer[] meshRenderers = boardContainer.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].sortingOrder = sortingOrderBase + 45;
            }
        }

        if (runtimeKnight != null)
        {
            SpriteRenderer knightRenderer = runtimeKnight.GetComponent<SpriteRenderer>();
            if (knightRenderer != null)
            {
                knightRenderer.sortingOrder = sortingOrderBase + 40;
            }
        }
    }
}
