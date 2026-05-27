using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
[AddComponentMenu("Lab/Lab Box Collider Preview")]
public class LabBoxColliderPreview : MonoBehaviour
{
    public bool showAlways = true;
    public Color fillColor = new Color(1f, 0.85f, 0.05f, 0.18f);
    public Color lineColor = new Color(1f, 0.72f, 0.05f, 0.9f);
    public string label = "";

    public static LabBoxColliderPreview Attach(BoxCollider2D boxCollider, Color fill, Color line, string previewLabel)
    {
        if (boxCollider == null)
        {
            return null;
        }

        LabBoxColliderPreview preview = boxCollider.GetComponent<LabBoxColliderPreview>();
        if (preview == null)
        {
            preview = boxCollider.gameObject.AddComponent<LabBoxColliderPreview>();
        }

        preview.fillColor = fill;
        preview.lineColor = line;
        preview.label = previewLabel;
        preview.showAlways = true;
        return preview;
    }

    private void OnDrawGizmos()
    {
        if (showAlways)
        {
            DrawColliderGizmo();
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawColliderGizmo();
    }

    private void DrawColliderGizmo()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null || !box.enabled)
        {
            return;
        }

        Transform t = transform;
        if (t == null)
        {
            return;
        }

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = t.localToWorldMatrix;

        Vector3 center = new Vector3(box.offset.x, box.offset.y, 0f);
        Vector3 size = new Vector3(box.size.x, box.size.y, 0.02f);

        Gizmos.color = fillColor;
        Gizmos.DrawCube(center, size);
        Gizmos.color = lineColor;
        Gizmos.DrawWireCube(center, size);

        Gizmos.matrix = oldMatrix;

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(label))
        {
            Vector3 labelPosition = t.TransformPoint(center + new Vector3(0f, box.size.y * 0.5f + 0.08f, 0f));
            Handles.color = lineColor;
            Handles.Label(labelPosition, label);
        }
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LabBoxColliderPreview))]
public class LabBoxColliderPreviewEditor : Editor
{
    private const float HandleSize = 0.08f;

    private void OnSceneGUI()
    {
        LabBoxColliderPreview preview = (LabBoxColliderPreview)target;
        if (preview == null)
        {
            return;
        }

        BoxCollider2D box = preview.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            return;
        }

        Transform transform = preview.transform;
        if (transform == null)
        {
            return;
        }

        Vector3 centerWorld = transform.TransformPoint(new Vector3(box.offset.x, box.offset.y, 0f));

        EditorGUI.BeginChangeCheck();
        Vector3 movedCenter = Handles.PositionHandle(centerWorld, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(transform, "Move Box Collider Preview");
            Vector3 delta = movedCenter - centerWorld;
            transform.position += delta;
            EditorUtility.SetDirty(transform);
        }

        Vector2 halfSize = box.size * 0.5f;
        Vector2[] corners =
        {
            box.offset + new Vector2(-halfSize.x, -halfSize.y),
            box.offset + new Vector2(-halfSize.x, halfSize.y),
            box.offset + new Vector2(halfSize.x, halfSize.y),
            box.offset + new Vector2(halfSize.x, -halfSize.y)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            DrawCornerHandle(preview, box, i, corners);
        }
    }

    private void DrawCornerHandle(LabBoxColliderPreview preview, BoxCollider2D box, int cornerIndex, Vector2[] originalCorners)
    {
        if (preview == null || box == null || originalCorners == null || cornerIndex < 0 || cornerIndex >= originalCorners.Length)
        {
            return;
        }

        Transform transform = preview.transform;
        if (transform == null)
        {
            return;
        }

        Vector3 cornerWorld = transform.TransformPoint(new Vector3(originalCorners[cornerIndex].x, originalCorners[cornerIndex].y, 0f));
        float size = HandleUtility.GetHandleSize(cornerWorld) * HandleSize;

        Handles.color = preview.lineColor;
        EditorGUI.BeginChangeCheck();
        Vector3 movedWorld = Handles.FreeMoveHandle(cornerWorld, size, Vector3.zero, Handles.RectangleHandleCap);
        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        Undo.RecordObject(box, "Resize Box Collider Preview");
        Undo.RecordObject(transform, "Resize Box Collider Preview");
        Vector3 movedLocal3 = transform.InverseTransformPoint(movedWorld);
        Vector2 movedLocal = new Vector2(movedLocal3.x, movedLocal3.y);

        int oppositeIndex = (cornerIndex + 2) % 4;
        Vector2 oppositeLocal = originalCorners[oppositeIndex];
        Vector2 min = Vector2.Min(movedLocal, oppositeLocal);
        Vector2 max = Vector2.Max(movedLocal, oppositeLocal);

        Vector2 newOffset = (min + max) * 0.5f;
        Vector3 worldOffsetDelta = transform.TransformVector(new Vector3(newOffset.x - box.offset.x, newOffset.y - box.offset.y, 0f));
        transform.position += worldOffsetDelta;
        box.offset = Vector2.zero;
        box.size = new Vector2(Mathf.Max(0.05f, max.x - min.x), Mathf.Max(0.05f, max.y - min.y));
        EditorUtility.SetDirty(transform);
        EditorUtility.SetDirty(box);
    }
}
#endif
