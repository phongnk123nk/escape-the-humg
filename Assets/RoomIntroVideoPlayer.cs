using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Phat video mo dau cho scene room1, sau do fade tu man den ve gameplay.
/// Script tu tao overlay runtime nen khong lam thay doi UI/gameplay san co.
/// </summary>
[DisallowMultipleComponent]
public class RoomIntroVideoPlayer : MonoBehaviour
{
    [Header("Intro Video")]
    public VideoClip introClip;
    public bool playOnStart = true;

    [Header("Transition")]
    [Min(0f)] public float blackScreenDelay = 0.15f;
    [Min(0f)] public float fadeFromBlackDuration = 1.2f;
    public int sortingOrder = 5000;

    private Canvas overlayCanvas;
    private CanvasGroup overlayGroup;
    private RawImage videoImage;
    private Image blackImage;
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private bool finished;

    private void Start()
    {
        if (playOnStart)
        {
            PlayIntro();
        }
    }

    public void PlayIntro()
    {
        if (introClip == null || finished)
        {
            return;
        }

        CreateOverlay();
        StartCoroutine(PlayIntroRoutine());
    }

    private void CreateOverlay()
    {
        GameObject canvasObject = new GameObject("Room1IntroVideoOverlay");
        canvasObject.transform.SetParent(transform, false);

        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        overlayGroup = canvasObject.AddComponent<CanvasGroup>();
        overlayGroup.blocksRaycasts = true;
        overlayGroup.interactable = true;
        overlayGroup.alpha = 1f;

        GameObject videoObject = new GameObject("IntroVideoImage");
        videoObject.transform.SetParent(canvasObject.transform, false);
        videoImage = videoObject.AddComponent<RawImage>();
        Stretch(videoImage.rectTransform);

        GameObject blackObject = new GameObject("IntroBlackFade");
        blackObject.transform.SetParent(canvasObject.transform, false);
        blackImage = blackObject.AddComponent<Image>();
        blackImage.color = Color.clear;
        Stretch(blackImage.rectTransform);

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.clip = introClip;

        renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        renderTexture.Create();
        videoPlayer.targetTexture = renderTexture;
        videoImage.texture = renderTexture;
    }

    private IEnumerator PlayIntroRoutine()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        videoImage.enabled = false;
        blackImage.color = Color.black;

        if (blackScreenDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(blackScreenDelay);
        }

        float elapsed = 0f;
        while (elapsed < fadeFromBlackDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / Mathf.Max(0.0001f, fadeFromBlackDuration));
            blackImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        finished = true;
        CleanupOverlay();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private void CleanupOverlay()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
            Destroy(videoPlayer);
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (overlayCanvas != null)
        {
            Destroy(overlayCanvas.gameObject);
        }
    }
}
