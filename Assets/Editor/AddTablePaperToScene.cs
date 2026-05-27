using UnityEngine;
using UnityEditor;

public static class AddTablePaperToScene
{
    [MenuItem("Lab/Add Table Paper To Scene")]
    public static void Add()
    {
        // Try to find a sprite asset named "cong thuc" first
        string[] guids = AssetDatabase.FindAssets("cong thuc t:Sprite");
        Sprite sprite = null;

        if (guids == null || guids.Length == 0)
        {
            // fallback: find any sprite named "cong thuc"
            guids = AssetDatabase.FindAssets("cong thuc");
        }

        if (guids != null && guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // If not found, try any sprite under Assets/image
        if (sprite == null)
        {
            guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/image" });
            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }

        // Create GameObject
        GameObject go = new GameObject("TablePaper_Target");
        Undo.RegisterCreatedObjectUndo(go, "Add TablePaper_Target");

        // Try to parent under the TableView preview if it exists, otherwise place near main camera
        GameObject parent = GameObject.Find("PREVIEW 5 - TableView");
        if (parent == null)
        {
            parent = GameObject.Find("PREVIEW Background - TableView");
        }

        if (parent != null)
        {
            go.transform.SetParent(parent.transform, false);
            // place roughly centered on the parent
            RectTransform parentRect = parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                go.transform.localPosition = Vector3.zero;
            }
            else
            {
                go.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            Camera cam = Camera.main;
            Vector3 pos = Vector3.zero;
            if (cam != null)
            {
                pos = cam.transform.position + cam.transform.forward * 5f;
                pos.z = 0f;
            }
            go.transform.position = pos;
        }

        // SpriteRenderer
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Simple;
        }

        // Add BoxCollider2D for clicks
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        // size will match sprite if available
        if (sr.sprite != null)
        {
            box.size = sr.sprite.bounds.size;
        }

        // Add TablePaper component
        var tp = go.AddComponent<TablePaper>();
        tp.paperSprite = sprite;

        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        Debug.Log("TablePaper_Target created. Assignments: Sprite=" + (sprite != null ? sprite.name : "(none)"));
    }

    [MenuItem("Lab/Move TablePaper To TableView")]
    public static void MoveToTableView()
    {
        GameObject existing = GameObject.Find("TablePaper_Target");
        if (existing == null)
        {
            Debug.LogWarning("TablePaper_Target not found in scene.");
            return;
        }

        GameObject parent = GameObject.Find("PREVIEW 5 - TableView");
        if (parent == null)
        {
            parent = GameObject.Find("PREVIEW Background - TableView");
        }

        if (parent == null)
        {
            Debug.LogWarning("TableView parent not found. Make sure PREVIEW 5 - TableView or PREVIEW Background - TableView exists.");
            return;
        }

        Undo.SetTransformParent(existing.transform, parent.transform, "Reparent TablePaper_Target");
        existing.transform.localPosition = Vector3.zero;
        Debug.Log("TablePaper_Target moved under " + parent.name);
        Selection.activeGameObject = existing;
    }
}
