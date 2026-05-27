using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// PauseMenu.cs
/// Quản lý toàn bộ hệ thống Pause Menu của game.
/// Dùng New Input System (InputSystem package) để bắt phím ESC.
/// Gắn script này vào GameObject "PauseMenuManager".
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR FIELDS – Kéo các object vào đây
    // ─────────────────────────────────────────────

    [Header("═══ UI References ═══")]
    [Tooltip("Panel cha chứa toàn bộ Pause Menu (PauseMenuPanel)")]
    public GameObject pauseMenuPanel;

    [Tooltip("CanvasGroup dùng để fade in/out (gắn trên PauseMenuPanel)")]
    public CanvasGroup canvasGroup;

    [Header("═══ Scene Settings ═══")]
    [Tooltip("Tên scene Menu Chính – phải trùng với tên trong Build Settings")]
    public string mainMenuSceneName = "main menu";

    [Header("═══ Button Customization ═══")]
    [Tooltip("Nút Continue")]
    public Button continueButton;

    [Tooltip("Nút Exit To Menu")]
    public Button exitToMenuButton;

    [Tooltip("Hình nền nút Continue (để trống = dùng màu solid)")]
    public Sprite continueButtonSprite;

    [Tooltip("Hình nền nút Exit To Menu (để trống = dùng màu solid)")]
    public Sprite exitToMenuButtonSprite;

    [Tooltip("Màu nút Continue")]
    public Color continueButtonColor = new Color(0.12f, 0.56f, 1f, 1f);

    [Tooltip("Màu nút Exit To Menu")]
    public Color exitToMenuButtonColor = new Color(0.85f, 0.20f, 0.20f, 1f);

    [Tooltip("Màu chữ trên button")]
    public Color buttonTextColor = Color.white;

    [Tooltip("Cỡ chữ trên button")]
    [Range(12, 48)]
    public int buttonFontSize = 20;

    [Header("═══ Panel Customization ═══")]
    [Tooltip("Hình nền của card giữa màn hình (để trống = dùng màu solid)")]
    public Sprite menuCardSprite;

    [Tooltip("Màu nền card")]
    public Color menuCardColor = new Color(0.08f, 0.08f, 0.10f, 0.95f);

    [Tooltip("Hình nền phủ toàn màn hình (để trống = dùng màu tối mờ)")]
    public Sprite dimmerSprite;

    [Tooltip("Màu nền tối phủ toàn màn hình")]
    public Color dimmerColor = new Color(0f, 0f, 0f, 0.65f);

    [Header("═══ Title Customization ═══")]
    [Tooltip("Text hiển thị khi pause (mặc định: TẠM DỪNG)")]
    public string pauseTitle = "TẠM DỪNG";

    [Tooltip("Cỡ chữ tiêu đề")]
    [Range(16, 72)]
    public int titleFontSize = 36;

    [Tooltip("Màu chữ tiêu đề")]
    public Color titleColor = Color.white;


    [Header("═══ Player (Tùy chọn) ═══")]
    [Tooltip("Script movement của player – sẽ bị tắt khi pause")]
    public MonoBehaviour playerMovementScript;

    [Header("═══ Fade Settings ═══")]
    [Tooltip("Thời gian fade in / fade out (giây)")]
    [Range(0.05f, 0.5f)]
    public float fadeDuration = 0.15f;

    // ─────────────────────────────────────────────
    // PRIVATE REFERENCES (tìm tự động)
    // ─────────────────────────────────────────────
    private Image dimmerImage;       // Image phủ màn hình
    private Image menuCardImage;     // Image của card giữa
    private Text titleText;          // Text tiêu đề
    private Image continueImg;       // Image của nút Continue
    private Image exitImg;           // Image của nút Exit
    private Text continueText;       // Text trên nút Continue
    private Text exitText;           // Text trên nút Exit

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────

    // Trạng thái game có đang pause không
    private bool isPaused = false;

    // Dùng để ngăn spam nhấn ESC trong lúc đang fade
    private bool isFading = false;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Start()
    {
        // Đảm bảo menu ẩn ngay khi game bắt đầu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Bắt đầu với alpha = 0 (trong suốt)
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // Đảm bảo game đang chạy bình thường
        Time.timeScale = 1f;

        // Tự tìm các component con để apply customization
        FindUIReferences();

        // Tự động gắn sự kiện click cho các nút bấm
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ResumeGame);
        }

        if (exitToMenuButton != null)
        {
            exitToMenuButton.onClick.RemoveAllListeners();
            exitToMenuButton.onClick.AddListener(ExitToMenu);
        }

        // Apply các thiết lập từ Inspector
        ApplyCustomization();
    }

