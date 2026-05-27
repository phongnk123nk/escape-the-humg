using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Mui ten click de sang man tiep theo trong hanh lang.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
[AddComponentMenu("Hallway/Hallway Arrow Hotspot")]
public class HallwayArrowHotspot : MonoBehaviour
{
    private const string DefaultStreamingAssetsVideoFileName = "video vap.MOV";

    public HallwayHotspotAction action = HallwayHotspotAction.GoNextFrame;
    public HallwayImageNavigator navigator;
    public VideoClip videoClip;
    [Tooltip("Ten file video trong Assets/StreamingAssets. Dung de build sang may khac van phat duoc video.")]
    public string streamingAssetsVideoFileName = "video vap.MOV";
    public string nextSceneName = "room1";
    public BoxCollider2D boxCollider;
    public SpriteRenderer spriteRenderer;

    [Header("Arrow Float Animation")]
    public bool enableFloatAnimation = true;
    [Min(0f)]
    public float floatAmplitude = 0.12f;
    [Min(0f)]
    public float floatSpeed = 2f;

    private Vector3 baseLocalPosition;

    private void OnEnable()
    {
        baseLocalPosition = transform.localPosition;
    }

    private void Reset()
    {
        CacheComponents();
        boxCollider.isTrigger = true;
    }

    private void Awake()
    {
        CacheComponents();
        FindNavigator();
        EnsureDefaultStreamingVideoName();
    }

    private void OnValidate()
    {
        CacheComponents();
        EnsureDefaultStreamingVideoName();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying || !enableFloatAnimation || action == HallwayHotspotAction.LoadScene)
        {
            return;
        }

        if (spriteRenderer != null && !spriteRenderer.enabled)
        {
            return;
        }

        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = baseLocalPosition + new Vector3(0f, offsetY, 0f);
    }

    private void OnMouseDown()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        FindNavigator();
        if (action == HallwayHotspotAction.LoadScene)
        {
            LoadNextScene();
            return;
        }

        if (navigator == null)
        {
            return;
        }

        if (action == HallwayHotspotAction.PlayVideoThenNextFrame)
        {
            EnsureDefaultStreamingVideoName();
            navigator.PlayVideoThenNext(videoClip, streamingAssetsVideoFileName);
            return;
        }

        navigator.GoNext();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        if (currentIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
            return;
        }

        Debug.LogWarning("Chua gan nextSceneName cho vung click cuoi hanh lang.", this);
    }

    private void CacheComponents()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void FindNavigator()
    {
        if (navigator != null)
        {
            return;
        }

        navigator = GetComponentInParent<HallwayImageNavigator>();
        if (navigator == null)
        {
            navigator = FindFirstObjectByType<HallwayImageNavigator>();
        }
    }

    private void EnsureDefaultStreamingVideoName()
    {
        if (action == HallwayHotspotAction.PlayVideoThenNextFrame && string.IsNullOrWhiteSpace(streamingAssetsVideoFileName))
        {
            streamingAssetsVideoFileName = DefaultStreamingAssetsVideoFileName;
        }
    }
}

public enum HallwayHotspotAction
{
    GoNextFrame,
    PlayVideoThenNextFrame,
    LoadScene
}
