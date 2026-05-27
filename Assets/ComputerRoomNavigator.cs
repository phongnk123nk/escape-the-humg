using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public enum ComputerRoomView
{
    Frame44_MainDoorView,
    Frame45_MainComputerView,
    Frame46_BackToDoorView,
    Frame47_ComputerDeskView,
    Frame49_ComputerScreenView,
    Frame50_DoorCloseView
}

/// <summary>
/// Dieu huong cac man trong scene PhongTinHoc.
/// Script chi bat/tat object, khong destroy hotspot hay object ban da keo vao scene.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Computer Room Navigator")]
public class ComputerRoomNavigator : MonoBehaviour
{
    [Header("Scene Name")]
    public string expectedSceneName = "PhongTinHoc";

    [Header("Background")]
    public SpriteRenderer backgroundRenderer;
    public bool autoCreateBackgroundRenderer = true;
    public Camera targetCamera;

    [Header("Background Sprites")]
    public Sprite frame44MainDoor;
    public Sprite frame45MainComputer;
    public Sprite frame46BackToDoor;
    public Sprite frame47ComputerDesk;
    public Sprite frame49ComputerScreen;
    public Sprite frame50DoorClose;

    [Header("Hotspot Root")]
    public Transform hotspotsRoot;
    public Sprite defaultArrowSprite;

    [Header("Puzzle")]
    public GameObject computerPuzzlePanel;
    public bool computerPuzzleSolved = false;
    public bool openComputerPuzzlePanelOnFrame49 = false;

    [Header("Door Lock Puzzle")]
    public GameObject doorLockPuzzlePanel;
    public bool doorLockSolved = false;
    public int doorLockFirstDigit = 6;
    public int doorLockSecondDigit = 6;
    public bool loadSceneWhenDoorLockSolved = true;
    public string nextSceneAfterDoorLockSolved = "room1";

    private InputField doorLockFirstInput;
    private InputField doorLockSecondInput;
    private Text doorLockMessageText;
    private int doorLockActiveInputIndex;

    [Header("Transition")]
    public bool useTransitionAnimation = true;
    public float fadeOutDuration = 0.18f;
    public float fadeInDuration = 0.18f;
    public Color transitionColor = Color.black;
    public SpriteRenderer transitionFadeRenderer;

    [Header("Debug")]
    public bool showHotspotPreview = true;
    public bool isolateEditModePreview = true;
    public bool showAllViewsSeparatedInEditMode = true;
    public Vector2 editViewSpacing = new Vector2(11f, 6.5f);
    public float editViewGapPixels = 80f;
    public ComputerRoomView editPreviewView = ComputerRoomView.Frame44_MainDoorView;
    public ComputerRoomView startView = ComputerRoomView.Frame44_MainDoorView;

    private ComputerRoomView currentView;
    private bool isTransitioning;
    private Texture2D transitionTexture;
    private Sprite transitionSprite;

    private void Reset()
    {
        expectedSceneName = "PhongTinHoc";
        startView = ComputerRoomView.Frame44_MainDoorView;
        editPreviewView = ComputerRoomView.Frame44_MainDoorView;
        isolateEditModePreview = true;
        showAllViewsSeparatedInEditMode = true;
        showHotspotPreview = true;
        FindHotspotsRoot();
    }

