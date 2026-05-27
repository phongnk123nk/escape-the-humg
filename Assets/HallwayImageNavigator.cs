using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Dieu huong cac man anh trong scene hanh lang 1.
/// Moi frame la mot GameObject that trong Hierarchy de co the keo chinh truc tiep.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Hallway/Hallway Image Navigator")]
public class HallwayImageNavigator : MonoBehaviour
{
    [Header("Scene")]
    public string expectedSceneName = "hanh lang 1";
    public int startFrameIndex;
    public bool loopLastFrameToFirst = true;
    public bool moveCameraToActiveFrame = true;
    public Camera targetCamera;

    [Header("Background Fit")]
    public bool autoFitBackgroundToCamera = false;
    [Range(0.5f, 1f)]
    public float fitScreenPercent = 1f;

    [Header("Frames")]
    public Transform framesRoot;
    public HallwayFrame[] frames = new HallwayFrame[0];

    [Header("Transition Effect")]
    public bool useFadeTransition = true;
    [Min(0f)]
    public float fadeOutDuration = 0.25f;
    [Min(0f)]
    public float fadeInDuration = 0.25f;

    private int currentFrameIndex;
    private bool isChangingFrame;
    private bool isPlayingTransitionVideo;
    private VideoPlayer transitionVideoPlayer;
    private SpriteRenderer fadeRenderer;
    private Sprite fadeSprite;

    private void Awake()
    {
        RefreshFramesFromHierarchy();
    }

