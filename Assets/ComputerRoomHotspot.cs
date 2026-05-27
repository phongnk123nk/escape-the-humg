using UnityEngine;

/// <summary>
/// Vung click that trong scene PhongTinHoc.
/// Gan script nay vao tung hotspot de co the keo/chinh BoxCollider2D truc tiep trong Scene View.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Computer Room Hotspot")]
public class ComputerRoomHotspot : MonoBehaviour
{
    [Header("Hotspot")]
    public string hotspotName = "Hotspot";
    public ComputerRoomView visibleOnView;
    public ComputerRoomView targetView;

    [Header("Rules")]
    public bool startsComputerPuzzle;
    public bool requiresPuzzleSolved;
    public bool isExitDoor;
    public bool isContinueAfterPuzzle;
    public bool canUseWhenPuzzleLocked;

    [Header("Preview")]
    public SpriteRenderer debugRenderer;
    public BoxCollider2D boxCollider;

    [Header("Float Animation")]
    public bool enableFloatAnimation = true;
    [Min(0f)]
    public float floatAmplitude = 0.12f;
    [Min(0f)]
    public float floatSpeed = 2f;

    private ComputerRoomNavigator navigator;
    private Vector3 floatBaseLocalPosition;
    private bool floatBaseCaptured;

    private void Reset()
    {
        CacheComponents();
        hotspotName = gameObject.name;
        boxCollider.isTrigger = true;
    }

    private void OnValidate()
    {
        CacheComponents();

        if (string.IsNullOrWhiteSpace(hotspotName))
        {
            hotspotName = gameObject.name;
        }

        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        CacheNavigator();

        if (Application.isPlaying)
        {
            CaptureFloatBasePosition();
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying && floatBaseCaptured)
        {
            transform.localPosition = floatBaseLocalPosition;
            floatBaseCaptured = false;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying || !enableFloatAnimation || floatAmplitude <= 0f || floatSpeed <= 0f)
        {
            return;
        }

        CaptureFloatBasePosition();
        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = floatBaseLocalPosition + new Vector3(0f, offsetY, 0f);
    }

    private void OnMouseDown()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        CacheNavigator();

        if (navigator != null)
        {
            navigator.HandleHotspotClick(this);
        }
        else
        {
            Debug.LogWarning("ComputerRoomHotspot khong tim thay ComputerRoomNavigator: " + hotspotName, this);
        }
    }

    public void SetPreviewVisible(bool visible)
    {
        if (debugRenderer != null)
        {
            debugRenderer.enabled = visible;
        }
    }

    private void CacheComponents()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        if (debugRenderer == null)
        {
            debugRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void CacheNavigator()
    {
        if (navigator != null)
        {
            return;
        }

        navigator = GetComponentInParent<ComputerRoomNavigator>();

        if (navigator == null)
        {
            navigator = FindFirstObjectByType<ComputerRoomNavigator>();
        }
    }

    private void CaptureFloatBasePosition()
    {
        if (floatBaseCaptured)
        {
            return;
        }

        floatBaseLocalPosition = transform.localPosition;
        floatBaseCaptured = true;
    }

    private void OnDrawGizmos()
    {
        DrawPreviewGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        DrawPreviewGizmo();
    }

    private void DrawPreviewGizmo()
    {
        CacheComponents();

        ComputerRoomNavigator owner = GetComponentInParent<ComputerRoomNavigator>();
        if (owner != null && !owner.showHotspotPreview)
        {
            return;
        }

        if (boxCollider == null || !boxCollider.enabled)
        {
            return;
        }

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector3 center = new Vector3(boxCollider.offset.x, boxCollider.offset.y, 0f);
        Vector3 size = new Vector3(boxCollider.size.x, boxCollider.size.y, 0.02f);

        Gizmos.color = startsComputerPuzzle
            ? new Color(0.2f, 0.8f, 1f, 0.26f)
            : new Color(1f, 0.85f, 0.05f, 0.24f);
        Gizmos.DrawCube(center, size);

        Gizmos.color = requiresPuzzleSolved
            ? new Color(0.25f, 1f, 0.35f, 0.95f)
            : new Color(1f, 0.75f, 0.05f, 0.95f);
        Gizmos.DrawWireCube(center, size);

        Gizmos.matrix = oldMatrix;

#if UNITY_EDITOR
        string label = string.IsNullOrWhiteSpace(hotspotName) ? gameObject.name : hotspotName;
        Vector3 labelPosition = transform.TransformPoint(center + new Vector3(0f, boxCollider.size.y * 0.5f + 0.12f, 0f));
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(labelPosition, label);
#endif
    }
}