    private void OnValidate()
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.sortingOrder = -100;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        FindHotspotsRoot();
        EnsureBackgroundRenderer();
        ApplyHotspotPreviewVisibility();
        ApplyEditModePreview();
    }

    private void Awake()
    {
        FindHotspotsRoot();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (!Application.isPlaying)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != expectedSceneName)
        {
            Debug.LogWarning("ComputerRoomNavigator dang chay trong scene '" + SceneManager.GetActiveScene().name + "', khong phai '" + expectedSceneName + "'.", this);
        }

        EnsurePuzzlePanel();
        EnsureTransitionFadeRenderer();
        ShowView(startView);
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            ShowView(startView);
        }
    }

    private void Update()
    {
        HandleDoorLockKeyboardInput();
    }

    /// <summary>
    /// Doi background va chi bat hotspot thuoc man hien tai.
    /// </summary>
    public void ShowView(ComputerRoomView view)
    {
        currentView = view;

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = GetBackgroundForView(view);
            backgroundRenderer.sortingOrder = -100;
            backgroundRenderer.gameObject.SetActive(!showAllViewsSeparatedInEditMode);
        }
        else
        {
            Debug.LogWarning("Chua gan backgroundRenderer cho ComputerRoomNavigator.", this);
        }

        ComputerRoomHotspot[] hotspots = GetHotspots();
        PrepareViewFoldersForPlay(view);
        for (int i = 0; i < hotspots.Length; i++)
        {
            ComputerRoomHotspot hotspot = hotspots[i];
            if (hotspot == null)
            {
                continue;
            }

            bool visible = hotspot.visibleOnView == view;

            // Trong man man hinh may tinh, hotspot can puzzle solved se chi hien sau khi giai xong.
            if (view == ComputerRoomView.Frame49_ComputerScreenView && !computerPuzzleSolved && !hotspot.canUseWhenPuzzleLocked)
            {
                visible = false;
            }

            if (hotspot.requiresPuzzleSolved && !computerPuzzleSolved)
            {
                visible = false;
            }

            hotspot.gameObject.SetActive(visible);
            hotspot.SetPreviewVisible(showHotspotPreview);
        }

        MoveCameraToView(view);

        if (view == ComputerRoomView.Frame49_ComputerScreenView && !computerPuzzleSolved && openComputerPuzzlePanelOnFrame49)
        {
            OpenComputerPuzzle();
        }
        else
        {
            CloseComputerPuzzle();
        }
    }

    /// <summary>
    /// Duoc ComputerRoomHotspot goi khi nguoi choi click vao vung click.
    /// </summary>
    public void HandleHotspotClick(ComputerRoomHotspot hotspot)
    {
        if (hotspot == null)
        {
            return;
        }

        if (isTransitioning)
        {
            return;
        }

        if (currentView == ComputerRoomView.Frame49_ComputerScreenView && !computerPuzzleSolved && !hotspot.canUseWhenPuzzleLocked)
        {
            Debug.Log("Phai giai puzzle may tinh truoc khi roi man hinh nay.");
            OpenComputerPuzzle();
            return;
        }

        if (hotspot.requiresPuzzleSolved && !computerPuzzleSolved)
        {
            if (hotspot.isExitDoor)
            {
                if (!doorLockSolved)
                {
                    Debug.Log("Cửa vẫn bị khóa. Bấm vào ổ khóa để nhập mã.");
                    OpenDoorLockPuzzle();
                    return;
                }
            }
            else
            {
                Debug.Log("Hotspot can giai puzzle truoc: " + hotspot.hotspotName);
                return;
            }
        }

        if (hotspot.startsComputerPuzzle)
        {
            GoToView(ComputerRoomView.Frame49_ComputerScreenView);
            return;
        }

        if (hotspot.isExitDoor)
        {
            if (!doorLockSolved)
            {
                Debug.Log("Cửa vẫn bị khóa. Bấm vào ổ khóa để nhập mã.");
                OpenDoorLockPuzzle();
                return;
            }

            Debug.Log("Exit computer room");
            return;
        }

        if (hotspot.isContinueAfterPuzzle)
        {
            Debug.Log("Continue after computer puzzle");
            return;
        }

        GoToView(hotspot.targetView);
    }

    /// <summary>
    /// Chuyen goc nhin co animation fade neu dang Play.
    /// </summary>
    public void GoToView(ComputerRoomView targetView)
    {
        if (!Application.isPlaying || !useTransitionAnimation)
        {
            ShowView(targetView);
            return;
        }

        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(TransitionToView(targetView));
    }

    private IEnumerator TransitionToView(ComputerRoomView targetView)
    {
        isTransitioning = true;
        SetAllHotspotsInteractable(false);
        EnsureTransitionFadeRenderer();

        yield return FadeTransition(0f, 1f, fadeOutDuration);
        ShowView(targetView);
        yield return FadeTransition(1f, 0f, fadeInDuration);

        SetAllHotspotsInteractable(true);
        isTransitioning = false;
    }

    public void OpenComputerPuzzle()
    {
        EnsurePuzzlePanel();

        if (computerPuzzlePanel != null)
        {
            computerPuzzlePanel.SetActive(true);
        }
    }

    public void CloseComputerPuzzle()
    {
        if (computerPuzzlePanel != null)
        {
            computerPuzzlePanel.SetActive(false);
        }
    }

    public void OpenDoorLockPuzzle()
    {
        EnsureDoorLockPuzzlePanel();
        EnsureEventSystem();

        if (doorLockPuzzlePanel != null)
        {
            doorLockPuzzlePanel.SetActive(true);
        }

        if (doorLockMessageText != null)
        {
            doorLockMessageText.text = doorLockSolved ? "Đã mở khóa" : "";
        }

        if (doorLockFirstInput != null && !doorLockSolved)
        {
            doorLockActiveInputIndex = 0;
            EventSystem.current.SetSelectedGameObject(doorLockFirstInput.gameObject);
            doorLockFirstInput.ActivateInputField();
        }
    }

    public void CloseDoorLockPuzzle()
    {
        if (doorLockPuzzlePanel != null)
        {
            doorLockPuzzlePanel.SetActive(false);
        }
    }

    public void SubmitDoorLockPuzzle()
    {
        int firstDigit = -1;
        int secondDigit = -1;

        bool firstOk = doorLockFirstInput != null && int.TryParse(doorLockFirstInput.text, out firstDigit);
        bool secondOk = doorLockSecondInput != null && int.TryParse(doorLockSecondInput.text, out secondDigit);

        if (firstOk && secondOk && firstDigit == doorLockFirstDigit && secondDigit == doorLockSecondDigit)
        {
            doorLockSolved = true;
            if (doorLockMessageText != null)
            {
                doorLockMessageText.text = "Đúng mã. Cửa đã mở.";
            }

            Debug.Log("Door lock solved!");
            CloseDoorLockPuzzle();
            LoadSceneAfterDoorLockSolved();
            return;
        }

        if (doorLockMessageText != null)
        {
            doorLockMessageText.text = "Sai mã.";
        }

        Debug.Log("Wrong door lock code.");
    }

    private void LoadSceneAfterDoorLockSolved()
    {
        if (!loadSceneWhenDoorLockSolved)
        {
            ShowView(currentView);
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneAfterDoorLockSolved))
        {
            Debug.LogWarning("Chưa điền tên scene tiếp theo trong nextSceneAfterDoorLockSolved.", this);
            ShowView(currentView);
            return;
        }

        SceneManager.LoadScene(nextSceneAfterDoorLockSolved);
    }

    public void OnComputerPuzzleSolved()
    {
        computerPuzzleSolved = true;
        CloseComputerPuzzle();
        Debug.Log("Computer puzzle solved!");
        ShowView(currentView);
    }

    private IEnumerator FadeTransition(float fromAlpha, float toAlpha, float duration)
    {
        if (transitionFadeRenderer == null)
        {
            yield break;
        }

        duration = Mathf.Max(0.01f, duration);
        float time = 0f;
        transitionFadeRenderer.gameObject.SetActive(true);

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            SetTransitionAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
            yield return null;
        }

        SetTransitionAlpha(toAlpha);

        if (Mathf.Approximately(toAlpha, 0f))
        {
            transitionFadeRenderer.gameObject.SetActive(false);
        }
    }

    private void SetTransitionAlpha(float alpha)
    {
        if (transitionFadeRenderer == null)
        {
            return;
        }

        Color color = transitionColor;
        color.a = alpha;
        transitionFadeRenderer.color = color;
        PositionTransitionFadeRenderer();
    }

    private Sprite GetBackgroundForView(ComputerRoomView view)
    {
        switch (view)
        {
            case ComputerRoomView.Frame44_MainDoorView:
                return frame44MainDoor;
            case ComputerRoomView.Frame45_MainComputerView:
                return frame45MainComputer;
            case ComputerRoomView.Frame46_BackToDoorView:
                return frame46BackToDoor;
            case ComputerRoomView.Frame47_ComputerDeskView:
                return frame47ComputerDesk;
            case ComputerRoomView.Frame49_ComputerScreenView:
                return frame49ComputerScreen;
            case ComputerRoomView.Frame50_DoorCloseView:
                return frame50DoorClose;
            default:
                return null;
        }
    }

    private ComputerRoomHotspot[] GetHotspots()
    {
        if (hotspotsRoot != null)
        {
            return hotspotsRoot.GetComponentsInChildren<ComputerRoomHotspot>(true);
        }

        return GetComponentsInChildren<ComputerRoomHotspot>(true);
    }

    private void FindHotspotsRoot()
    {
        if (hotspotsRoot != null)
        {
            return;
        }

        Transform found = transform.Find("Hotspots");
        if (found != null)
        {
            hotspotsRoot = found;
        }
    }

    private void ApplyHotspotPreviewVisibility()
    {
        ComputerRoomHotspot[] hotspots = GetComponentsInChildren<ComputerRoomHotspot>(true);
        for (int i = 0; i < hotspots.Length; i++)
        {
            if (hotspots[i] != null)
            {
                hotspots[i].SetPreviewVisible(showHotspotPreview);
            }
        }
    }

    private void ApplyEditModePreview()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = GetBackgroundForView(editPreviewView);
            backgroundRenderer.sortingOrder = -100;
            backgroundRenderer.gameObject.SetActive(!showAllViewsSeparatedInEditMode);
        }

        if (showAllViewsSeparatedInEditMode)
        {
#if UNITY_EDITOR
            ArrangeAllViewsForEditing(false);
#endif
            return;
        }

        if (!isolateEditModePreview)
        {
            return;
        }

        ComputerRoomHotspot[] hotspots = GetHotspots();
        for (int i = 0; i < hotspots.Length; i++)
        {
            ComputerRoomHotspot hotspot = hotspots[i];
            if (hotspot == null)
            {
                continue;
            }

            hotspot.gameObject.SetActive(hotspot.visibleOnView == editPreviewView);
        }
    }

    private void EnsurePuzzlePanel()
    {
        EnsureEventSystem();

        if (computerPuzzlePanel != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("ComputerPuzzleCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panel = new GameObject("ComputerPuzzlePanel");
        panel.transform.SetParent(canvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(360f, 160f);

        GameObject textObject = new GameObject("Title");
        textObject.transform.SetParent(panel.transform, false);
        Text title = textObject.AddComponent<Text>();
        title.text = "Computer Puzzle";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        title.fontSize = 24;
        title.font = GetBuiltinFont();
        RectTransform titleRect = textObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.58f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        GameObject buttonObject = new GameObject("Solve Puzzle Debug");
        buttonObject.transform.SetParent(panel.transform, false);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.7f, 1f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(OnComputerPuzzleSolved);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.2f, 0.15f);
        buttonRect.anchorMax = new Vector2(0.8f, 0.45f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        GameObject buttonTextObject = new GameObject("Text");
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.text = "Solve Puzzle Debug";
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.black;
        buttonText.fontSize = 18;
        buttonText.font = GetBuiltinFont();
        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        computerPuzzlePanel = panel;
        computerPuzzlePanel.SetActive(false);
    }

    private void EnsureDoorLockPuzzlePanel()
    {
        EnsureEventSystem();

        if (doorLockPuzzlePanel != null)
        {
            CacheDoorLockInputs();
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("ComputerPuzzleCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panel = new GameObject("DoorLockPuzzlePanel");
        panel.transform.SetParent(canvas.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.78f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(320f, 190f);

        Text title = CreateDoorLockText(panel.transform, "Title", "Nhập mã ổ khóa", 22, Color.white);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.08f, 0.72f);
        titleRect.anchorMax = new Vector2(0.92f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        doorLockFirstInput = CreateDoorLockInput(panel.transform, "Digit 1", new Vector2(0.25f, 0.48f), new Vector2(0.45f, 0.68f));
        doorLockSecondInput = CreateDoorLockInput(panel.transform, "Digit 2", new Vector2(0.55f, 0.48f), new Vector2(0.75f, 0.68f));

        Button submitButton = CreateDoorLockButton(panel.transform, "Submit", "OK", new Vector2(0.18f, 0.16f), new Vector2(0.48f, 0.36f));
        submitButton.onClick.AddListener(SubmitDoorLockPuzzle);

        Button closeButton = CreateDoorLockButton(panel.transform, "Close", "Đóng", new Vector2(0.52f, 0.16f), new Vector2(0.82f, 0.36f));
        closeButton.onClick.AddListener(CloseDoorLockPuzzle);

        doorLockMessageText = CreateDoorLockText(panel.transform, "Message", "", 16, new Color(1f, 0.85f, 0.2f, 1f));
        RectTransform messageRect = doorLockMessageText.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.08f, 0.02f);
        messageRect.anchorMax = new Vector2(0.92f, 0.14f);
        messageRect.offsetMin = Vector2.zero;
        messageRect.offsetMax = Vector2.zero;

        doorLockPuzzlePanel = panel;
        doorLockPuzzlePanel.SetActive(false);
    }

    private void CacheDoorLockInputs()
    {
        if (doorLockPuzzlePanel == null)
        {
            return;
        }

        InputField[] inputs = doorLockPuzzlePanel.GetComponentsInChildren<InputField>(true);
        if (inputs.Length > 0)
        {
            doorLockFirstInput = inputs[0];
        }

        if (inputs.Length > 1)
        {
            doorLockSecondInput = inputs[1];
        }
    }

    private Text CreateDoorLockText(Transform parent, string name, string value, int fontSize, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.fontSize = fontSize;
        text.font = GetBuiltinFont();

        return text;
    }

    private InputField CreateDoorLockInput(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject inputObject = new GameObject(name);
        inputObject.transform.SetParent(parent, false);

        Image image = inputObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.04f, 0.02f, 0.95f);

        InputField inputField = inputObject.AddComponent<InputField>();
        inputField.characterLimit = 1;
        inputField.contentType = InputField.ContentType.IntegerNumber;
        inputField.interactable = true;
        inputField.transition = Selectable.Transition.ColorTint;

        RectTransform rect = inputObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text text = CreateDoorLockText(inputObject.transform, "Text", "", 32, new Color(1f, 0.2f, 0.05f, 1f));
        inputField.textComponent = text;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text placeholder = CreateDoorLockText(inputObject.transform, "Placeholder", "-", 28, new Color(1f, 1f, 1f, 0.35f));
        inputField.placeholder = placeholder;
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        EventTrigger eventTrigger = inputObject.AddComponent<EventTrigger>();
        EventTrigger.Entry selectEntry = new EventTrigger.Entry();
        selectEntry.eventID = EventTriggerType.Select;
        int inputIndex = name.Contains("1") ? 0 : 1;
        selectEntry.callback.AddListener(delegate { doorLockActiveInputIndex = inputIndex; });
        eventTrigger.triggers.Add(selectEntry);

        return inputField;
    }

    private Button CreateDoorLockButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.28f, 0.16f, 0.08f, 1f);

        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text text = CreateDoorLockText(buttonObject.transform, "Text", label, 18, Color.white);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            InputSystemUIInputModule inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.enabled = true;
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            StandaloneInputModule inputModule = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            inputModule.enabled = true;
        }
#endif
    }

    private void HandleDoorLockKeyboardInput()
    {
        if (!Application.isPlaying || doorLockPuzzlePanel == null || !doorLockPuzzlePanel.activeInHierarchy || doorLockSolved)
        {
            return;
        }

        int digit = ReadPressedDigit();
        if (digit >= 0)
        {
            SetDoorLockDigit(digit);
            return;
        }

        if (ReadSubmitPressed())
        {
            SubmitDoorLockPuzzle();
            return;
        }

        if (ReadBackspacePressed())
        {
            ClearDoorLockDigit();
        }
    }

    private void SetDoorLockDigit(int digit)
    {
        InputField target = doorLockActiveInputIndex == 0 ? doorLockFirstInput : doorLockSecondInput;
        if (target == null)
        {
            return;
        }

        target.text = digit.ToString();

        if (doorLockActiveInputIndex == 0)
        {
            doorLockActiveInputIndex = 1;
            if (doorLockSecondInput != null)
            {
                EventSystem.current.SetSelectedGameObject(doorLockSecondInput.gameObject);
                doorLockSecondInput.ActivateInputField();
            }
        }
    }

    private void ClearDoorLockDigit()
    {
        InputField target = doorLockActiveInputIndex == 0 ? doorLockFirstInput : doorLockSecondInput;
        if (target != null)
        {
            target.text = "";
        }

        if (doorLockActiveInputIndex == 1 && doorLockSecondInput != null && string.IsNullOrEmpty(doorLockSecondInput.text))
        {
            doorLockActiveInputIndex = 0;
            if (doorLockFirstInput != null)
            {
                EventSystem.current.SetSelectedGameObject(doorLockFirstInput.gameObject);
                doorLockFirstInput.ActivateInputField();
            }
        }
    }

    private int ReadPressedDigit()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame) return 0;
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) return 1;
            if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) return 2;
            if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) return 3;
            if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) return 4;
            if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame) return 5;
            if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame) return 6;
            if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame) return 7;
            if (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame) return 8;
            if (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame) return 9;
        }