#if UNITY_EDITOR
    // Hàm này tự động chạy trong Editor mỗi khi mày thay đổi giá trị trong Inspector
    private void OnValidate()
    {
        // Tìm lại các component vì ở chế độ Edit có thể nó chưa được gán
        FindUIReferences();
        
        // Cập nhật giao diện ngay lập tức để preview
        // EditorApplication.delayCall giúp tránh cảnh báo của Unity khi sửa UI trong OnValidate
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) ApplyCustomization();
        };
    }
#endif

    private void Update()
    {
        // ═══ NEW INPUT SYSTEM (CHUẨN NHẤT) ═══
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && !isFading)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ─────────────────────────────────────────────
    // PUBLIC METHODS – Gắn vào Button OnClick
    // ─────────────────────────────────────────────

    /// <summary>
    /// Gọi khi người chơi nhấn nút "Continue".
    /// Đóng menu và tiếp tục game.
    /// </summary>
    public void ResumeGame()
    {
        StartCoroutine(FadeAndResume());
    }

    /// <summary>
    /// Gọi khi người chơi nhấn nút "Exit To Menu".
    /// Load scene Menu Chính.
    /// </summary>
    public void ExitToMenu()
    {
        StartCoroutine(FadeAndLoadMenu());
    }

    // ─────────────────────────────────────────────
    // CUSTOMIZATION – Tìm & áp dụng thiết lập
    // ─────────────────────────────────────────────

    /// <summary>
    /// Tìm tất cả UI component con từ hierarchy.
    /// Chạy 1 lần trong Start().
    /// </summary>
    private void FindUIReferences()
    {
        if (pauseMenuPanel == null) return;

        // Dimmer = chính PauseMenuPanel
        dimmerImage = pauseMenuPanel.GetComponent<Image>();

        // MenuCard
        var cardTransform = pauseMenuPanel.transform.Find("MenuCard");
        if (cardTransform != null)
            menuCardImage = cardTransform.GetComponent<Image>();

        // TitleText
        if (cardTransform != null)
        {
            var titleTransform = cardTransform.Find("TitleText");
            if (titleTransform != null)
                titleText = titleTransform.GetComponent<Text>();
        }

        // Buttons – lấy Image và Text
        if (continueButton != null)
        {
            continueImg = continueButton.GetComponent<Image>();
            continueText = continueButton.GetComponentInChildren<Text>();
        }

        if (exitToMenuButton != null)
        {
            exitImg = exitToMenuButton.GetComponent<Image>();
            exitText = exitToMenuButton.GetComponentInChildren<Text>();
        }
    }

    /// <summary>
    /// Áp dụng tất cả thiết lập tuỳ chỉnh từ Inspector.
    /// Gọi trong Start() và cũng có thể gọi lại bất cứ lúc nào.
    /// </summary>
    public void ApplyCustomization()
    {
        // ── Dimmer ──
        if (dimmerImage != null)
        {
            dimmerImage.color = dimmerColor;
            if (dimmerSprite != null)
                dimmerImage.sprite = dimmerSprite;
        }

        // ── Card ──
        if (menuCardImage != null)
        {
            menuCardImage.color = menuCardColor;
            if (menuCardSprite != null)
                menuCardImage.sprite = menuCardSprite;
        }

        // ── Title ──
        if (titleText != null)
        {
            titleText.text = pauseTitle;
            titleText.fontSize = titleFontSize;
            titleText.color = titleColor;
        }

        // ── Continue Button ──
        if (continueImg != null)
        {
            continueImg.color = continueButtonColor;
            if (continueButtonSprite != null)
                continueImg.sprite = continueButtonSprite;
        }
        if (continueText != null)
        {
            continueText.color = buttonTextColor;
            continueText.fontSize = buttonFontSize;
        }
        if (continueButton != null)
        {
            var colors = continueButton.colors;
            colors.normalColor = continueButtonColor;
            colors.highlightedColor = LightenColor(continueButtonColor, 0.15f);
            colors.pressedColor = DarkenColor(continueButtonColor, 0.15f);
            colors.selectedColor = continueButtonColor;
            continueButton.colors = colors;
        }

        // ── Exit Button ──
        if (exitImg != null)
        {
            exitImg.color = exitToMenuButtonColor;
            if (exitToMenuButtonSprite != null)
                exitImg.sprite = exitToMenuButtonSprite;
        }
        if (exitText != null)
        {
            exitText.color = buttonTextColor;
            exitText.fontSize = buttonFontSize;
        }
        if (exitToMenuButton != null)
        {
            var colors = exitToMenuButton.colors;
            colors.normalColor = exitToMenuButtonColor;
            colors.highlightedColor = LightenColor(exitToMenuButtonColor, 0.15f);
            colors.pressedColor = DarkenColor(exitToMenuButtonColor, 0.15f);
            colors.selectedColor = exitToMenuButtonColor;
            exitToMenuButton.colors = colors;
        }
    }

    // ─────────────────────────────────────────────
    // PAUSE / RESUME LOGIC
    // ─────────────────────────────────────────────

    private void PauseGame()
    {
        isPaused = true;

        // Dừng game lại
        Time.timeScale = 0f;

        // Hiện cursor để người chơi dùng chuột bấm button
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Khóa script movement của player (nếu có gán)
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Hiện menu với hiệu ứng fade in
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }

    private void DoResume()
    {
        isPaused = false;

        // Chạy game lại
        Time.timeScale = 1f;

        // Mở khóa script movement của player
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // Ẩn menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // COROUTINES – Fade Animation
    // ─────────────────────────────────────────────

    /// <summary>
    /// Fade in (alpha: 0 → 1) khi mở menu.
    /// </summary>
    private IEnumerator FadeIn()
    {
        isFading = true;

        if (canvasGroup != null)
        {
            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < fadeDuration)
            {
                // unscaledDeltaTime vì Time.timeScale = 0
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        isFading = false;
    }

    /// <summary>
    /// Fade out (alpha: 1 → 0) rồi Resume.
    /// </summary>
    private IEnumerator FadeAndResume()
    {
        isFading = true;

        if (canvasGroup != null)
        {
            float elapsed = 0f;
            canvasGroup.alpha = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        isFading = false;
        DoResume();
    }

    /// <summary>
    /// Hiển thị chữ tạm thời vì chưa có Main Menu
    /// </summary>
    private IEnumerator FadeAndLoadMenu()
    {
        if (titleText != null)
        {
            titleText.text = "CHƯA CÓ MAIN MENU NHA SẾP!";
            titleText.color = Color.yellow;
        }
        
        Debug.Log("🤖 AI: Mày vừa bấm Exit! Vì chưa có scene MainMenu nên tao cho hiện chữ báo lỗi tạm thời nhé.");
        
        yield return null;
    }


    private static Color LightenColor(Color c, float amount) =>
        new Color(
            Mathf.Clamp01(c.r + amount),
            Mathf.Clamp01(c.g + amount),
            Mathf.Clamp01(c.b + amount),
            c.a);

    private static Color DarkenColor(Color c, float amount) =>
        new Color(
            Mathf.Clamp01(c.r - amount),
            Mathf.Clamp01(c.g - amount),
            Mathf.Clamp01(c.b - amount),
            c.a);
}
