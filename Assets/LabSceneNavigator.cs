using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Lab/Lab Scene Navigator")]
public class LabSceneNavigator : MonoBehaviour
{
    private const int DefaultBackgroundOrder = -100;
    private const int DefaultHotspotOrder = 50;
    private const int DefaultFadeOrder = 1000;

    private static readonly string[] DefaultScreenNames =
    {
        "MainLab",
        "BackView",
        "CabinetView",
        "CabinetOpenView",
        "MachineView",
        "TableView"
    };

    [Header("Quick Setup")]
    [Tooltip("Keo 6 anh nen theo dung thu tu: MainLab, BackView, CabinetView, CabinetOpenView, MachineView, TableView.")]
    [SerializeField] private List<Sprite> backgroundsInOrder = new List<Sprite>();

    [Tooltip("Sprite mui ten mac dinh. Neu tung hotspot khong gan sprite rieng thi script dung sprite nay.")]
    [SerializeField] private Sprite defaultArrowSprite;

    [Tooltip("Neu bat, script tu tao san 6 man va luong di chuyen mac dinh trong Inspector.")]
    [SerializeField] private bool autoCreateDefaultScreens = true;

    [Tooltip("Neu bat, danh sach backgroundsInOrder se tu gan vao cac man theo dung thu tu mac dinh.")]
    [SerializeField] private bool autoAssignOrderedBackgrounds = true;

    [Header("Scene Setup")]
    [SerializeField] private string initialScreenName = "MainLab";
    [SerializeField] private int initialScreenIndex = 0;
    [SerializeField] private List<LabScreen> screens = new List<LabScreen>();

    [Header("Edit Mode Preview")]
    [Tooltip("Bat cai nay de thay anh nen va cac mui ten ngay trong Scene View khi chua Play.")]
    [SerializeField] private bool showEditModePreview = true;

    [Tooltip("Bat cai nay de hien tat ca cac man cung luc trong Scene View theo dang luoi.")]
    [SerializeField] private bool showAllScreensInEditMode = true;

    [Tooltip("Man dang hien trong Scene View de chinh vi tri. 0=MainLab, 1=BackView, 2=CabinetView, 3=CabinetOpenView, 4=MachineView, 5=TableView.")]
    [SerializeField] private int editPreviewScreenIndex = 0;

    [Tooltip("So cot khi hien tat ca cac man trong Scene View.")]
    [SerializeField, Min(1)] private int editPreviewColumns = 3;

    [Tooltip("Khoang cach them giua cac man preview.")]
    [SerializeField, Min(0f)] private float editPreviewPadding = 1.5f;

    [Tooltip("Neu bat, khi keo/scale/rotate cac object PREVIEW trong Scene View, script tu ghi nguoc lai vao cau hinh hotspot.")]
    [SerializeField] private bool syncPreviewTransformToInspector = true;

    [Tooltip("Hien cac vung click dac biet dang invisible bang o mau vang trong Edit Mode.")]
    [SerializeField] private bool showInteractionHotspotsInEditMode = true;

    [SerializeField] private Color interactionPreviewTint = new Color(1f, 0.76f, 0.12f, 0.28f);

    [Header("Rendering")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private string sortingLayerName = "";
    [SerializeField] private int backgroundSortingOrder = DefaultBackgroundOrder;
    [SerializeField] private int hotspotSortingOrder = DefaultHotspotOrder;
    [SerializeField] private bool fitBackgroundToCamera = true;
    [SerializeField] private bool coverCameraInsteadOfFitInside = true;
    [SerializeField] private Color backgroundTint = Color.white;

    [Header("Fade")]
    [SerializeField] private bool useFade = true;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private bool useUnscaledTimeForFade = false;

    [Header("Runtime Events")]
    [SerializeField] private StringEvent onScreenChanged = new StringEvent();
    [SerializeField] private StringEvent onHotspotClicked = new StringEvent();

    private readonly List<RuntimeHotspot> activeHotspots = new List<RuntimeHotspot>();
    private readonly Dictionary<string, int> screenIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    private GameObject backgroundObject;
    private SpriteRenderer backgroundRenderer;
    private GameObject hotspotRoot;
    private GameObject fadeObject;
    private SpriteRenderer fadeRenderer;
    private Texture2D fadeTexture;
    private Sprite fadeSprite;
    private Texture2D previewTexture;
    private Sprite previewSprite;
    private Coroutine transitionCoroutine;
    private int currentScreenIndex = -1;
    private bool isTransitioning;
    private bool isRebuildingEditPreview;
    private GameObject editPreviewRoot;
    private readonly List<SpriteRenderer> editPreviewBackgroundRenderers = new List<SpriteRenderer>();
    private readonly List<Transform> editPreviewScreenRoots = new List<Transform>();

#if UNITY_EDITOR
    private bool editPreviewRebuildQueued;
#endif

    public string CurrentScreenName
    {
        get
        {
            if (currentScreenIndex < 0 || currentScreenIndex >= screens.Count)
            {
                return string.Empty;
            }

            return screens[currentScreenIndex].screenName;
        }
    }

    private void Reset()
    {
        backgroundsInOrder = CreateEmptyBackgroundSlots();
        initialScreenName = "MainLab";
        initialScreenIndex = 0;
        autoCreateDefaultScreens = true;
        autoAssignOrderedBackgrounds = true;
        fitBackgroundToCamera = true;
        coverCameraInsteadOfFitInside = true;
        showEditModePreview = true;
        showAllScreensInEditMode = true;
        editPreviewScreenIndex = 0;
        editPreviewColumns = 3;
        editPreviewPadding = 1.5f;
        syncPreviewTransformToInspector = true;
        showInteractionHotspotsInEditMode = true;
        useFade = true;
        fadeDuration = 0.25f;
        screens = CreateDefaultScreens();
    }

    private void OnValidate()
    {
        if (backgroundsInOrder == null)
        {
            backgroundsInOrder = CreateEmptyBackgroundSlots();
        }

        while (backgroundsInOrder.Count < DefaultScreenNames.Length)
        {
            backgroundsInOrder.Add(null);
        }

        if (screens == null)
        {
            screens = new List<LabScreen>();
        }

        if (autoCreateDefaultScreens && screens.Count == 0)
        {
            screens = CreateDefaultScreens();
        }

        if (autoAssignOrderedBackgrounds)
        {
            ApplyOrderedBackgroundsToScreens();
        }

        fadeDuration = Mathf.Max(0.01f, fadeDuration);
        editPreviewScreenIndex = Mathf.Max(0, editPreviewScreenIndex);
        editPreviewColumns = Mathf.Max(1, editPreviewColumns);
        editPreviewPadding = Mathf.Max(0f, editPreviewPadding);

        if (screens.Count > 0)
        {
            editPreviewScreenIndex = Mathf.Clamp(editPreviewScreenIndex, 0, screens.Count - 1);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            QueueEditModePreviewRebuild();
        }
#endif
    }

    private void Awake()
    {
        if (screens == null || screens.Count == 0)
        {
            screens = CreateDefaultScreens();
        }

        if (autoAssignOrderedBackgrounds)
        {
            ApplyOrderedBackgroundsToScreens();
        }

        CacheScreenLookup();
        EnsureCamera();

        if (!Application.isPlaying)
        {
            RebuildEditModePreview();
            return;
        }

        HideLeakedEditModePreviewObjects();
        EnsureBackgroundRenderer();
        EnsureHotspotRoot();
        EnsureFadeRenderer();
    }

    private void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        int startIndex = FindInitialScreenIndex();
        ShowScreenImmediate(startIndex, false);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateEditModePreview();
            return;
        }

