using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý luồng Cutscene Ending cho game.
/// Code được viết tối ưu, dễ đọc và dễ mở rộng cho người mới (Beginner-Friendly).
/// </summary>
public class EndingCutscene : MonoBehaviour
{
    [Header("--- THIẾT LẬP HÌNH ẢNH (UI) ---")]
    [Tooltip("Hình nền đen (luôn nằm dưới cùng).")]
    public Image blackBackground;
    
    [Tooltip("Hình ảnh Good Ending chính của bạn.")]
    public Image endingImage;

    [Tooltip("Hình ảnh thứ 2. MẸO: Bạn có thể tạo vô số dòng Text làm 'con' của ảnh này, chúng sẽ tự động hiện mờ dần theo!")]
    public Image midImage1;
    
    [Tooltip("Hình ảnh thứ 3 (sếp có thể chèn hình bất kỳ vào đây).")]
    public Image finalBlackImage;

    [Header("--- THIẾT LẬP THỜI GIAN (TIMING) ---")]
    [Tooltip("Thời gian chờ ở màn hình đen ban đầu (giây).")]
    public float startBlackDelay = 2.0f;
    
    [Tooltip("Thời gian để hình ảnh 1 từ từ hiện ra (giây).")]
    public float fadeInDuration = 3.0f;
    
    [Tooltip("Thời gian giữ nguyên hình ảnh 1 trên màn hình (giây).")]
    public float imageHoldDuration = 4.0f;

    [Tooltip("Thời gian để hình ảnh 2 (và các Text bên trong nó) từ từ hiện ra (giây).")]
    public float midImage1FadeInDuration = 3.0f;

    [Tooltip("Thời gian giữ nguyên hình ảnh 2 trên màn hình (giây).")]
    public float midImage1HoldDuration = 4.0f;

    [Tooltip("Thời gian để hình ảnh 2 (và Text) mờ dần biến mất trước khi sang ảnh 3 (giây).")]
    public float midImage1FadeOutDuration = 1.0f;
    
    [Tooltip("Thời gian để hình ảnh 3 từ từ hiện ra (giây).")]
    public float fadeOutDuration = 2.5f;

    [Tooltip("Thời gian giữ nguyên hình ảnh 3 trên màn hình trước khi bắt đầu tối đen dần (giây).")]
    public float finalImageHoldDuration = 3.0f;

    [Header("--- BƯỚC CUỐI: TỐI DẦN VÀ CHUYỂN SCENE ---")]
    [Tooltip("Màn hình đen phụ dùng để làm tối dần màn hình lúc cuối cùng.")]
    public Image transitionBlackScreen;

    [Tooltip("Thời gian màn hình từ từ chuyển sang Đen Thui (2-3 giây như sếp muốn).")]
    public float transitionFadeDuration = 3.0f;

    [Tooltip("Tên Scene để chuyển qua sau khi màn hình đã đen hẳn.")]
    public string nextSceneName = "MainMenu";

    [Tooltip("Thời gian chờ ngâm ở màn hình đen hoàn toàn trước khi chuyển sang Scene mới (giây).")]
    public float waitBeforeLoadScene = 1.0f;

    [Header("--- BONUS: THIẾT LẬP ÂM THANH ---")]
    [Tooltip("Kéo AudioSource chứa nhạc Ending vào đây (nếu có).")]
    public AudioSource endingMusic;
    
    [Tooltip("Thời gian nhạc to dần lên (fade in audio) (giây).")]
    public float audioFadeInDuration = 2.0f;

    [Header("--- THIẾT LẬP ĐIỀU KHIỂN ---")]
    [Tooltip("Cho phép người chơi Skip cutscene bằng các phím này.")]
    public KeyCode skipKey1 = KeyCode.Space;
    public KeyCode skipKey2 = KeyCode.Escape;

    // Biến lưu trữ tiến trình để có thể dừng giữa chừng khi Skip
    private Coroutine cutsceneCoroutine;
    private bool isSkipped = false;

    void Start()
    {
        // Kiểm tra và tự động thêm CanvasGroup cho ảnh giữa (để có thể fade cùng các Text con bên trong)
        if (midImage1 != null && midImage1.GetComponent<CanvasGroup>() == null)
        {
            midImage1.gameObject.AddComponent<CanvasGroup>();
        }

        // 1. Khởi tạo trạng thái ban đầu khi bắt đầu Scene
        InitializeCutscene();

        // 2. Bắt đầu chạy kịch bản Cutscene
        cutsceneCoroutine = StartCoroutine(PlayCutsceneFlow());
    }

    void Update()
    {
        // Kiểm tra người chơi có nhấn phím Skip không
        if (!isSkipped)
        {
            bool skipPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || 
                    UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    skipPressed = true;
                }
            }
#else
            if (Input.GetKeyDown(skipKey1) || Input.GetKeyDown(skipKey2))
            {
                skipPressed = true;
            }
#endif