    private void Start()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != expectedSceneName)
        {
            Debug.LogWarning("HallwayImageNavigator dang chay khac scene mong doi: " + expectedSceneName, this);
        }

        ShowFrame(startFrameIndex);
    }

    public void GoNext()
    {
        if (isChangingFrame || isPlayingTransitionVideo)
        {
            return;
        }

        int nextIndex = GetNextFrameIndex();
        if (nextIndex < 0)
        {
            return;
        }

        StartCoroutine(ShowFrameWithTransitionRoutine(nextIndex));
    }

    public void PlayVideoThenNext(VideoClip videoClip)
    {
        PlayVideoThenNext(videoClip, string.Empty);
    }

    public void PlayVideoThenNext(VideoClip videoClip, string streamingAssetsVideoFileName)
    {
        if (isChangingFrame || isPlayingTransitionVideo)
        {
            return;
        }

        bool hasStreamingFile = HasStreamingVideoFile(streamingAssetsVideoFileName);
        if (videoClip == null && !hasStreamingFile)
        {
            Debug.LogWarning("Chua gan video cho mui ten hanh lang.", this);
            GoNext();
            return;
        }

        StartCoroutine(PlayVideoThenNextRoutine(videoClip, streamingAssetsVideoFileName));
    }

    public void ShowFrame(int frameIndex)
    {
        ShowFrameImmediate(frameIndex);
    }

    private void ShowFrameImmediate(int frameIndex)
    {
        if (frames == null || frames.Length == 0)
        {
            RefreshFramesFromHierarchy();
        }

        if (frames == null || frames.Length == 0)
        {
            return;
        }

        frameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
        currentFrameIndex = frameIndex;

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null && frames[i].root != null)
            {
                frames[i].root.gameObject.SetActive(i == currentFrameIndex);
            }
        }

        MoveCameraToCurrentFrame();
        if (autoFitBackgroundToCamera)
        {
            FitCurrentBackgroundToCamera();
        }
    }

    private IEnumerator ShowFrameWithTransitionRoutine(int frameIndex)
    {
        isChangingFrame = true;
        yield return FadeScreen(1f, fadeOutDuration);
        ShowFrameImmediate(frameIndex);
        yield return FadeScreen(0f, fadeInDuration);
        isChangingFrame = false;
    }

    private IEnumerator PlayVideoThenNextRoutine(VideoClip videoClip, string streamingAssetsVideoFileName)
    {
        isPlayingTransitionVideo = true;

        Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
        if (cameraToUse == null)
        {
            isPlayingTransitionVideo = false;
            GoNext();
            yield break;
        }

        if (transitionVideoPlayer == null)
        {
            transitionVideoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (transitionVideoPlayer == null)
            {
                transitionVideoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }

        bool videoEnded = false;
        bool videoError = false;
        yield return FadeScreen(1f, fadeOutDuration);

        transitionVideoPlayer.Stop();
        transitionVideoPlayer.playOnAwake = false;
        transitionVideoPlayer.isLooping = false;
        transitionVideoPlayer.waitForFirstFrame = true;
        transitionVideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        transitionVideoPlayer.targetCamera = cameraToUse;
        transitionVideoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        transitionVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        transitionVideoPlayer.errorReceived += OnVideoError;
        if (HasStreamingVideoFile(streamingAssetsVideoFileName))
        {
            transitionVideoPlayer.source = VideoSource.Url;
            transitionVideoPlayer.url = GetStreamingVideoPath(streamingAssetsVideoFileName);
        }
        else
        {
            transitionVideoPlayer.source = VideoSource.VideoClip;
            transitionVideoPlayer.clip = videoClip;
        }

        transitionVideoPlayer.loopPointReached += OnVideoEnded;
        transitionVideoPlayer.Play();
        yield return FadeScreen(0f, fadeInDuration);

        float timeout = GetVideoTimeoutSeconds(videoClip);
        float elapsed = 0f;
        while (!videoEnded && !videoError && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transitionVideoPlayer.loopPointReached -= OnVideoEnded;
        transitionVideoPlayer.errorReceived -= OnVideoError;
        transitionVideoPlayer.Stop();
        transitionVideoPlayer.clip = null;
        transitionVideoPlayer.url = string.Empty;

        yield return FadeScreen(1f, fadeOutDuration);
        isPlayingTransitionVideo = false;
        int nextIndex = GetNextFrameIndex();
        if (nextIndex >= 0)
        {
            ShowFrameImmediate(nextIndex);
        }

        yield return FadeScreen(0f, fadeInDuration);

        void OnVideoEnded(VideoPlayer player)
        {
            videoEnded = true;
        }

        void OnVideoError(VideoPlayer player, string message)
        {
            Debug.LogWarning("Khong phat duoc video hanh lang, tu chuyen sang frame tiep theo: " + message, this);
            videoError = true;
        }
    }

    private bool HasStreamingVideoFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        return File.Exists(GetStreamingVideoPath(fileName));
    }

    private string GetStreamingVideoPath(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, fileName);
    }

    private float GetVideoTimeoutSeconds(VideoClip videoClip)
    {
        if (videoClip != null && videoClip.length > 0.1)
        {
            return (float)videoClip.length + 2f;
        }

        return 30f;
    }

    private int GetNextFrameIndex()
    {
        int nextIndex = currentFrameIndex + 1;
        if (nextIndex < frames.Length)
        {
            return nextIndex;
        }

        return loopLastFrameToFirst ? 0 : -1;
    }

    private IEnumerator FadeScreen(float targetAlpha, float duration)
    {
        if (!useFadeTransition)
        {
            SetFadeAlpha(0f);
            yield break;
        }

        SpriteRenderer renderer = EnsureFadeRenderer();
        if (renderer == null)
        {
            yield break;
        }

        float startAlpha = renderer.color.a;
        if (duration <= 0f)
        {
            SetFadeAlpha(targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }

    private SpriteRenderer EnsureFadeRenderer()
    {
        Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
        if (cameraToUse == null || !cameraToUse.orthographic)
        {
            return null;
        }

        if (fadeRenderer == null)
        {
            GameObject fadeObject = new GameObject("Runtime Fade Overlay");
            fadeObject.transform.SetParent(transform, false);
            fadeRenderer = fadeObject.AddComponent<SpriteRenderer>();
            fadeRenderer.sprite = GetFadeSprite();
            fadeRenderer.sortingOrder = 10000;
        }

        float height = cameraToUse.orthographicSize * 2f;
        float width = height * cameraToUse.aspect;
        Vector3 cameraPosition = cameraToUse.transform.position;
        fadeRenderer.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);
        fadeRenderer.transform.localScale = new Vector3(width, height, 1f);
        return fadeRenderer;
    }

    private Sprite GetFadeSprite()
    {
        if (fadeSprite != null)
        {
            return fadeSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        fadeSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return fadeSprite;
    }

    private void SetFadeAlpha(float alpha)
    {
        SpriteRenderer renderer = EnsureFadeRenderer();
        if (renderer == null)
        {
            return;
        }

        renderer.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
    }


    private void MoveCameraToCurrentFrame()
    {
        if (!moveCameraToActiveFrame || frames[currentFrameIndex] == null || frames[currentFrameIndex].root == null)
        {
            return;
        }

        Camera cameraToMove = targetCamera != null ? targetCamera : Camera.main;
        if (cameraToMove == null)
        {
            return;
        }

        Vector3 framePosition = frames[currentFrameIndex].root.position;
        Vector3 cameraPosition = cameraToMove.transform.position;
        cameraToMove.transform.position = new Vector3(framePosition.x, framePosition.y, cameraPosition.z);
    }

    [ContextMenu("Fit All Backgrounds To Camera")]
    public void FitAllBackgroundsToCamera()
    {
        RefreshFramesFromHierarchy();

        if (frames == null)
        {
            return;
        }

        for (int i = 0; i < frames.Length; i++)
        {
            FitBackgroundToCamera(frames[i]);
        }
    }

    private void FitCurrentBackgroundToCamera()
    {
        if (!autoFitBackgroundToCamera || frames == null || currentFrameIndex < 0 || currentFrameIndex >= frames.Length)
        {
            return;
        }

        FitBackgroundToCamera(frames[currentFrameIndex]);
    }

    private void FitBackgroundToCamera(HallwayFrame frame)
    {
        if (frame == null || frame.background == null || frame.background.sprite == null)
        {
            return;
        }

        Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
        if (cameraToUse == null || !cameraToUse.orthographic)
        {
            return;
        }

        // Fit contain: hien tron anh trong camera, khong bi cat vien.
        Vector2 spriteSize = frame.background.sprite.bounds.size;
        float cameraHeight = cameraToUse.orthographicSize * 2f * fitScreenPercent;
        float cameraWidth = cameraHeight * cameraToUse.aspect;
        float scale = Mathf.Min(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y);

        frame.background.transform.localScale = new Vector3(scale, scale, 1f);
        frame.background.transform.localPosition = new Vector3(0f, 0f, frame.background.transform.localPosition.z);
    }

    public void RefreshFramesFromHierarchy()
    {
        if (framesRoot == null)
        {
            Transform foundRoot = transform.Find("Frames");
            if (foundRoot != null)
            {
                framesRoot = foundRoot;
            }
        }

        if (framesRoot == null)
        {
            return;
        }

        int childCount = framesRoot.childCount;
        frames = new HallwayFrame[childCount];
        for (int i = 0; i < childCount; i++)
        {
            Transform frameRoot = framesRoot.GetChild(i);
            frames[i] = new HallwayFrame
            {
                root = frameRoot,
                background = FindSpriteRenderer(frameRoot, "PreviewBackground"),
                nextArrow = FindArrow(frameRoot)
            };
        }
    }

    private SpriteRenderer FindSpriteRenderer(Transform root, string childName)
    {
        Transform child = root.Find(childName);
        return child != null ? child.GetComponent<SpriteRenderer>() : null;
    }

    private HallwayArrowHotspot FindArrow(Transform root)
    {
        return root.GetComponentInChildren<HallwayArrowHotspot>(true);
    }
}

[System.Serializable]
public class HallwayFrame
{
    public Transform root;
    public SpriteRenderer background;
    public HallwayArrowHotspot nextArrow;
}