        AnimateHotspots();
        HandlePointerInput();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            if (showEditModePreview)
            {
                SyncPreviewTransformChanges();

                if (fitBackgroundToCamera)
                {
                    if (showAllScreensInEditMode)
                    {
                        FitAllEditPreviewBackgrounds();
                    }
                    else
                    {
                        FitSpriteRendererToCamera(backgroundRenderer, coverCameraInsteadOfFitInside);
                    }
                }
            }

            return;
        }

        if (fitBackgroundToCamera)
        {
            FitSpriteRendererToCamera(backgroundRenderer, coverCameraInsteadOfFitInside);
        }

        FitFadeToCamera();
    }

    private void OnDestroy()
    {
        ClearHotspots();

        if (fadeSprite != null)
        {
            DestroyGeneratedObject(fadeSprite);
        }

        if (fadeTexture != null)
        {
            DestroyGeneratedObject(fadeTexture);
        }

        if (previewSprite != null)
        {
            DestroyGeneratedObject(previewSprite);
        }

        if (previewTexture != null)
        {
            DestroyGeneratedObject(previewTexture);
        }
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ClearEditModePreviewObjects();
        }
#endif
    }

    [ContextMenu("Rebuild Default Lab Screens")]
    private void RebuildDefaultLabScreens()
    {
        screens = CreateDefaultScreens();
        ApplyOrderedBackgroundsToScreens();
        CacheScreenLookup();

        if (!Application.isPlaying)
        {
            RebuildEditModePreview();
        }
    }

    [ContextMenu("Refresh Edit Mode Preview")]
    private void RefreshEditModePreview()
    {
        if (Application.isPlaying)
        {
            Debug.Log("LabSceneNavigator: Edit Mode Preview chi dung khi chua Play.", this);
            return;
        }

        RebuildEditModePreview();
    }

    [ContextMenu("Go To Initial Screen")]
    private void GoToInitialScreenFromContextMenu()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("LabSceneNavigator: chi co the chuyen man khi dang Play Mode.", this);
            return;
        }

        GoToScreen(initialScreenName);
    }

    public void GoToScreen(string screenName)
    {
        int targetIndex = FindScreenIndex(screenName);
        if (targetIndex < 0)
        {
            Debug.LogWarning("LabSceneNavigator: khong tim thay man '" + screenName + "'.", this);
            return;
        }

        GoToScreen(targetIndex);
    }

    public void GoToScreen(int screenIndex)
    {
        if (screenIndex < 0 || screenIndex >= screens.Count)
        {
            Debug.LogWarning("LabSceneNavigator: screenIndex khong hop le: " + screenIndex, this);
            return;
        }

        if (isTransitioning)
        {
            return;
        }

        if (!useFade)
        {
            ShowScreenImmediate(screenIndex, true);
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(FadeAndSwitch(screenIndex));
    }

    private IEnumerator FadeAndSwitch(int screenIndex)
    {
        isTransitioning = true;
        EnsureFadeRenderer();

        yield return FadeOverlay(0f, 1f);
        ShowScreenImmediate(screenIndex, true);
        yield return FadeOverlay(1f, 0f);

        isTransitioning = false;
        transitionCoroutine = null;
    }

    private IEnumerator FadeOverlay(float fromAlpha, float toAlpha)
    {
        if (fadeRenderer == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Color color = fadeColor;

        while (elapsed < fadeDuration)
        {
            elapsed += useUnscaledTimeForFade ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            color.a = Mathf.Lerp(fromAlpha, toAlpha, Smooth01(t));
            fadeRenderer.color = color;
            yield return null;
        }

        color.a = toAlpha;
        fadeRenderer.color = color;
    }

    private void ShowScreenImmediate(int screenIndex, bool invokeEvent)
    {
        ShowScreenImmediate(screenIndex, invokeEvent, false);
    }

    private void ShowScreenImmediate(int screenIndex, bool invokeEvent, bool editPreview)
    {
        if (screenIndex < 0 || screenIndex >= screens.Count)
        {
            return;
        }

        currentScreenIndex = screenIndex;
        LabScreen screen = screens[currentScreenIndex];

        EnsureBackgroundRenderer();
        backgroundObject.name = (editPreview ? "PREVIEW Background - " : "Lab Background - ") + SafeName(screen.screenName, currentScreenIndex);
        backgroundRenderer.sprite = screen.background;
        backgroundRenderer.color = backgroundTint;
        ApplySorting(backgroundRenderer, backgroundSortingOrder);

        if (fitBackgroundToCamera)
        {
            FitSpriteRendererToCamera(backgroundRenderer, coverCameraInsteadOfFitInside);
        }

        ClearHotspots();
        CreateHotspotsForScreen(screen, editPreview);

        if (invokeEvent && !editPreview)
        {
            onScreenChanged.Invoke(screen.screenName);
        }
    }

    private void CreateHotspotsForScreen(LabScreen screen, bool editPreview)
    {
        Transform parent = hotspotRoot != null ? hotspotRoot.transform : transform;
        CreateHotspotsForScreen(screen, editPreview, parent, parent, currentScreenIndex);
    }

    private void CreateHotspotsForScreen(LabScreen screen, bool editPreview, Transform parent, Transform previewScreenRoot, int screenIndex)
    {
        if (screen == null || screen.hotspots == null)
        {
            return;
        }

        for (int i = 0; i < screen.hotspots.Count; i++)
        {
            LabHotspot hotspot = screen.hotspots[i];
            if (hotspot == null || !hotspot.enabled)
            {
                continue;
            }

            CreateHotspot(screen, hotspot, i, editPreview, parent, previewScreenRoot, screenIndex);
        }
    }

    private void CreateHotspot(LabScreen screen, LabHotspot hotspot, int index, bool editPreview, Transform parent, Transform previewScreenRoot, int screenIndex)
    {
        GameObject hotspotObject = new GameObject((editPreview ? "PREVIEW " : "") + SafeName(hotspot.hotspotName, index));
        ApplyGeneratedObjectFlags(hotspotObject);
        hotspotObject.transform.SetParent(parent, false);

        Vector3 configWorldPosition = ResolvePosition(hotspot.position, hotspot.positionSpace, hotspot.zOffset);
        if (editPreview)
        {
            hotspotObject.transform.localPosition = configWorldPosition;
            hotspotObject.transform.localRotation = Quaternion.Euler(0f, 0f, hotspot.rotationZ);
        }
        else
        {
            hotspotObject.transform.position = configWorldPosition;
            hotspotObject.transform.rotation = Quaternion.Euler(0f, 0f, hotspot.rotationZ);
        }

        Sprite sprite = ResolveHotspotSprite(hotspot);
        bool usesInteractionPreviewMarker = editPreview && hotspot.kind == LabHotspotKind.Interaction && showInteractionHotspotsInEditMode && sprite == null;
        if (usesInteractionPreviewMarker)
        {
            sprite = EnsurePreviewSprite();
            hotspotObject.transform.localScale = new Vector3(Mathf.Max(0.05f, hotspot.colliderSize.x), Mathf.Max(0.05f, hotspot.colliderSize.y), 1f);
        }
        else
        {
            hotspotObject.transform.localScale = new Vector3(hotspot.scale.x, hotspot.scale.y, 1f);
        }

        SpriteRenderer spriteRenderer = null;
        bool shouldRender = hotspot.visible || usesInteractionPreviewMarker;

        if (shouldRender && sprite != null)
        {
            spriteRenderer = hotspotObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = usesInteractionPreviewMarker ? interactionPreviewTint : hotspot.tint;
            ApplySorting(spriteRenderer, hotspot.useCustomSortingOrder ? hotspot.sortingOrder : hotspotSortingOrder + index);
        }

        BoxCollider2D collider = hotspotObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;

        if (usesInteractionPreviewMarker)
        {
            collider.size = Vector2.one;
        }
        else if (hotspot.useSpriteBoundsAsCollider && sprite != null)
        {
            collider.size = sprite.bounds.size;
        }
        else
        {
            Vector2 safeScale = new Vector2(Mathf.Max(0.0001f, Mathf.Abs(hotspot.scale.x)), Mathf.Max(0.0001f, Mathf.Abs(hotspot.scale.y)));
            collider.size = new Vector2(hotspot.colliderSize.x / safeScale.x, hotspot.colliderSize.y / safeScale.y);
        }

        if (editPreview)
        {
            Color fill = hotspot.kind == LabHotspotKind.Arrow
                ? new Color(0.2f, 0.75f, 1f, 0.16f)
                : new Color(1f, 0.72f, 0.05f, 0.22f);
            Color line = hotspot.kind == LabHotspotKind.Arrow
                ? new Color(0.1f, 0.85f, 1f, 0.95f)
                : new Color(1f, 0.72f, 0.05f, 0.95f);
            LabBoxColliderPreview.Attach(collider, fill, line, hotspot.hotspotName);
        }

        RuntimeHotspot runtimeHotspot = new RuntimeHotspot
        {
            config = hotspot,
            screen = screen,
            gameObject = hotspotObject,
            renderer = spriteRenderer,
            collider = collider,
            basePosition = hotspotObject.transform.position,
            baseScale = hotspotObject.transform.localScale,
            moveDirection = ResolveAnimationDirection(hotspot),
            hotspotIndex = index,
            screenIndex = screenIndex,
            editPreview = editPreview,
            usesInteractionPreviewMarker = usesInteractionPreviewMarker,
            previewScreenRoot = previewScreenRoot
        };

        RememberTransform(runtimeHotspot);
        hotspotObject.transform.hasChanged = false;
        activeHotspots.Add(runtimeHotspot);
    }

    private void ClearHotspots()
    {
        for (int i = 0; i < activeHotspots.Count; i++)
        {
            RuntimeHotspot hotspot = activeHotspots[i];
            if (hotspot != null && hotspot.gameObject != null)
            {
                // Trong build, Destroy bi delay toi cuoi frame. An ngay truoc khi destroy
                // de tranh mui ten cu bi sot lai phia duoi mui ten that sau khi doi goc nhin.
                hotspot.gameObject.SetActive(false);
                DestroyGeneratedObject(hotspot.gameObject);
            }
        }

        activeHotspots.Clear();
    }

    private void UpdateEditModePreview()
    {
        if (!showEditModePreview)
        {
            ClearEditModePreviewObjects();
            return;
        }

        if (screens == null || screens.Count == 0)
        {
            return;
        }

        if (showAllScreensInEditMode)
        {
            bool needsAllScreensRebuild = editPreviewRoot == null
                || editPreviewScreenRoots.Count != screens.Count
                || activeHotspots.Count != CountAllEnabledHotspots()
                || MissingAnyHotspotObject();

            if (needsAllScreensRebuild)
            {
                RebuildEditModePreview();
                return;
            }

            RefreshAllEditPreviewSprites();
            SyncPreviewTransformChanges();
            return;
        }

        if (editPreviewRoot != null)
        {
            ClearEditModePreviewObjects();
        }

        int clampedIndex = Mathf.Clamp(editPreviewScreenIndex, 0, screens.Count - 1);
        if (editPreviewScreenIndex != clampedIndex)
        {
            editPreviewScreenIndex = clampedIndex;
        }

        LabScreen screen = screens[editPreviewScreenIndex];
        bool needsRebuild = backgroundRenderer == null
            || hotspotRoot == null
            || currentScreenIndex != editPreviewScreenIndex
            || activeHotspots.Count != CountEnabledHotspots(screen)
            || MissingAnyHotspotObject();

        if (needsRebuild)
        {
            RebuildEditModePreview();
            return;
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = screen.background;
            backgroundRenderer.color = backgroundTint;
            ApplySorting(backgroundRenderer, backgroundSortingOrder);
        }

        SyncPreviewTransformChanges();
    }

    private void RebuildEditModePreview()
    {
        if (Application.isPlaying || isRebuildingEditPreview)
        {
            return;
        }

        if (!showEditModePreview)
        {
            ClearEditModePreviewObjects();
            return;
        }

        if (screens == null || screens.Count == 0)
        {
            return;
        }

        isRebuildingEditPreview = true;

        CacheScreenLookup();
        EnsureCamera();

        if (showAllScreensInEditMode)
        {
            RebuildAllScreensEditPreview();
            isRebuildingEditPreview = false;
            return;
        }

        if (editPreviewRoot != null)
        {
            ClearEditModePreviewObjects();
        }

        EnsureBackgroundRenderer();
        EnsureHotspotRoot();

        editPreviewScreenIndex = Mathf.Clamp(editPreviewScreenIndex, 0, screens.Count - 1);
        ShowScreenImmediate(editPreviewScreenIndex, false, true);

        isRebuildingEditPreview = false;
    }

    private void ClearEditModePreviewObjects()
    {
        ClearHotspots();
        editPreviewBackgroundRenderers.Clear();
        editPreviewScreenRoots.Clear();

        if (editPreviewRoot != null)
        {
            DestroyGeneratedObject(editPreviewRoot);
            editPreviewRoot = null;
        }

        if (hotspotRoot != null)
        {
            DestroyGeneratedObject(hotspotRoot);
            hotspotRoot = null;
        }

        if (backgroundObject != null)
        {
            DestroyGeneratedObject(backgroundObject);
            backgroundObject = null;
            backgroundRenderer = null;
        }
    }

    private void HideLeakedEditModePreviewObjects()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child == null || child == transform)
            {
                continue;
            }

            if (child.name.StartsWith("PREVIEW ") || child.name.StartsWith("PREVIEW_"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void RebuildAllScreensEditPreview()
    {
        ClearEditModePreviewObjects();

        editPreviewRoot = new GameObject("PREVIEW All Lab Screens");
        ApplyGeneratedObjectFlags(editPreviewRoot);
        editPreviewRoot.transform.SetParent(transform, false);
        editPreviewRoot.transform.localPosition = Vector3.zero;

        for (int i = 0; i < screens.Count; i++)
        {
            LabScreen screen = screens[i];
            if (screen == null)
            {
                continue;
            }

            Vector3 screenOrigin = GetEditPreviewScreenOrigin(i);
            GameObject screenRoot = new GameObject("PREVIEW " + i + " - " + SafeName(screen.screenName, i));
            ApplyGeneratedObjectFlags(screenRoot);
            screenRoot.transform.SetParent(editPreviewRoot.transform, false);
            screenRoot.transform.localPosition = screenOrigin;
            editPreviewScreenRoots.Add(screenRoot.transform);

            CreateEditPreviewBackground(screen, i, screenRoot.transform);
            CreateHotspotsForScreen(screen, true, screenRoot.transform, screenRoot.transform, i);
        }

        currentScreenIndex = -1;
    }

    private void CreateEditPreviewBackground(LabScreen screen, int screenIndex, Transform screenRoot)
    {
        GameObject previewBackground = new GameObject("PREVIEW Background - " + SafeName(screen.screenName, screenIndex));
        ApplyGeneratedObjectFlags(previewBackground);
        previewBackground.transform.SetParent(screenRoot, false);

        SpriteRenderer renderer = previewBackground.AddComponent<SpriteRenderer>();
        renderer.sprite = screen.background;
        renderer.color = backgroundTint;
        ApplySorting(renderer, backgroundSortingOrder);

        editPreviewBackgroundRenderers.Add(renderer);

        if (fitBackgroundToCamera)
        {
            FitSpriteRendererToCamera(renderer, coverCameraInsteadOfFitInside, screenRoot.position);
        }
    }

    private void RefreshAllEditPreviewSprites()
    {
        for (int i = 0; i < editPreviewBackgroundRenderers.Count && i < screens.Count; i++)
        {
            SpriteRenderer renderer = editPreviewBackgroundRenderers[i];
            if (renderer == null || screens[i] == null)
            {
                continue;
            }

            renderer.sprite = screens[i].background;
            renderer.color = backgroundTint;
            ApplySorting(renderer, backgroundSortingOrder);
        }
    }

    private void FitAllEditPreviewBackgrounds()
    {
        int count = Mathf.Min(editPreviewBackgroundRenderers.Count, editPreviewScreenRoots.Count);
        for (int i = 0; i < count; i++)
        {
            SpriteRenderer renderer = editPreviewBackgroundRenderers[i];
            Transform screenRoot = editPreviewScreenRoots[i];
            if (renderer == null || screenRoot == null)
            {
                continue;
            }

            FitSpriteRendererToCamera(renderer, coverCameraInsteadOfFitInside, screenRoot.position);
        }
    }

    private void SyncPreviewTransformChanges()
    {
        if (Application.isPlaying || !showEditModePreview || !syncPreviewTransformToInspector || isRebuildingEditPreview)
        {
            return;
        }

        bool changedAny = false;

        for (int i = 0; i < activeHotspots.Count; i++)
        {
            RuntimeHotspot runtimeHotspot = activeHotspots[i];
            if (runtimeHotspot == null || !runtimeHotspot.editPreview || runtimeHotspot.gameObject == null || runtimeHotspot.config == null)
            {
                continue;
            }

            Transform hotspotTransform = runtimeHotspot.gameObject.transform;
            if (!hotspotTransform.hasChanged && TransformApproximatelyEquals(runtimeHotspot, hotspotTransform))
            {
                continue;
            }

            changedAny = true;
#if UNITY_EDITOR
            Undo.RecordObject(this, "Move Lab Hotspot");
#endif
            WritePreviewTransformToConfig(runtimeHotspot, hotspotTransform);
            RememberTransform(runtimeHotspot);
            hotspotTransform.hasChanged = false;
        }

#if UNITY_EDITOR
        if (changedAny)
        {
            EditorUtility.SetDirty(this);

            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
#endif
    }

    private void WritePreviewTransformToConfig(RuntimeHotspot runtimeHotspot, Transform hotspotTransform)
    {
        LabHotspot hotspot = runtimeHotspot.config;
        Vector3 configWorldPosition = hotspotTransform.position;
        if (runtimeHotspot.previewScreenRoot != null)
        {
            configWorldPosition = runtimeHotspot.previewScreenRoot.InverseTransformPoint(hotspotTransform.position);
        }

        if (hotspot.positionSpace == LabPositionSpace.Viewport && targetCamera != null)
        {
            Vector3 viewportPosition = targetCamera.WorldToViewportPoint(configWorldPosition);
            hotspot.position = new Vector2(viewportPosition.x, viewportPosition.y);
        }
        else
        {
            hotspot.position = new Vector2(configWorldPosition.x, configWorldPosition.y);
        }

        hotspot.zOffset = configWorldPosition.z;
        hotspot.rotationZ = Mathf.DeltaAngle(0f, hotspotTransform.localEulerAngles.z);

        if (runtimeHotspot.usesInteractionPreviewMarker)
        {
            hotspot.scale = Vector2.one;
            hotspot.colliderSize = new Vector2(Mathf.Max(0.05f, Mathf.Abs(hotspotTransform.localScale.x)), Mathf.Max(0.05f, Mathf.Abs(hotspotTransform.localScale.y)));
        }
        else
        {
            hotspot.scale = new Vector2(hotspotTransform.localScale.x, hotspotTransform.localScale.y);
        }

        runtimeHotspot.basePosition = hotspotTransform.position;
        runtimeHotspot.baseScale = hotspotTransform.localScale;
        runtimeHotspot.moveDirection = ResolveAnimationDirection(hotspot);
    }

    private bool TransformApproximatelyEquals(RuntimeHotspot runtimeHotspot, Transform hotspotTransform)
    {
        return Vector3.SqrMagnitude(runtimeHotspot.lastPosition - hotspotTransform.position) <= 0.000001f
            && Vector3.SqrMagnitude(runtimeHotspot.lastScale - hotspotTransform.localScale) <= 0.000001f
            && Mathf.Abs(Mathf.DeltaAngle(runtimeHotspot.lastRotationZ, hotspotTransform.eulerAngles.z)) <= 0.01f;
    }

    private void RememberTransform(RuntimeHotspot runtimeHotspot)
    {
        if (runtimeHotspot == null || runtimeHotspot.gameObject == null)
        {
            return;
        }

        Transform hotspotTransform = runtimeHotspot.gameObject.transform;
        runtimeHotspot.lastPosition = hotspotTransform.position;
        runtimeHotspot.lastScale = hotspotTransform.localScale;
        runtimeHotspot.lastRotationZ = hotspotTransform.eulerAngles.z;
    }

    private int CountEnabledHotspots(LabScreen screen)
    {
        if (screen == null || screen.hotspots == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < screen.hotspots.Count; i++)
        {
            if (screen.hotspots[i] != null && screen.hotspots[i].enabled)
            {
                count++;
            }
        }

        return count;
    }

    private int CountAllEnabledHotspots()
    {
        if (screens == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < screens.Count; i++)
        {
            count += CountEnabledHotspots(screens[i]);
        }

        return count;
    }

    private bool MissingAnyHotspotObject()
    {
        for (int i = 0; i < activeHotspots.Count; i++)
        {
            if (activeHotspots[i] == null || activeHotspots[i].gameObject == null)
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 GetEditPreviewScreenOrigin(int screenIndex)
    {
        int columns = Mathf.Max(1, editPreviewColumns);
        int column = screenIndex % columns;
        int row = screenIndex / columns;

        float width = GetCameraWorldWidth() + editPreviewPadding;
        float height = GetCameraWorldHeight() + editPreviewPadding;

        return new Vector3(column * width, -row * height, 0f);
    }

    private float GetCameraWorldWidth()
    {
        EnsureCamera();
        if (targetCamera != null && targetCamera.orthographic)
        {
            return targetCamera.orthographicSize * 2f * targetCamera.aspect;
        }

        return 18f;
    }

    private float GetCameraWorldHeight()
    {
        EnsureCamera();
        if (targetCamera != null && targetCamera.orthographic)
        {
            return targetCamera.orthographicSize * 2f;
        }

        return 10f;
    }

    private void AnimateHotspots()
    {
        if (activeHotspots.Count == 0)
        {
            return;
        }

        float time = Time.time;

        for (int i = 0; i < activeHotspots.Count; i++)
        {
            RuntimeHotspot runtimeHotspot = activeHotspots[i];
            if (runtimeHotspot == null || runtimeHotspot.gameObject == null || runtimeHotspot.config == null)
            {
                continue;
            }

            LabHotspot hotspot = runtimeHotspot.config;
            if (!hotspot.animate || hotspot.animationMode == LabHotspotAnimation.None)
            {
                runtimeHotspot.gameObject.transform.position = runtimeHotspot.basePosition;
                runtimeHotspot.gameObject.transform.localScale = runtimeHotspot.baseScale;
                continue;
            }

            float wave = Mathf.Sin((time + hotspot.animationPhase) * hotspot.animationSpeed);
            Vector3 positionOffset = Vector3.zero;
            Vector3 scale = runtimeHotspot.baseScale;

            if (hotspot.animationMode == LabHotspotAnimation.FloatAlongDirection)
            {
                positionOffset = new Vector3(runtimeHotspot.moveDirection.x, runtimeHotspot.moveDirection.y, 0f) * (wave * hotspot.animationDistance);
            }
            else if (hotspot.animationMode == LabHotspotAnimation.ShakeSideways)
            {
                Vector2 side = new Vector2(-runtimeHotspot.moveDirection.y, runtimeHotspot.moveDirection.x);
                positionOffset = new Vector3(side.x, side.y, 0f) * (wave * hotspot.animationDistance);
            }
            else if (hotspot.animationMode == LabHotspotAnimation.Pulse)
            {
                float pulse = 1f + wave * hotspot.pulseAmount;
                scale = new Vector3(runtimeHotspot.baseScale.x * pulse, runtimeHotspot.baseScale.y * pulse, runtimeHotspot.baseScale.z);
            }

            runtimeHotspot.gameObject.transform.position = runtimeHotspot.basePosition + positionOffset;
            runtimeHotspot.gameObject.transform.localScale = scale;
        }
    }

    private void HandlePointerInput()
    {
        if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
        {
            return;
        }

        if (isTransitioning || activeHotspots.Count == 0)
        {
            return;
        }

        Vector2 worldPoint;
        if (!TryGetPointerDownWorldPosition(out worldPoint))
        {
            return;
        }

        RuntimeHotspot clickedHotspot = FindClickedHotspot(worldPoint);
        if (clickedHotspot == null)
        {
            return;
        }

        HandleHotspotClicked(clickedHotspot);
    }

    private RuntimeHotspot FindClickedHotspot(Vector2 worldPoint)
    {
        RuntimeHotspot bestHotspot = null;
        int bestPriority = int.MinValue;

        for (int i = 0; i < activeHotspots.Count; i++)
        {
            RuntimeHotspot hotspot = activeHotspots[i];
            if (hotspot == null || hotspot.collider == null || hotspot.config == null)
            {
                continue;
            }

            if (!hotspot.collider.OverlapPoint(worldPoint))
            {
                continue;
            }

            int priority = hotspot.config.clickPriority;
            if (priority >= bestPriority)
            {
                bestPriority = priority;
                bestHotspot = hotspot;
            }
        }

        return bestHotspot;
    }

    private void HandleHotspotClicked(RuntimeHotspot runtimeHotspot)
    {
        LabHotspot hotspot = runtimeHotspot.config;
        onHotspotClicked.Invoke(hotspot.hotspotName);

        if (!hotspot.changeScreenOnClick)
        {
            Debug.Log("LabSceneNavigator: clicked hotspot '" + hotspot.hotspotName + "'.", this);
            return;
        }

        int targetIndex = ResolveTargetScreenIndex(hotspot);
        if (targetIndex < 0)
        {
            Debug.LogWarning("LabSceneNavigator: hotspot '" + hotspot.hotspotName + "' chua co target screen hop le.", this);
            return;
        }

        GoToScreen(targetIndex);
    }

    private int ResolveTargetScreenIndex(LabHotspot hotspot)
    {
        if (!string.IsNullOrWhiteSpace(hotspot.targetScreenName))
        {
            int namedIndex = FindScreenIndex(hotspot.targetScreenName);
            if (namedIndex >= 0)
            {
                return namedIndex;
            }
        }

        if (hotspot.targetScreenIndex >= 0 && hotspot.targetScreenIndex < screens.Count)
        {
            return hotspot.targetScreenIndex;
        }

        return -1;
    }

    private bool TryGetPointerDownWorldPosition(out Vector2 worldPosition)
    {
        Vector2 screenPosition;
        if (!TryGetPointerDownScreenPosition(out screenPosition))
        {
            worldPosition = Vector2.zero;
            return false;
        }

        EnsureCamera();
        if (targetCamera == null)
        {
            worldPosition = Vector2.zero;
            return false;
        }

        Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(targetCamera.transform.position.z)));
        worldPosition = new Vector2(world.x, world.y);
        return true;
    }

    private bool TryGetPointerDownScreenPosition(out Vector2 screenPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null)
        {
            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    screenPosition = touch.position.ReadValue();
                    return true;
                }
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            return true;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                screenPosition = touch.position;
                return true;
            }
        }
#endif

        screenPosition = Vector2.zero;
        return false;
    }

    private Vector3 ResolvePosition(Vector2 position, LabPositionSpace positionSpace, float zOffset)
    {
        if (positionSpace == LabPositionSpace.World)
        {
            return new Vector3(position.x, position.y, zOffset);
        }

        EnsureCamera();
        if (targetCamera == null)
        {
            return new Vector3(position.x, position.y, zOffset);
        }

        Vector3 world = targetCamera.ViewportToWorldPoint(new Vector3(position.x, position.y, Mathf.Abs(targetCamera.transform.position.z)));
        world.z = zOffset;
        return world;
    }

    private Vector2 ResolveAnimationDirection(LabHotspot hotspot)
    {
        Vector2 direction = hotspot.animationDirection;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Quaternion.Euler(0f, 0f, hotspot.rotationZ) * Vector2.up;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Vector2.up;
        }

        return direction.normalized;
    }

    private Sprite ResolveHotspotSprite(LabHotspot hotspot)
    {
        if (hotspot.sprite != null)
        {
            return hotspot.sprite;
        }

        if (hotspot.kind == LabHotspotKind.Arrow)
        {
            return defaultArrowSprite;
        }

        return null;
    }

    private void EnsureCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void EnsureBackgroundRenderer()
    {
        if (backgroundRenderer != null)
        {
            return;
        }

        backgroundObject = new GameObject(Application.isPlaying ? "Lab Background" : "PREVIEW Lab Background");
        ApplyGeneratedObjectFlags(backgroundObject);
        backgroundObject.transform.SetParent(transform, false);
        backgroundObject.transform.localPosition = Vector3.zero;
        backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.color = backgroundTint;
        ApplySorting(backgroundRenderer, backgroundSortingOrder);
    }

    private void EnsureHotspotRoot()
    {
        if (hotspotRoot != null)
        {
            return;
        }

        hotspotRoot = new GameObject(Application.isPlaying ? "Lab Hotspots" : "PREVIEW Lab Hotspots");
        ApplyGeneratedObjectFlags(hotspotRoot);
        hotspotRoot.transform.SetParent(transform, false);
        hotspotRoot.transform.localPosition = Vector3.zero;
    }

    private void EnsureFadeRenderer()
    {
        if (fadeRenderer != null)
        {
            return;
        }

        fadeTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        fadeTexture.name = "LabSceneNavigator Fade Texture";
        fadeTexture.hideFlags = HideFlags.HideAndDontSave;
        fadeTexture.SetPixel(0, 0, Color.white);
        fadeTexture.Apply();

        fadeSprite = Sprite.Create(fadeTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        fadeSprite.name = "LabSceneNavigator Fade Sprite";
        fadeSprite.hideFlags = HideFlags.HideAndDontSave;

        fadeObject = new GameObject("Lab Fade Overlay");
        ApplyGeneratedObjectFlags(fadeObject);
        fadeObject.transform.SetParent(transform, false);
        fadeRenderer = fadeObject.AddComponent<SpriteRenderer>();
        fadeRenderer.sprite = fadeSprite;

        Color color = fadeColor;
        color.a = 0f;
        fadeRenderer.color = color;
        ApplySorting(fadeRenderer, DefaultFadeOrder);
        FitFadeToCamera();
    }

    private Sprite EnsurePreviewSprite()
    {
        if (previewSprite != null)
        {
            return previewSprite;
        }

        previewTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        previewTexture.name = "LabSceneNavigator Preview Texture";
        previewTexture.hideFlags = HideFlags.HideAndDontSave;
        previewTexture.SetPixel(0, 0, Color.white);
        previewTexture.Apply();

        previewSprite = Sprite.Create(previewTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        previewSprite.name = "LabSceneNavigator Preview Sprite";
        previewSprite.hideFlags = HideFlags.HideAndDontSave;

        return previewSprite;
    }

    private void ApplyGeneratedObjectFlags(GameObject generatedObject)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && generatedObject != null)
        {
            generatedObject.hideFlags = HideFlags.DontSaveInEditor;
        }
#endif
    }

    private void DestroyGeneratedObject(UnityEngine.Object generatedObject)
    {
        if (generatedObject == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(generatedObject);
            return;
        }
#endif

        Destroy(generatedObject);
    }