#else
        string input = Input.inputString;
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsDigit(input[i]))
            {
                return input[i] - '0';
            }
        }
#endif
        return -1;
    }

    private bool ReadSubmitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
    }

    private bool ReadBackspacePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Backspace);
#endif
    }

    private Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    private void EnsureBackgroundRenderer()
    {
        if (backgroundRenderer != null || !autoCreateBackgroundRenderer)
        {
            return;
        }

        Transform found = transform.Find("Background");
        if (found != null)
        {
            backgroundRenderer = found.GetComponent<SpriteRenderer>();
        }
    }

    private void EnsureTransitionFadeRenderer()
    {
        if (transitionFadeRenderer != null)
        {
            PositionTransitionFadeRenderer();
            return;
        }

        Transform found = transform.Find("Transition Fade");
        if (found == null)
        {
            GameObject fadeObject = new GameObject("Transition Fade");
            fadeObject.transform.SetParent(transform, false);
            found = fadeObject.transform;
        }

        transitionFadeRenderer = found.GetComponent<SpriteRenderer>();
        if (transitionFadeRenderer == null)
        {
            transitionFadeRenderer = found.gameObject.AddComponent<SpriteRenderer>();
        }

        if (transitionTexture == null)
        {
            transitionTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            transitionTexture.SetPixel(0, 0, Color.white);
            transitionTexture.Apply();
        }

        if (transitionSprite == null)
        {
            transitionSprite = Sprite.Create(transitionTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        transitionFadeRenderer.sprite = transitionSprite;
        transitionFadeRenderer.sortingOrder = 5000;
        SetTransitionAlpha(0f);
        transitionFadeRenderer.gameObject.SetActive(false);
    }

    private void PositionTransitionFadeRenderer()
    {
        if (transitionFadeRenderer == null)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null || !targetCamera.orthographic)
        {
            transitionFadeRenderer.transform.localPosition = new Vector3(0f, 0f, -1f);
            transitionFadeRenderer.transform.localScale = new Vector3(100f, 100f, 1f);
            return;
        }

        float height = targetCamera.orthographicSize * 2f;
        float width = height * targetCamera.aspect;
        Vector3 position = targetCamera.transform.position;
        position.z = 0f;

        transitionFadeRenderer.transform.position = position;
        transitionFadeRenderer.transform.rotation = Quaternion.identity;
        transitionFadeRenderer.transform.localScale = new Vector3(width + 1f, height + 1f, 1f);
    }

    private void SetAllHotspotsInteractable(bool interactable)
    {
        ComputerRoomHotspot[] hotspots = GetHotspots();
        for (int i = 0; i < hotspots.Length; i++)
        {
            if (hotspots[i] != null && hotspots[i].boxCollider != null)
            {
                hotspots[i].boxCollider.enabled = interactable;
            }
        }
    }

    private void PrepareViewFoldersForPlay(ComputerRoomView activeView)
    {
        if (hotspotsRoot == null)
        {
            return;
        }

        ComputerRoomView[] views = GetAllViews();
        for (int i = 0; i < views.Length; i++)
        {
            Transform folder = hotspotsRoot.Find(views[i].ToString());
            if (folder == null)
            {
                continue;
            }

            folder.gameObject.SetActive(folder.name == activeView.ToString());

            Transform previewBackground = folder.Find("PreviewBackground");
            if (previewBackground != null)
            {
                previewBackground.gameObject.SetActive(showAllViewsSeparatedInEditMode);
            }
        }
    }

    private void MoveCameraToView(ComputerRoomView view)
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null || hotspotsRoot == null)
        {
            return;
        }

        Transform folder = hotspotsRoot.Find(view.ToString());
        if (folder == null)
        {
            return;
        }

        Vector3 cameraPosition = folder.position;
        cameraPosition.z = targetCamera.transform.position.z;
        targetCamera.transform.position = cameraPosition;
    }

    private ComputerRoomView[] GetAllViews()
    {
        return new ComputerRoomView[]
        {
            ComputerRoomView.Frame44_MainDoorView,
            ComputerRoomView.Frame45_MainComputerView,
            ComputerRoomView.Frame46_BackToDoorView,
            ComputerRoomView.Frame47_ComputerDeskView,
            ComputerRoomView.Frame49_ComputerScreenView,
            ComputerRoomView.Frame50_DoorCloseView
        };
    }

