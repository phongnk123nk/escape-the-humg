using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Tao menu pause dung chung cho toan game.
/// Neu scene da co UI pause thi script se dung UI do de may chinh sua truc tiep.
/// Neu chua co UI thi script moi tu tao UI tam khi Play.
/// </summary>
public class GlobalEscPauseMenu : MonoBehaviour
{
    [Header("Scene")]
    public string mainMenuSceneName = "main menu";
    public string[] scenesDisablePause = { "main menu" };

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("UI")]
    public GameObject pauseMenuPanel;
    public CanvasGroup pauseCanvasGroup;
    public Button continueButton;
    public Button exitToMenuButton;

    [Header("Default UI Text")]
    public string titleText = "TẠM DỪNG";
    public string continueText = "TIẾP TỤC";
    public string exitText = "THOÁT";
    public float fadeDuration = 0.15f;

    private static GlobalEscPauseMenu instance;

    private Canvas canvas;
    private GameObject panel;
    private bool isPaused;
    private bool isFading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnGameStart()
    {
        if (instance != null) return;

        GameObject obj = new GameObject("GlobalEscPauseMenu");
        DontDestroyOnLoad(obj);
        instance = obj.AddComponent<GlobalEscPauseMenu>();
        instance.BuildPauseMenu();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        SetupPauseMenu();
        ApplyPauseAvailabilityForCurrentScene();
    }

    private void Update()
    {
        if (IsPauseDisabledInCurrentScene())
            return;

        if (isFading) return;

        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (IsPauseDisabledInCurrentScene())
            return;

        EnsureEventSystem();

        if (panel == null)
            SetupPauseMenu();

        isPaused = true;
        Time.timeScale = 0f;

        panel.SetActive(true);
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        StartCoroutine(FadeCanvas(0f, 1f, null));
    }

    public void ResumeGame()
    {
        StartCoroutine(FadeCanvas(1f, 0f, () =>
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (panel != null)
                panel.SetActive(false);

            if (pauseCanvasGroup != null)
            {
                pauseCanvasGroup.interactable = false;
                pauseCanvasGroup.blocksRaycasts = false;
            }
        }));
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureEventSystem();
        ApplyPauseAvailabilityForCurrentScene();
    }

    private void ApplyPauseAvailabilityForCurrentScene()
    {
        if (!IsPauseDisabledInCurrentScene())
            return;

        isPaused = false;
        isFading = false;
        Time.timeScale = 1f;

        if (panel != null)
            panel.SetActive(false);

        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }
    }

    private bool IsPauseDisabledInCurrentScene()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName) && activeSceneName == mainMenuSceneName)
            return true;

        if (scenesDisablePause == null)
            return false;

        for (int i = 0; i < scenesDisablePause.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(scenesDisablePause[i]) && activeSceneName == scenesDisablePause[i])
                return true;
        }

        return false;
    }

    private IEnumerator FadeCanvas(float from, float to, System.Action onDone)
    {
        isFading = true;

        if (pauseCanvasGroup != null)
        {
            float elapsed = 0f;
            pauseCanvasGroup.alpha = from;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                pauseCanvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }

            pauseCanvasGroup.alpha = to;
        }

        isFading = false;
        onDone?.Invoke();
    }

    private void SetupPauseMenu()
    {
        EnsureEventSystem();
        EnsureCanvasComponents();

        if (pauseMenuPanel == null)
            BuildPauseMenu();
        else
            UseExistingPauseMenu();

        BindButtons();
    }

    private void EnsureCanvasComponents()
    {
        if (canvas == null)
            canvas = gameObject.GetComponent<Canvas>();

        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void UseExistingPauseMenu()
    {
        panel = pauseMenuPanel;

        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();

        if (pauseCanvasGroup == null)
            pauseCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();

        if (continueButton == null)
            continueButton = FindButtonByName(pauseMenuPanel.transform, "ContinueButton");

        if (exitToMenuButton == null)
            exitToMenuButton = FindButtonByName(pauseMenuPanel.transform, "ExitToMenuButton");

        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
        panel.SetActive(false);
    }

    private void BindButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ResumeGame);
        }

        if (exitToMenuButton != null)
        {
            exitToMenuButton.onClick.RemoveAllListeners();
            exitToMenuButton.onClick.AddListener(ExitToMainMenu);
        }
    }

    private static Button FindButtonByName(Transform root, string buttonName)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].name == buttonName)
                return buttons[i];
        }

        return null;
    }

    private void BuildPauseMenu()
    {
        EnsureCanvasComponents();

        panel = CreatePanel("PauseMenuPanel", transform);
        pauseMenuPanel = panel;
        pauseCanvasGroup = panel.AddComponent<CanvasGroup>();
        pauseCanvasGroup.alpha = 0f;
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
        panel.SetActive(false);

        Image dim = panel.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.68f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject card = CreatePanel("MenuCard", panel.transform);
        Image cardImage = card.GetComponent<Image>();
        cardImage.color = new Color(0.08f, 0.08f, 0.09f, 0.94f);

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(560f, 360f);
        cardRect.anchoredPosition = Vector2.zero;

        CreateText("TitleText", card.transform, titleText, 48, Color.white, new Vector2(0f, 95f), new Vector2(500f, 80f));
        continueButton = CreateButton("ContinueButton", card.transform, continueText, new Vector2(0f, 10f), ResumeGame);
        exitToMenuButton = CreateButton("ExitToMenuButton", card.transform, exitText, new Vector2(0f, -90f), ExitToMainMenu);
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<CanvasRenderer>();
        obj.AddComponent<Image>();
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

    private static Button CreateButton(string name, Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = CreatePanel(name, parent);
        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.55f, 0.05f, 0.05f, 1f);

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

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
        EventSystem existing = FindObjectOfType<EventSystem>();
        if (existing != null)
        {
#if ENABLE_INPUT_SYSTEM
            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                StandaloneInputModule oldModule = existing.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    Destroy(oldModule);
                }

                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#endif
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystem.AddComponent<InputSystemUIInputModule>();
#else
        eventSystem.AddComponent<StandaloneInputModule>();
#endif
    }
}