#if UNITY_EDITOR
    private void QueueEditModePreviewRebuild()
    {
        if (editPreviewRebuildQueued || !isActiveAndEnabled)
        {
            return;
        }

        editPreviewRebuildQueued = true;
        EditorApplication.delayCall += DelayedEditModePreviewRebuild;
    }

    private void DelayedEditModePreviewRebuild()
    {
        editPreviewRebuildQueued = false;

        if (this == null || Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        RebuildEditModePreview();
    }
#endif

    private void FitSpriteRendererToCamera(SpriteRenderer spriteRenderer, bool coverCamera)
    {
        FitSpriteRendererToCamera(spriteRenderer, coverCamera, Vector3.zero);
    }

    private void FitSpriteRendererToCamera(SpriteRenderer spriteRenderer, bool coverCamera, Vector3 worldOffset)
    {
        EnsureCamera();
        if (targetCamera == null || spriteRenderer == null || spriteRenderer.sprite == null || !targetCamera.orthographic)
        {
            return;
        }

        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float scaleX = cameraWidth / spriteSize.x;
        float scaleY = cameraHeight / spriteSize.y;
        float scale = coverCamera ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);

        Vector3 cameraPosition = targetCamera.transform.position;
        spriteRenderer.transform.position = new Vector3(cameraPosition.x + worldOffset.x, cameraPosition.y + worldOffset.y, worldOffset.z);
        spriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void FitFadeToCamera()
    {
        EnsureCamera();
        if (targetCamera == null || fadeRenderer == null || !targetCamera.orthographic)
        {
            return;
        }

        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;
        Vector3 cameraPosition = targetCamera.transform.position;

        fadeRenderer.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, -0.5f);
        fadeRenderer.transform.localScale = new Vector3(cameraWidth, cameraHeight, 1f);
    }

    private void ApplySorting(SpriteRenderer renderer, int sortingOrder)
    {
        if (renderer == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(sortingLayerName))
        {
            renderer.sortingLayerName = sortingLayerName;
        }

        renderer.sortingOrder = sortingOrder;
    }

    private void CacheScreenLookup()
    {
        screenIndexByName.Clear();

        if (screens == null)
        {
            return;
        }

        for (int i = 0; i < screens.Count; i++)
        {
            LabScreen screen = screens[i];
            if (screen == null || string.IsNullOrWhiteSpace(screen.screenName))
            {
                continue;
            }

            screenIndexByName[screen.screenName] = i;
        }
    }

    private int FindInitialScreenIndex()
    {
        int namedIndex = FindScreenIndex(initialScreenName);
        if (namedIndex >= 0)
        {
            return namedIndex;
        }

        if (initialScreenIndex >= 0 && initialScreenIndex < screens.Count)
        {
            return initialScreenIndex;
        }

        return screens.Count > 0 ? 0 : -1;
    }

    private int FindScreenIndex(string screenName)
    {
        if (string.IsNullOrWhiteSpace(screenName))
        {
            return -1;
        }

        if (screenIndexByName.Count != screens.Count)
        {
            CacheScreenLookup();
        }

        int index;
        if (screenIndexByName.TryGetValue(screenName, out index))
        {
            return index;
        }

        return -1;
    }

    private void ApplyOrderedBackgroundsToScreens()
    {
        if (backgroundsInOrder == null || screens == null)
        {
            return;
        }

        for (int i = 0; i < DefaultScreenNames.Length && i < backgroundsInOrder.Count; i++)
        {
            Sprite background = backgroundsInOrder[i];
            if (background == null)
            {
                continue;
            }

            int screenIndex = FindScreenIndexInList(DefaultScreenNames[i], screens);
            if (screenIndex >= 0)
            {
                screens[screenIndex].background = background;
            }
        }
    }

    private List<Sprite> CreateEmptyBackgroundSlots()
    {
        List<Sprite> slots = new List<Sprite>();
        for (int i = 0; i < DefaultScreenNames.Length; i++)
        {
            slots.Add(null);
        }

        return slots;
    }

    private List<LabScreen> CreateDefaultScreens()
    {
        List<LabScreen> defaultScreens = new List<LabScreen>
        {
            new LabScreen
            {
                screenName = "MainLab",
                background = GetOrderedBackground(0),
                hotspots = new List<LabHotspot>
                {
                    CreateArrow("Look Cabinet", "CabinetView", new Vector2(0.12f, 0.5f), 90f),
                    CreateArrow("Look Machine", "MachineView", new Vector2(0.88f, 0.5f), -90f),
                    CreateArrow("Turn Back", "BackView", new Vector2(0.5f, 0.12f), 180f),
                    CreateInteraction("Look Table", "TableView", new Vector2(0.5f, 0.34f), new Vector2(2.8f, 1.2f))
                }
            },
            new LabScreen
            {
                screenName = "BackView",
                background = GetOrderedBackground(1),
                hotspots = new List<LabHotspot>
                {
                    CreateArrow("Return MainLab", "MainLab", new Vector2(0.5f, 0.12f), 180f)
                }
            },
            new LabScreen
            {
                screenName = "CabinetView",
                background = GetOrderedBackground(2),
                hotspots = new List<LabHotspot>
                {
                    CreateArrow("Return MainLab", "MainLab", new Vector2(0.5f, 0.12f), 180f),
                    CreateInteraction("Open Cabinet", "CabinetOpenView", new Vector2(0.5f, 0.52f), new Vector2(3.0f, 3.0f))
                }
            },
            new LabScreen
            {
                screenName = "CabinetOpenView",
                background = GetOrderedBackground(3),
                hotspots = new List<LabHotspot>
                {
                    CreateArrow("Return CabinetView", "CabinetView", new Vector2(0.5f, 0.12f), 180f)
                }
            },
            new LabScreen
            {
                screenName = "MachineView",
                background = GetOrderedBackground(4),
                hotspots = new List<LabHotspot>
                {
                    CreateArrow("Return MainLab", "MainLab", new Vector2(0.5f, 0.12f), 180f)
                }
            },
            new LabScreen
            {
                screenName = "TableView",
                background = GetOrderedBackground(5),
                hotspots = new List<LabHotspot>
                {
                    CreateArrow("Return MainLab", "MainLab", new Vector2(0.5f, 0.12f), 180f)
                }
            }
        };

        return defaultScreens;
    }

    private LabHotspot CreateArrow(string hotspotName, string targetScreenName, Vector2 viewportPosition, float rotationZ)
    {
        return new LabHotspot
        {
            hotspotName = hotspotName,
            kind = LabHotspotKind.Arrow,
            targetScreenName = targetScreenName,
            positionSpace = LabPositionSpace.Viewport,
            position = viewportPosition,
            rotationZ = rotationZ,
            scale = new Vector2(1f, 1f),
            colliderSize = new Vector2(1f, 1f),
            visible = true,
            useSpriteBoundsAsCollider = true,
            animate = true,
            animationMode = LabHotspotAnimation.FloatAlongDirection,
            animationDistance = 0.08f,
            animationSpeed = 3.5f,
            clickPriority = 10
        };
    }

    private LabHotspot CreateInteraction(string hotspotName, string targetScreenName, Vector2 viewportPosition, Vector2 colliderSize)
    {
        return new LabHotspot
        {
            hotspotName = hotspotName,
            kind = LabHotspotKind.Interaction,
            targetScreenName = targetScreenName,
            positionSpace = LabPositionSpace.Viewport,
            position = viewportPosition,
            rotationZ = 0f,
            scale = new Vector2(1f, 1f),
            colliderSize = colliderSize,
            visible = false,
            useSpriteBoundsAsCollider = false,
            animate = false,
            animationMode = LabHotspotAnimation.None,
            clickPriority = 0
        };
    }

    private Sprite GetOrderedBackground(int index)
    {
        if (backgroundsInOrder == null || index < 0 || index >= backgroundsInOrder.Count)
        {
            return null;
        }

        return backgroundsInOrder[index];
    }

    private int FindScreenIndexInList(string screenName, List<LabScreen> screenList)
    {
        if (screenList == null)
        {
            return -1;
        }

        for (int i = 0; i < screenList.Count; i++)
        {
            LabScreen screen = screenList[i];
            if (screen != null && string.Equals(screen.screenName, screenName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private string SafeName(string value, int fallbackIndex)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return "Item " + fallbackIndex;
    }

    private float Smooth01(float t)
    {
        return t * t * (3f - 2f * t);
    }

    [Serializable]
    public sealed class StringEvent : UnityEvent<string>
    {
    }

    [Serializable]
    public sealed class LabScreen
    {
        [Tooltip("Ten man. Vi du: MainLab, BackView, CabinetView, CabinetOpenView, MachineView, TableView.")]
        public string screenName;

        [Tooltip("Anh nen cua man nay.")]
        public Sprite background;

        [Tooltip("Danh sach mui ten va diem click cua man nay.")]
        public List<LabHotspot> hotspots = new List<LabHotspot>();
    }

    [Serializable]
    public sealed class LabHotspot
    {
        [Tooltip("Ten de de nhin trong Hierarchy va log.")]
        public string hotspotName = "Hotspot";

        [Tooltip("Arrow se dung sprite mui ten mac dinh neu khong gan sprite rieng. Interaction co the an sprite va chi dung collider.")]
        public LabHotspotKind kind = LabHotspotKind.Arrow;

        public bool enabled = true;
        public bool visible = true;

        [Tooltip("Sprite rieng cho hotspot. De trong neu muon dung defaultArrowSprite cho mui ten.")]
        public Sprite sprite;

        [Tooltip("World: toa do trong scene. Viewport: x/y tu 0 den 1 theo khung camera.")]
        public LabPositionSpace positionSpace = LabPositionSpace.Viewport;

        public Vector2 position = new Vector2(0.5f, 0.5f);
        public float zOffset = 0f;
        public Vector2 scale = Vector2.one;

        [Tooltip("Goc xoay quanh truc Z. Mac dinh gia dinh sprite mui ten dang chi len.")]
        public float rotationZ = 0f;

        [Tooltip("Neu bat va co sprite, collider se lay kich thuoc theo sprite.")]
        public bool useSpriteBoundsAsCollider = true;

        [Tooltip("Kich thuoc collider theo world units khi khong dung bounds cua sprite.")]
        public Vector2 colliderSize = Vector2.one;

        public Color tint = Color.white;
        public bool useCustomSortingOrder = false;
        public int sortingOrder = DefaultHotspotOrder;

        [Header("Navigation")]
        public bool changeScreenOnClick = true;

        [Tooltip("Ten man se chuyen toi khi click. Uu tien hon targetScreenIndex.")]
        public string targetScreenName;

        [Tooltip("Dung index neu khong muon go ten man.")]
        public int targetScreenIndex = -1;

        [Tooltip("Hotspot co priority cao hon se duoc click truoc neu collider chong len nhau.")]
        public int clickPriority = 0;

        [Header("Animation")]
        public bool animate = true;
        public LabHotspotAnimation animationMode = LabHotspotAnimation.FloatAlongDirection;
        public float animationDistance = 0.08f;
        public float animationSpeed = 3.5f;
        public float animationPhase = 0f;
        public float pulseAmount = 0.06f;

        [Tooltip("De (0,0) de tu lay huong theo rotationZ. Mac dinh gia dinh sprite mui ten dang chi len.")]
        public Vector2 animationDirection = Vector2.zero;
    }

    public enum LabHotspotKind
    {
        Arrow,
        Interaction
    }

    public enum LabPositionSpace
    {
        Viewport,
        World
    }

    public enum LabHotspotAnimation
    {
        None,
        FloatAlongDirection,
        ShakeSideways,
        Pulse
    }

    private sealed class RuntimeHotspot
    {
        public LabScreen screen;
        public LabHotspot config;
        public GameObject gameObject;
        public SpriteRenderer renderer;
        public BoxCollider2D collider;
        public Vector3 basePosition;
        public Vector3 baseScale;
        public Vector2 moveDirection;
        public int hotspotIndex;
        public int screenIndex;
        public bool editPreview;
        public bool usesInteractionPreviewMarker;
        public Transform previewScreenRoot;
        public Vector3 lastPosition;
        public Vector3 lastScale;
        public float lastRotationZ;
    }
}