#if UNITY_EDITOR
    [ContextMenu("Create Background Renderer")]
    public void CreateBackgroundRenderer()
    {
        Transform found = transform.Find("Background");
        if (found == null)
        {
            GameObject backgroundObject = new GameObject("Background");
            Undo.RegisterCreatedObjectUndo(backgroundObject, "Create Computer Room Background");
            backgroundObject.transform.SetParent(transform, false);
            found = backgroundObject.transform;
        }

        SpriteRenderer renderer = found.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(found.gameObject);
        }

        renderer.sortingOrder = -100;
        backgroundRenderer = renderer;

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(renderer);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    [ContextMenu("Setup Default Computer Room Hotspots")]
    public void SetupDefaultComputerRoomHotspots()
    {
        if (gameObject.scene.IsValid() && gameObject.scene.name != expectedSceneName)
        {
            Debug.LogWarning("Dang setup trong scene '" + gameObject.scene.name + "', khong phai '" + expectedSceneName + "'. Vui long mo scene PhongTinHoc neu day khong dung.", this);
        }

        Undo.RecordObject(this, "Setup Computer Room Hotspots");
        CreateBackgroundRenderer();
        EnsureHotspotRootEditor();
        EnsureViewFolder(ComputerRoomView.Frame44_MainDoorView);
        EnsureViewFolder(ComputerRoomView.Frame45_MainComputerView);
        EnsureViewFolder(ComputerRoomView.Frame46_BackToDoorView);
        EnsureViewFolder(ComputerRoomView.Frame47_ComputerDeskView);
        EnsureViewFolder(ComputerRoomView.Frame49_ComputerScreenView);
        EnsureViewFolder(ComputerRoomView.Frame50_DoorCloseView);

        CreateHotspotIfMissing("GoToMainComputer", ComputerRoomView.Frame44_MainDoorView, ComputerRoomView.Frame45_MainComputerView, new Vector3(0f, -3f, 0f), new Vector2(2f, 1f), false, false, false, false);
        CreateHotspotIfMissing("GoToDoorCloseFromMainDoor", ComputerRoomView.Frame44_MainDoorView, ComputerRoomView.Frame50_DoorCloseView, new Vector3(0f, 2.5f, 0f), new Vector2(2f, 1f), false, false, false, false);

        CreateHotspotIfMissing("GoToComputerDesk", ComputerRoomView.Frame45_MainComputerView, ComputerRoomView.Frame47_ComputerDeskView, new Vector3(0f, -2.8f, 0f), new Vector2(2f, 1f), false, false, false, false);
        DisableHotspotIfExists(ComputerRoomView.Frame45_MainComputerView, "TurnBackToDoor");

        CreateHotspotIfMissing("GoToDoorCloseView", ComputerRoomView.Frame46_BackToDoorView, ComputerRoomView.Frame50_DoorCloseView, new Vector3(0f, -2.8f, 0f), new Vector2(2f, 1f), false, false, false, false);
        DisableHotspotIfExists(ComputerRoomView.Frame46_BackToDoorView, "BackToComputer");
        DisableHotspotIfExists(ComputerRoomView.Frame46_BackToDoorView, "GoToDoor");

        CreateHotspotIfMissing("ClickComputerScreen", ComputerRoomView.Frame47_ComputerDeskView, ComputerRoomView.Frame49_ComputerScreenView, new Vector3(0f, 0.5f, 0f), new Vector2(4f, 3f), true, false, false, false);
        CreateHotspotIfMissing("TurnBackToDoorView", ComputerRoomView.Frame47_ComputerDeskView, ComputerRoomView.Frame46_BackToDoorView, new Vector3(0f, -2.8f, 0f), new Vector2(2f, 1f), false, false, false, false);
        DisableHotspotIfExists(ComputerRoomView.Frame47_ComputerDeskView, "BackToMainComputer");

        CreateHotspotIfMissing("ContinueAfterComputerPuzzle", ComputerRoomView.Frame49_ComputerScreenView, ComputerRoomView.Frame49_ComputerScreenView, new Vector3(0f, -2.8f, 0f), new Vector2(2f, 1f), false, true, false, true);
        DisableHotspotIfExists(ComputerRoomView.Frame49_ComputerScreenView, "BackToDeskAfterSolved");

        CreateHotspotIfMissing("ExitRoom", ComputerRoomView.Frame50_DoorCloseView, ComputerRoomView.Frame50_DoorCloseView, new Vector3(0f, -2.7f, 0f), new Vector2(2f, 1f), false, true, true, false);
        CreateHotspotIfMissing("BackToDoorView", ComputerRoomView.Frame50_DoorCloseView, ComputerRoomView.Frame44_MainDoorView, new Vector3(0f, 2.5f, 0f), new Vector2(2f, 1f), false, false, false, false);

        ApplyHotspotPreviewVisibility();
        ArrangeAllViewsForEditing(true);
        ApplyEditModePreview();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    [ContextMenu("Add Only Frame44 Frame50 Door Hotspots")]
    public void AddOnlyFrame44Frame50DoorHotspots()
    {
        RestoreDisabledHotspotIfExists(ComputerRoomView.Frame50_DoorCloseView, "BackToDoorView");
        CreateHotspotIfMissing("BackToDoorView", ComputerRoomView.Frame50_DoorCloseView, ComputerRoomView.Frame44_MainDoorView, new Vector3(0f, 2.5f, 0f), new Vector2(2f, 1f), false, false, false, false);
        CreateHotspotIfMissing("GoToDoorCloseFromMainDoor", ComputerRoomView.Frame44_MainDoorView, ComputerRoomView.Frame50_DoorCloseView, new Vector3(0f, 2.5f, 0f), new Vector2(2f, 1f), false, false, false, false);
        ApplyHotspotPreviewVisibility();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    public void AddOnlyDoorLockPuzzle(Sprite lockSprite)
    {
        Transform folder = EnsureViewFolder(ComputerRoomView.Frame50_DoorCloseView);
        Transform existing = folder.Find("DoorLockPuzzle");
        GameObject lockObject;

        if (existing != null)
        {
            lockObject = existing.gameObject;
        }
        else
        {
            lockObject = new GameObject("DoorLockPuzzle");
            Undo.RegisterCreatedObjectUndo(lockObject, "Create Door Lock Puzzle");
            lockObject.transform.SetParent(folder, false);
            lockObject.transform.localPosition = new Vector3(0f, 0.55f, -0.1f);
            lockObject.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        }

        SpriteRenderer renderer = lockObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(lockObject);
        }

        if (lockSprite != null)
        {
            renderer.sprite = lockSprite;
        }

        renderer.sortingOrder = 90;

        BoxCollider2D collider = lockObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(lockObject);
        }

        collider.isTrigger = true;
        if (collider.size == Vector2.one && renderer.sprite != null)
        {
            collider.size = renderer.sprite.bounds.size;
        }

        ComputerRoomDoorLockPuzzleTrigger trigger = lockObject.GetComponent<ComputerRoomDoorLockPuzzleTrigger>();
        if (trigger == null)
        {
            trigger = Undo.AddComponent<ComputerRoomDoorLockPuzzleTrigger>(lockObject);
        }

        trigger.navigator = this;
        lockObject.SetActive(true);

        EditorUtility.SetDirty(lockObject);
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    public void AddOnlyFrame49MiniGameIcons(Sprite horseIcon, Sprite foodIcon)
    {
        Transform folder = EnsureViewFolder(ComputerRoomView.Frame49_ComputerScreenView);
        CreateMiniGameIconIfMissing(folder, "MiniGameIcon_Horse", horseIcon, new Vector3(-2.15f, 1.15f, -0.1f));
        CreateMiniGameIconIfMissing(folder, "MiniGameIcon_Food", foodIcon, new Vector3(-1.25f, 1.15f, -0.1f));

        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    public void AddOnlyChessKnightMiniGame(GameObject tilePrefab, GameObject highlightDotPrefab, Sprite knightSprite, Sprite grassSprite, Sprite boardBackgroundSprite)
    {
        Transform folder = EnsureViewFolder(ComputerRoomView.Frame49_ComputerScreenView);
        Transform existing = folder.Find("ChessKnightMiniGame");
        GameObject root;

        if (existing != null)
        {
            root = existing.gameObject;
        }
        else
        {
            root = new GameObject("ChessKnightMiniGame");
            Undo.RegisterCreatedObjectUndo(root, "Create Chess Knight Mini Game");
            root.transform.SetParent(folder, false);
        }

        root.transform.localPosition = new Vector3(0.15f, 0.2f, -0.2f);
        root.transform.localScale = Vector3.one;

        GameObject boardContainer = EnsureChild(root.transform, "BoardContainer", Vector3.zero);
        GameObject boardObject = EnsureChild(root.transform, "BoardManager", Vector3.zero);
        GameObject knightObject = EnsureChild(root.transform, "KnightPiece", Vector3.zero);

        BoardManager boardManager = boardObject.GetComponent<BoardManager>();
        if (boardManager == null)
        {
            boardManager = Undo.AddComponent<BoardManager>(boardObject);
        }

        boardManager.width = 3;
        boardManager.height = 3;
        boardManager.tileSize = 1.2f;
        boardManager.spacing = 0f;
        boardManager.tilePrefab = tilePrefab;
        boardManager.highlightDotPrefab = highlightDotPrefab;
        boardManager.goalSprite = grassSprite;
        boardManager.goalScale = 0.7f;
        boardManager.customBoardSprite = boardBackgroundSprite;
        boardManager.hideTileColorsToSeeBackground = true;
        boardManager.boardContainer = boardContainer.transform;
        boardManager.centerCameraOnBoard = false;
        boardManager.showDebugGrid = false;
        boardManager.sortingOrderBase = 2000;

        Knight knight = knightObject.GetComponent<Knight>();
        if (knight == null)
        {
            knight = Undo.AddComponent<Knight>(knightObject);
        }

        SpriteRenderer knightRenderer = knightObject.GetComponent<SpriteRenderer>();
        if (knightRenderer == null)
        {
            knightRenderer = Undo.AddComponent<SpriteRenderer>(knightObject);
        }

        knightRenderer.sprite = knightSprite;
        knightRenderer.sortingOrder = boardManager.sortingOrderBase + 40;

        if (knightSprite != null)
        {
            float targetHeight = boardManager.tileSize * 0.8f;
            float spriteHeight = knightSprite.bounds.size.y;
            if (spriteHeight > 0f)
            {
                float scale = targetHeight / spriteHeight;
                knightObject.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        GameManager gameManager = root.GetComponent<GameManager>();
        if (gameManager == null)
        {
            gameManager = Undo.AddComponent<GameManager>(root);
        }

        gameManager.boardManager = boardManager;
        gameManager.knight = knight;
        gameManager.computerRoomNavigator = this;
        gameManager.goalsNeededToWin = 3;
        gameManager.randomGoalPlacement = true;
        gameManager.knightStartPos = new Vector2Int(1, 0);
        gameManager.goalPosition = new Vector2Int(0, 2);
        gameManager.currentTheme = null;
        BuildChessKnightEditorPreview(boardContainer.transform, boardManager.tileSize, knightSprite, grassSprite, boardBackgroundSprite, tilePrefab);

        ChessKnightMiniGamePreviewControls previewControls = root.GetComponent<ChessKnightMiniGamePreviewControls>();
        if (previewControls == null)
        {
            previewControls = Undo.AddComponent<ChessKnightMiniGamePreviewControls>(root);
        }

        previewControls.boardContainer = boardContainer.transform;
        previewControls.previewKnight = boardContainer.transform.Find("PreviewKnight_b1");
        previewControls.previewGrass = boardContainer.transform.Find("PreviewGrassGoal_a3");
        previewControls.runtimeKnight = knightObject.transform;
        previewControls.boardManager = boardManager;
        previewControls.sortingOrderBase = boardManager.sortingOrderBase;
        previewControls.boardScale = 1f;
        previewControls.knightScale = previewControls.previewKnight != null ? previewControls.previewKnight.localScale.x : 1f;
        previewControls.grassScale = previewControls.previewGrass != null ? previewControls.previewGrass.localScale.x : 1f;
        previewControls.Apply();

        ComputerRoomMiniGameStartHidden startHidden = root.GetComponent<ComputerRoomMiniGameStartHidden>();
        if (startHidden == null)
        {
            startHidden = Undo.AddComponent<ComputerRoomMiniGameStartHidden>(root);
        }

        startHidden.hideOnPlay = true;

        Transform horseIconTransform = folder.Find("MiniGameIcon_Horse");
        if (horseIconTransform != null)
        {
            ComputerRoomMiniGameIcon horseIcon = horseIconTransform.GetComponent<ComputerRoomMiniGameIcon>();
            if (horseIcon != null)
            {
                horseIcon.miniGamePanelToOpen = root;
                horseIcon.sceneNameToLoad = "";
                horseIcon.resultNumberSortingOrder = boardManager.sortingOrderBase + 100;
                gameManager.iconToReplaceWithNumber = horseIcon;
                gameManager.firstDoorCodeNumber = string.IsNullOrWhiteSpace(gameManager.firstDoorCodeNumber) ? horseIcon.resultNumber : gameManager.firstDoorCodeNumber;
                EditorUtility.SetDirty(horseIcon);
            }
        }

        root.SetActive(true);
        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    private void BuildChessKnightEditorPreview(Transform boardContainer, float tileSize, Sprite knightSprite, Sprite grassSprite, Sprite boardBackgroundSprite, GameObject tilePrefab)
    {
        for (int i = boardContainer.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(boardContainer.GetChild(i).gameObject);
        }

        Color light = new Color(0.92f, 0.93f, 0.78f, 1f);
        Color dark = new Color(0.58f, 0.69f, 0.48f, 1f);
        float offset = tileSize;

        GameObject boardBackground = new GameObject("PreviewBoardBackground_Anh3");
        Undo.RegisterCreatedObjectUndo(boardBackground, "Create Chess Board Background");
        boardBackground.transform.SetParent(boardContainer, false);
        boardBackground.transform.localPosition = new Vector3(0f, 0f, 0.04f);

        SpriteRenderer boardBackgroundRenderer = boardBackground.AddComponent<SpriteRenderer>();
        boardBackgroundRenderer.sprite = boardBackgroundSprite;
        const int chessPreviewSortingBase = 2000;
        boardBackgroundRenderer.sortingOrder = chessPreviewSortingBase;

        if (boardBackgroundSprite != null)
        {
            Vector2 spriteSize = boardBackgroundSprite.bounds.size;
            float targetWidth = tileSize * 3f;
            float targetHeight = tileSize * 3f;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                boardBackground.transform.localScale = new Vector3(targetWidth / spriteSize.x, targetHeight / spriteSize.y, 1f);
            }
        }

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                GameObject tile;
                if (tilePrefab != null)
                {
                    tile = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, boardContainer);
                    Undo.RegisterCreatedObjectUndo(tile, "Create Chess Preview Tile");
                }
                else
                {
                    tile = new GameObject("PreviewTile_" + x + "_" + y);
                    Undo.RegisterCreatedObjectUndo(tile, "Create Chess Preview Tile");
                    tile.transform.SetParent(boardContainer, false);
                    tile.AddComponent<SpriteRenderer>();
                    tile.AddComponent<BoxCollider2D>();
                    tile.AddComponent<Tile>();
                }

                tile.name = "PreviewTile_" + x + "_" + y;
                tile.transform.localPosition = new Vector3((x - 1) * offset, (y - 1) * offset, 0f);

                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = tile.AddComponent<SpriteRenderer>();
                }

                if (boardBackgroundSprite != null)
                {
                    FitPreviewTileMarker(tile, renderer);
                }
                else
                {
                    FitPreviewTileVisual(tile, renderer, tileSize);
                }

                Color tileColor = ((x + y) % 2 == 0) ? dark : light;
                tileColor.a = boardBackgroundSprite != null ? 0.35f : 1f;
                renderer.color = tileColor;
                renderer.sortingOrder = chessPreviewSortingBase + 10;
            }
        }

        CreateChessPreviewLabel(boardContainer, "a", new Vector3(-tileSize, -1.68f * tileSize, 0f));
        CreateChessPreviewLabel(boardContainer, "b", new Vector3(0f, -1.68f * tileSize, 0f));
        CreateChessPreviewLabel(boardContainer, "c", new Vector3(tileSize, -1.68f * tileSize, 0f));
        CreateChessPreviewLabel(boardContainer, "1", new Vector3(-1.68f * tileSize, -tileSize, 0f));
        CreateChessPreviewLabel(boardContainer, "2", new Vector3(-1.68f * tileSize, 0f, 0f));
        CreateChessPreviewLabel(boardContainer, "3", new Vector3(-1.68f * tileSize, tileSize, 0f));

        GameObject knightPreview = new GameObject("PreviewKnight_b1");
        Undo.RegisterCreatedObjectUndo(knightPreview, "Create Chess Preview Knight");
        knightPreview.transform.SetParent(boardContainer, false);
        knightPreview.transform.localPosition = new Vector3(0f, -tileSize, -0.02f);

        SpriteRenderer knightRenderer = knightPreview.AddComponent<SpriteRenderer>();
        knightRenderer.sprite = knightSprite;
        knightRenderer.sortingOrder = chessPreviewSortingBase + 40;

        if (knightSprite != null && knightSprite.bounds.size.y > 0f)
        {
            float scale = (tileSize * 0.75f) / knightSprite.bounds.size.y;
            knightPreview.transform.localScale = new Vector3(scale, scale, 1f);
        }

        GameObject grassPreview = new GameObject("PreviewGrassGoal_a3");
        Undo.RegisterCreatedObjectUndo(grassPreview, "Create Chess Preview Grass");
        grassPreview.transform.SetParent(boardContainer, false);
        grassPreview.transform.localPosition = new Vector3(-tileSize, tileSize, -0.03f);

        SpriteRenderer grassRenderer = grassPreview.AddComponent<SpriteRenderer>();
        grassRenderer.sprite = grassSprite;
        grassRenderer.sortingOrder = chessPreviewSortingBase + 35;

        if (grassSprite != null && grassSprite.bounds.size.y > 0f)
        {
            float scale = (tileSize * 0.62f) / Mathf.Max(grassSprite.bounds.size.x, grassSprite.bounds.size.y);
            grassPreview.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void FitPreviewTileVisual(GameObject tile, SpriteRenderer renderer, float tileSize)
    {
        if (tile == null || renderer == null || renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = renderer.sprite.bounds.size;
        float spriteMaxSize = Mathf.Max(spriteSize.x, spriteSize.y);
        if (spriteMaxSize <= 0f)
        {
            return;
        }

        float scale = tileSize / spriteMaxSize;
        tile.transform.localScale = new Vector3(scale, scale, 1f);

        BoxCollider2D collider = tile.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = renderer.sprite.bounds.size;
            collider.offset = Vector2.zero;
        }
    }

    private void FitPreviewTileMarker(GameObject tile, SpriteRenderer renderer)
    {
        if (tile == null || renderer == null)
        {
            return;
        }

        tile.transform.localScale = Vector3.one;

        BoxCollider2D collider = tile.GetComponent<BoxCollider2D>();
        if (collider != null && renderer.sprite != null)
        {
            collider.size = renderer.sprite.bounds.size;
            collider.offset = Vector2.zero;
        }
    }

    private void CreateChessPreviewLabel(Transform parent, string label, Vector3 localPosition)
    {
        GameObject labelObject = new GameObject("PreviewLabel_" + label);
        Undo.RegisterCreatedObjectUndo(labelObject, "Create Chess Preview Label");
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = localPosition;

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.28f;
        textMesh.fontSize = 48;
        textMesh.color = Color.white;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 2045;
        }
    }

    private GameObject EnsureChild(Transform parent, string childName, Vector3 localPosition)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(child, "Create Child Object");
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        return child;
    }

    private void CreateMiniGameIconIfMissing(Transform parent, string objectName, Sprite sprite, Vector3 localPosition)
    {
        Transform existing = parent.Find(objectName);
        GameObject iconObject;

        if (existing != null)
        {
            iconObject = existing.gameObject;
        }
        else
        {
            iconObject = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(iconObject, "Create Computer Mini Game Icon");
            iconObject.transform.SetParent(parent, false);
            iconObject.transform.localPosition = localPosition;
            iconObject.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        }

        SpriteRenderer renderer = iconObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(iconObject);
        }

        if (sprite != null)
        {
            renderer.sprite = sprite;
        }

        renderer.sortingOrder = 95;

        BoxCollider2D collider = iconObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(iconObject);
        }

        collider.isTrigger = true;
        if (renderer.sprite != null)
        {
            collider.size = renderer.sprite.bounds.size;
        }

        ComputerRoomMiniGameIcon miniGameIcon = iconObject.GetComponent<ComputerRoomMiniGameIcon>();
        if (miniGameIcon == null)
        {
            miniGameIcon = Undo.AddComponent<ComputerRoomMiniGameIcon>(iconObject);
        }

        miniGameIcon.iconName = objectName;
        iconObject.SetActive(true);
        EditorUtility.SetDirty(iconObject);
    }

    [ContextMenu("Arrange All Views Separated For Editing")]
    public void ArrangeAllViewsForEditingMenu()
    {
        ArrangeAllViewsForEditing(true);
    }

    private void ArrangeAllViewsForEditing(bool markDirty)
    {
        if (Application.isPlaying)
        {
            return;
        }

        EnsureHotspotRootEditor();

        ComputerRoomView[] views = GetAllViews();
        for (int i = 0; i < views.Length; i++)
        {
            Transform folder = EnsureViewFolder(views[i]);
            folder.gameObject.SetActive(true);

            Vector3 layoutPosition = GetSeparatedEditPosition(i);
            folder.position = layoutPosition;
            folder.localRotation = Quaternion.identity;
            folder.localScale = Vector3.one;

            EnsurePreviewBackground(folder, views[i]);

            ComputerRoomHotspot[] hotspots = folder.GetComponentsInChildren<ComputerRoomHotspot>(true);
            for (int h = 0; h < hotspots.Length; h++)
            {
                hotspots[h].gameObject.SetActive(true);
                hotspots[h].SetPreviewVisible(showHotspotPreview);
            }

            if (markDirty)
            {
                EditorUtility.SetDirty(folder);
            }
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.gameObject.SetActive(false);
            EditorUtility.SetDirty(backgroundRenderer.gameObject);
        }

        showAllViewsSeparatedInEditMode = true;
        isolateEditModePreview = false;

        if (markDirty)
        {
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }

    private Vector3 GetSeparatedEditPosition(int viewIndex)
    {
        int column = viewIndex % 3;
        int row = viewIndex / 3;
        Vector2 spacing = GetSafeEditViewSpacing();
        float startX = -spacing.x;
        float startY = spacing.y * 0.5f;

        return new Vector3(startX + column * spacing.x, startY - row * spacing.y, -viewIndex * 0.05f);
    }

    private Vector2 GetSafeEditViewSpacing()
    {
        Vector2 maxSize = Vector2.zero;
        ComputerRoomView[] views = GetAllViews();
        for (int i = 0; i < views.Length; i++)
        {
            Sprite sprite = GetBackgroundForView(views[i]);
            if (sprite == null)
            {
                continue;
            }

            Vector2 size = sprite.bounds.size;
            maxSize.x = Mathf.Max(maxSize.x, size.x);
            maxSize.y = Mathf.Max(maxSize.y, size.y);
        }

        float pixelsPerUnit = 100f;
        Sprite firstSprite = frame44MainDoor;
        if (firstSprite != null)
        {
            pixelsPerUnit = Mathf.Max(1f, firstSprite.pixelsPerUnit);
        }

        float gapWorld = Mathf.Max(5f, editViewGapPixels) / pixelsPerUnit;
        float safeX = maxSize.x + gapWorld;
        float safeY = maxSize.y + gapWorld;

        return new Vector2(
            Mathf.Max(editViewSpacing.x, safeX),
            Mathf.Max(editViewSpacing.y, safeY)
        );
    }

    private void EnsurePreviewBackground(Transform folder, ComputerRoomView view)
    {
        Transform found = folder.Find("PreviewBackground");
        if (found == null)
        {
            GameObject backgroundObject = new GameObject("PreviewBackground");
            Undo.RegisterCreatedObjectUndo(backgroundObject, "Create View Preview Background");
            backgroundObject.transform.SetParent(folder, false);
            found = backgroundObject.transform;
        }

        found.localPosition = Vector3.zero;
        found.localRotation = Quaternion.identity;
        found.localScale = Vector3.one;
        found.SetSiblingIndex(0);

        SpriteRenderer renderer = found.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(found.gameObject);
        }

        renderer.sprite = GetBackgroundForView(view);
        renderer.sortingOrder = -100 - (int)view;
        renderer.gameObject.SetActive(true);

        EditorUtility.SetDirty(renderer);
    }

    [ContextMenu("Edit Preview/Show Frame44 MainDoor")]
    public void EditPreviewFrame44()
    {
        SetEditPreviewView(ComputerRoomView.Frame44_MainDoorView);
    }

    [ContextMenu("Edit Preview/Show Frame45 MainComputer")]
    public void EditPreviewFrame45()
    {
        SetEditPreviewView(ComputerRoomView.Frame45_MainComputerView);
    }

    [ContextMenu("Edit Preview/Show Frame46 BackToDoor")]
    public void EditPreviewFrame46()
    {
        SetEditPreviewView(ComputerRoomView.Frame46_BackToDoorView);
    }

    [ContextMenu("Edit Preview/Show Frame47 ComputerDesk")]
    public void EditPreviewFrame47()
    {
        SetEditPreviewView(ComputerRoomView.Frame47_ComputerDeskView);
    }

    [ContextMenu("Edit Preview/Show Frame49 ComputerScreen")]
    public void EditPreviewFrame49()
    {
        SetEditPreviewView(ComputerRoomView.Frame49_ComputerScreenView);
    }

    [ContextMenu("Edit Preview/Show Frame50 DoorClose")]
    public void EditPreviewFrame50()
    {
        SetEditPreviewView(ComputerRoomView.Frame50_DoorCloseView);
    }

    private void SetEditPreviewView(ComputerRoomView view)
    {
        Undo.RecordObject(this, "Change Computer Room Edit Preview");
        editPreviewView = view;
        isolateEditModePreview = true;
        ApplyEditModePreview();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    private void EnsureHotspotRootEditor()
    {
        Transform found = transform.Find("Hotspots");
        if (found == null)
        {
            GameObject rootObject = new GameObject("Hotspots");
            Undo.RegisterCreatedObjectUndo(rootObject, "Create Hotspots Root");
            rootObject.transform.SetParent(transform, false);
            found = rootObject.transform;
        }

        hotspotsRoot = found;
    }

    private Transform EnsureViewFolder(ComputerRoomView view)
    {
        EnsureHotspotRootEditor();

        string folderName = view.ToString();
        Transform folder = hotspotsRoot.Find(folderName);
        if (folder == null)
        {
            GameObject folderObject = new GameObject(folderName);
            Undo.RegisterCreatedObjectUndo(folderObject, "Create Hotspot Folder");
            folderObject.transform.SetParent(hotspotsRoot, false);
            folder = folderObject.transform;
        }

        return folder;
    }

    private void CreateHotspotIfMissing(string hotspotName, ComputerRoomView visibleOnView, ComputerRoomView targetView, Vector3 position, Vector2 size, bool startsPuzzle, bool requiresSolved, bool isExitDoor, bool isContinueAfterPuzzle)
    {
        Transform folder = EnsureViewFolder(visibleOnView);
        Transform existing = folder.Find(hotspotName);
        if (existing != null)
        {
            ComputerRoomHotspot existingHotspot = existing.GetComponent<ComputerRoomHotspot>();
            if (existingHotspot != null)
            {
                existingHotspot.targetView = targetView;
                existingHotspot.startsComputerPuzzle = startsPuzzle;
                existingHotspot.requiresPuzzleSolved = requiresSolved;
                existingHotspot.isExitDoor = isExitDoor;
                existingHotspot.isContinueAfterPuzzle = isContinueAfterPuzzle;
                existing.gameObject.SetActive(true);
                EditorUtility.SetDirty(existingHotspot);
            }

            EnsureHotspotVisual(existing.gameObject, hotspotName, size);
            return;
        }

        GameObject hotspotObject = new GameObject(hotspotName);
        Undo.RegisterCreatedObjectUndo(hotspotObject, "Create Computer Room Hotspot");
        hotspotObject.transform.SetParent(folder, false);
        hotspotObject.transform.localPosition = position;

        BoxCollider2D box = hotspotObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = size;

        ComputerRoomHotspot hotspot = hotspotObject.AddComponent<ComputerRoomHotspot>();
        hotspot.hotspotName = hotspotName;
        hotspot.visibleOnView = visibleOnView;
        hotspot.targetView = targetView;
        hotspot.startsComputerPuzzle = startsPuzzle;
        hotspot.requiresPuzzleSolved = requiresSolved;
        hotspot.isExitDoor = isExitDoor;
        hotspot.isContinueAfterPuzzle = isContinueAfterPuzzle;
        hotspot.canUseWhenPuzzleLocked = false;
        hotspot.boxCollider = box;

        EnsureHotspotVisual(hotspotObject, hotspotName, size);
        EditorUtility.SetDirty(hotspotObject);
    }

    private void DisableHotspotIfExists(ComputerRoomView view, string hotspotName)
    {
        Transform folder = EnsureViewFolder(view);
        Transform found = folder.Find(hotspotName);
        if (found == null)
        {
            return;
        }

        found.gameObject.SetActive(false);
        found.name = hotspotName + "_Disabled";
        EditorUtility.SetDirty(found.gameObject);
    }

    private void RestoreDisabledHotspotIfExists(ComputerRoomView view, string hotspotName)
    {
        Transform folder = EnsureViewFolder(view);
        Transform disabled = folder.Find(hotspotName + "_Disabled");
        if (disabled == null)
        {
            return;
        }

        disabled.name = hotspotName;
        disabled.gameObject.SetActive(true);
        EditorUtility.SetDirty(disabled.gameObject);
    }

    private void EnsureHotspotVisual(GameObject hotspotObject, string hotspotName, Vector2 colliderSize)
    {
        if (hotspotObject == null)
        {
            return;
        }

        ComputerRoomHotspot hotspot = hotspotObject.GetComponent<ComputerRoomHotspot>();
        SpriteRenderer renderer = hotspotObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(hotspotObject);
        }

        if (renderer.sprite == null)
        {
            renderer.sprite = defaultArrowSprite;
        }

        renderer.color = renderer.sprite == null ? new Color(1f, 0.85f, 0.05f, 0.35f) : Color.white;
        renderer.sortingOrder = 50;

        if (hotspotObject.transform.localScale == Vector3.one)
        {
            hotspotObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }

        if (hotspot != null)
        {
            hotspot.debugRenderer = renderer;
            EditorUtility.SetDirty(hotspot);
        }

        if (hotspotName.Contains("Back") || hotspotName.Contains("TurnBack"))
        {
            hotspotObject.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
        }

        Transform label = hotspotObject.transform.Find("Label");
        if (label == null)
        {
            CreateLabel(hotspotObject.transform, hotspotName, colliderSize);
        }

        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(hotspotObject);
    }

    private void CreateLabel(Transform hotspotTransform, string label, Vector2 colliderSize)
    {
        GameObject labelObject = new GameObject("Label");
        Undo.RegisterCreatedObjectUndo(labelObject, "Create Hotspot Label");
        labelObject.transform.SetParent(hotspotTransform, false);
        labelObject.transform.localPosition = new Vector3(0f, colliderSize.y * 0.5f + 0.22f, 0f);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = NicifyLabel(label);
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.08f;
        textMesh.fontSize = 32;
        textMesh.color = Color.white;

        MeshRenderer meshRenderer = labelObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 60;
        }
    }

    private string NicifyLabel(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Hotspot";
        }

        return ObjectNames.NicifyVariableName(raw);
    }
#endif
}
