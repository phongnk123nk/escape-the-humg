#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tao san UI ESC Pause Menu trong scene hien tai de chinh truc tiep bang Scene View/Inspector.
/// </summary>
public static class GlobalEscPauseMenuEditorSetup
{
    [MenuItem("Tools/Setup Editable ESC Pause Menu")]
    public static void SetupEditableEscPauseMenu()
    {
        GlobalEscPauseMenu existing = Object.FindObjectOfType<GlobalEscPauseMenu>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            Debug.Log("Da co GlobalEscPauseMenu trong scene. Chon object do de chinh UI.");
            return;
        }

        EnsureEventSystem();

        GameObject root = new GameObject("GlobalEscPauseMenu");
        Undo.RegisterCreatedObjectUndo(root, "Create Global ESC Pause Menu");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        GlobalEscPauseMenu pauseMenu = root.AddComponent<GlobalEscPauseMenu>();
        pauseMenu.mainMenuSceneName = "main menu";
        pauseMenu.scenesDisablePause = new[] { "main menu" };

        GameObject panel = CreatePanel("PauseMenuPanel", root.transform, new Color(0f, 0f, 0f, 0.68f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        GameObject card = CreatePanel("MenuCard", panel.transform, new Color(0.08f, 0.08f, 0.09f, 0.94f));
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(560f, 360f);
        cardRect.anchoredPosition = Vector2.zero;

        CreateText("TitleText", card.transform, "TẠM DỪNG", 48, Color.white, new Vector2(0f, 95f), new Vector2(500f, 80f));
        Button continueButton = CreateButton("ContinueButton", card.transform, "TIẾP TỤC", new Vector2(0f, 10f));
        Button exitButton = CreateButton("ExitToMenuButton", card.transform, "THOÁT", new Vector2(0f, -90f));

        pauseMenu.pauseMenuPanel = panel;
        pauseMenu.pauseCanvasGroup = canvasGroup;
        pauseMenu.continueButton = continueButton;
        pauseMenu.exitToMenuButton = exitButton;

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Da tao Editable ESC Pause Menu. Chinh cac object con trong Hierarchy, sau do Play va bam ESC de test.");
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<CanvasRenderer>();

        Image image = obj.AddComponent<Image>();
        image.color = color;

        return obj;
    }

    private static Text CreateText(string name, Transform parent, string text, int size, Color color, Vector2 position, Vector2 rectSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<CanvasRenderer>();

        Text label = obj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (label.font == null)
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = size;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = rectSize;
        rect.anchoredPosition = position;

        return label;
    }

    private static Button CreateButton(string name, Transform parent, string text, Vector2 position)
    {
        GameObject obj = CreatePanel(name, parent, new Color(0.55f, 0.05f, 0.05f, 1f));
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = obj.GetComponent<Image>();

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(340f, 72f);
        rect.anchoredPosition = position;

        CreateText("Text", obj.transform, text, 28, Color.white, Vector2.zero, rect.sizeDelta);
        return button;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
}
#endif