            if (skipPressed)
            {
                SkipCutscene();
            }
        }
    }

    /// <summary>
    /// Hàm thiết lập trạng thái ban đầu của các hình ảnh.
    /// </summary>
    private void InitializeCutscene()
    {
        SetAlpha(blackBackground, 1f);
        
        SetAlpha(endingImage, 0f);
        SetAlpha(midImage1, 0f);
        SetAlpha(finalBlackImage, 0f);
        
        if (transitionBlackScreen != null) SetAlpha(transitionBlackScreen, 0f);

        if (endingMusic != null)
        {
            endingMusic.volume = 0f;
        }
    }

    /// <summary>
    /// Kịch bản chính của Cutscene (Chạy từng bước theo yêu cầu).
    /// </summary>
    private IEnumerator PlayCutsceneFlow()
    {
        // --- BƯỚC 1: MÀN HÌNH ĐEN HOÀN TOÀN ---
        yield return new WaitForSeconds(startBlackDelay);

        if (endingMusic != null)
        {
            endingMusic.Play();
            StartCoroutine(FadeAudio(endingMusic, 0f, 1f, audioFadeInDuration));
        }

        // --- BƯỚC 2: ẢNH 1 HIỆN MỜ DẦN (FADE IN) ---
        if (endingImage != null)
        {
            yield return StartCoroutine(FadeImage(endingImage, 0f, 1f, fadeInDuration));
            yield return new WaitForSeconds(imageHoldDuration);
        }

        // --- BƯỚC 3: ẢNH 2 (VÀ TOÀN BỘ CÁC TEXT BÊN TRONG NÓ) HIỆN MỜ DẦN ---
        if (midImage1 != null)
        {
            yield return StartCoroutine(FadeImage(midImage1, 0f, 1f, midImage1FadeInDuration));
            yield return new WaitForSeconds(midImage1HoldDuration);
            
            // Cho ảnh 2 (và text) mờ dần biến mất
            if (midImage1FadeOutDuration > 0f)
            {
                yield return StartCoroutine(FadeImage(midImage1, 1f, 0f, midImage1FadeOutDuration));
            }
        }

        // --- BƯỚC 4: HÌNH ẢNH THỨ 3 (Mà sếp gọi là Image Final Black) ---
        if (finalBlackImage != null)
        {
            yield return StartCoroutine(FadeImage(finalBlackImage, 0f, 1f, fadeOutDuration));
            // Giữ màn hình này một chút trước khi bị bóng tối nuốt chửng
            yield return new WaitForSeconds(finalImageHoldDuration);
        }

        // --- BƯỚC 5: MỘT MÀN HÌNH ĐEN THUI TỐI DẦN (2-3 GIÂY) ---
        if (transitionBlackScreen != null)
        {
            Debug.Log("Bắt đầu tối dần màn hình trong vòng " + transitionFadeDuration + " giây...");
            yield return StartCoroutine(FadeImage(transitionBlackScreen, 0f, 1f, transitionFadeDuration));
        }

        Debug.Log("Màn hình đã đen hẳn. Đợi một chút rồi chuyển Scene...");

        // --- BƯỚC 6: ĐỢI NGÂM Ở ĐEN THUI RỒI CHUYỂN SCENE ---
        yield return new WaitForSeconds(waitBeforeLoadScene);

        LoadNextScene();
    }

    /// <summary>
    /// Hàm xử lý khi người chơi nhấn Skip Cutscene.
    /// </summary>
    private void SkipCutscene()
    {
        isSkipped = true;

        if (cutsceneCoroutine != null)
        {
            StopCoroutine(cutsceneCoroutine);
        }

        SetAlpha(endingImage, 0f);
        SetAlpha(midImage1, 0f);
        SetAlpha(finalBlackImage, 1f);
        if (transitionBlackScreen != null) SetAlpha(transitionBlackScreen, 1f);

        if (endingMusic != null)
        {
            endingMusic.volume = 1f;
            if (!endingMusic.isPlaying)
            {
                endingMusic.Play();
            }
        }

        Debug.Log("Người chơi đã Skip Cutscene. Đang chuyển Scene...");
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning($"[HỆ THỐNG] Không tìm thấy Scene '{nextSceneName}' trong Build Settings. Tạm thời tải lại cảnh này!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void SetAlpha(Image img, float alphaValue)
    {
        if (img != null)
        {
            CanvasGroup cg = img.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = alphaValue;
                Color c = img.color;
                if (c.a != 1f) {
                    c.a = 1f;
                    img.color = c;
                }
            }
            else
            {
                Color c = img.color;
                c.a = alphaValue;
                img.color = c;
            }
        }
    }

    private IEnumerator FadeImage(Image img, float startAlpha, float targetAlpha, float duration)
    {
        if (img == null) yield break;

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
            SetAlpha(img, newAlpha);
            yield return null; 
        }
        SetAlpha(img, targetAlpha);
    }

    private IEnumerator FadeAudio(AudioSource audioSrc, float startVol, float targetVol, float duration)
    {
        if (audioSrc == null) yield break;

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(startVol, targetVol, timeElapsed / duration);
            yield return null;
        }

        audioSrc.volume = targetVol;
    }
}
