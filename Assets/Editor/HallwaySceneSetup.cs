using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[InitializeOnLoad]
public static class HallwaySceneSetup
{
    private const string TargetScenePath = "Assets/Scenes/hanh lang 1.unity";
    private const string NavigatorName = "HallwayNavigator";
    private const string ImageFolder = "Assets/image/hanh lang 1";
    private const string ArrowSpritePath = "Assets/image/PhongTinHoc/arrow.png";
    private const string VideoVapPath = "Assets/image/hanh lang 1/video vap.MOV";
    private const string AutoRunMarkerPath = "Assets/Editor/HallwaySceneSetup.run";
    private const float DefaultFrameSpacing = 12f;
    private const float FrameGapPixels = 2f;
    private const float TargetViewHeight = 10f;

    static HallwaySceneSetup()
    {
        EditorApplication.delayCall += EnsureWhenReady;
    }

    [MenuItem("Tools/Hallway/Setup Hanh Lang 1 Navigator")]
    public static void SetupMenu()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Dung Play Mode truoc khi setup hanh lang 1.");
            return;
        }

        SetupHallway();
    }

    private static void EnsureWhenReady()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += EnsureWhenReady;
            return;
        }

        bool shouldAutoRun = System.IO.File.Exists(AutoRunMarkerPath);
        if (shouldAutoRun)
        {
            SetupHallway();
        }
    }

    [MenuItem("Tools/Hallway/Arrange Frames With 2px Gap")]
    public static void ArrangeFramesWith2PxGapMenu()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Dung Play Mode truoc khi sap xep frame hanh lang 1.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != TargetScenePath)
        {
            scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        }

        GameObject navigatorObject = GameObject.Find(NavigatorName);
        if (navigatorObject == null)
        {
            Debug.LogWarning("Khong tim thay HallwayNavigator. Hay chay Setup Hanh Lang 1 Navigator truoc.");
            return;
        }

        Transform framesRoot = navigatorObject.transform.Find("Frames");
        if (framesRoot == null)
        {
            Debug.LogWarning("Khong tim thay Frames trong HallwayNavigator.");
            return;
        }

        ArrangeFramesWithPixelGap(framesRoot, FrameGapPixels);
        HallwayImageNavigator navigator = navigatorObject.GetComponent<HallwayImageNavigator>();
        if (navigator != null)
        {
            navigator.autoFitBackgroundToCamera = false;
            navigator.RefreshFramesFromHierarchy();
            EditorUtility.SetDirty(navigator);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da tach 5 frame hanh lang 1 voi khoang cach 2px.");
    }

    [MenuItem("Tools/Hallway/Fit Frames To 1920x1080")]
    public static void FitFramesTo1920x1080Menu()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Dung Play Mode truoc khi fit frame hanh lang 1.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != TargetScenePath)
        {
            scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        }

        GameObject navigatorObject = GameObject.Find(NavigatorName);
        if (navigatorObject == null)
        {
            Debug.LogWarning("Khong tim thay HallwayNavigator. Hay chay Setup Hanh Lang 1 Navigator truoc.");
            return;
        }

        Transform framesRoot = navigatorObject.transform.Find("Frames");
        if (framesRoot == null)
        {
            Debug.LogWarning("Khong tim thay Frames trong HallwayNavigator.");
            return;
        }

        FitBackgroundsTo1920x1080(framesRoot);
        ArrangeFramesWithPixelGap(framesRoot, FrameGapPixels);

        HallwayImageNavigator navigator = navigatorObject.GetComponent<HallwayImageNavigator>();
        if (navigator != null)
        {
            navigator.autoFitBackgroundToCamera = false;
            navigator.RefreshFramesFromHierarchy();
            EditorUtility.SetDirty(navigator);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da dua 5 anh hanh lang 1 ve ti le hien thi 1920x1080.");
    }

    private static void SetupHallway()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (System.IO.File.Exists(AutoRunMarkerPath))
        {
            AssetDatabase.DeleteAsset(AutoRunMarkerPath);
        }

        if (scene.path != TargetScenePath)
        {
            if (!System.IO.File.Exists(TargetScenePath))
            {
                Debug.LogWarning("Khong tim thay scene hanh lang 1: " + TargetScenePath);
                return;
            }

            scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        }

        GameObject navigatorObject = GameObject.Find(NavigatorName);
        if (navigatorObject == null)
        {
            navigatorObject = new GameObject(NavigatorName);
            Undo.RegisterCreatedObjectUndo(navigatorObject, "Create Hallway Navigator");
        }

        HallwayImageNavigator navigator = navigatorObject.GetComponent<HallwayImageNavigator>();
        if (navigator == null)
        {
            navigator = Undo.AddComponent<HallwayImageNavigator>(navigatorObject);
        }

        Transform framesRoot = navigatorObject.transform.Find("Frames");
        if (framesRoot == null)
        {
            GameObject framesObject = new GameObject("Frames");
            Undo.RegisterCreatedObjectUndo(framesObject, "Create Hallway Frames Root");
            framesObject.transform.SetParent(navigatorObject.transform, false);
            framesRoot = framesObject.transform;
        }

        navigator.framesRoot = framesRoot;
        navigator.expectedSceneName = "hanh lang 1";
        navigator.startFrameIndex = 0;
        navigator.loopLastFrameToFirst = true;
        navigator.moveCameraToActiveFrame = true;
        navigator.autoFitBackgroundToCamera = false;
        navigator.fitScreenPercent = 1f;
        if (navigator.targetCamera == null)
        {
            navigator.targetCamera = Camera.main;
        }

        Sprite arrowSprite = LoadSpriteAsset(ArrowSpritePath);
        VideoClip videoVap = AssetDatabase.LoadAssetAtPath<VideoClip>(VideoVapPath);
        for (int i = 0; i < 5; i++)
        {
            string frameName = "Frame0" + (i + 1);
            Sprite backgroundSprite = LoadSpriteAsset(ImageFolder + "/anh " + (i + 1) + ".png");
            EnsureFrame(framesRoot, frameName, backgroundSprite, arrowSprite, videoVap, navigator, i);
        }

        FitBackgroundsTo1920x1080(framesRoot);
        ArrangeFramesWithPixelGap(framesRoot, FrameGapPixels);
        navigator.RefreshFramesFromHierarchy();
        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Da setup navigator cho scene hanh lang 1.");
    }

    private static void EnsureFrame(Transform framesRoot, string frameName, Sprite backgroundSprite, Sprite arrowSprite, VideoClip videoVap, HallwayImageNavigator navigator, int frameIndex)
    {
        Transform frame = framesRoot.Find(frameName);
        if (frame == null)
        {
            GameObject frameObject = new GameObject(frameName);
            Undo.RegisterCreatedObjectUndo(frameObject, "Create Hallway Frame");
            frameObject.transform.SetParent(framesRoot, false);
            frameObject.transform.localPosition = new Vector3(frameIndex * DefaultFrameSpacing, 0f, 0f);
            frame = frameObject.transform;
        }

        SpriteRenderer background = EnsureBackground(frame, backgroundSprite);
        EnsureArrow(frame, arrowSprite, videoVap, navigator, frameIndex);

        frame.gameObject.SetActive(true);
        EditorUtility.SetDirty(background);
        EditorUtility.SetDirty(frame.gameObject);
    }

    private static SpriteRenderer EnsureBackground(Transform frame, Sprite sprite)
    {
        Transform existing = frame.Find("PreviewBackground");
        GameObject backgroundObject = existing != null ? existing.gameObject : new GameObject("PreviewBackground");
        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(backgroundObject, "Create Hallway Background");
            backgroundObject.transform.SetParent(frame, false);
            backgroundObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            backgroundObject.transform.localScale = Vector3.one;
        }

        SpriteRenderer renderer = backgroundObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(backgroundObject);
        }

        renderer.sprite = sprite;
        renderer.sortingOrder = -100;
        renderer.color = Color.white;
        return renderer;
    }

    private static void EnsureArrow(Transform frame, Sprite arrowSprite, VideoClip videoVap, HallwayImageNavigator navigator, int frameIndex)
    {
        string objectName = frameIndex == 4 ? "ExitSceneClickArea" : "NextArrow";
        Transform existing = frame.Find(objectName);
        if (existing == null && frameIndex == 4)
        {
            existing = frame.Find("NextArrow");
        }

        GameObject arrowObject = existing != null ? existing.gameObject : new GameObject(objectName);
        arrowObject.name = objectName;
        bool createdNew = existing == null;
        if (createdNew)
        {
            Undo.RegisterCreatedObjectUndo(arrowObject, "Create Hallway Next Arrow");
            arrowObject.transform.SetParent(frame, false);
            arrowObject.transform.localPosition = new Vector3(0f, -2.85f, -0.1f);
            arrowObject.transform.localRotation = Quaternion.identity;
            arrowObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }

        SpriteRenderer renderer = arrowObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(arrowObject);
        }

        renderer.sprite = arrowSprite;
        renderer.sortingOrder = 50;
        renderer.color = Color.white;
        renderer.enabled = frameIndex != 4;

        BoxCollider2D collider = arrowObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(arrowObject);
        }

        collider.isTrigger = true;
        collider.size = frameIndex == 4 ? new Vector2(16f, 9f) : new Vector2(2f, 1f);

        HallwayArrowHotspot hotspot = arrowObject.GetComponent<HallwayArrowHotspot>();
        if (hotspot == null)
        {
            hotspot = Undo.AddComponent<HallwayArrowHotspot>(arrowObject);
        }

        hotspot.navigator = navigator;
        hotspot.action = HallwayHotspotAction.GoNextFrame;
        hotspot.videoClip = null;
        hotspot.nextSceneName = "room1";
        if (frameIndex == 3)
        {
            hotspot.action = HallwayHotspotAction.PlayVideoThenNextFrame;
            hotspot.videoClip = videoVap;
        }
        else if (frameIndex == 4)
        {
            hotspot.action = HallwayHotspotAction.LoadScene;
        }

        hotspot.boxCollider = collider;
        hotspot.spriteRenderer = renderer;

        EditorUtility.SetDirty(arrowObject);
        EditorUtility.SetDirty(hotspot);
    }

    private static void ArrangeFramesWithPixelGap(Transform framesRoot, float gapPixels)
    {
        float nextLeftEdge = 0f;
        for (int i = 0; i < framesRoot.childCount; i++)
        {
            Transform frame = framesRoot.GetChild(i);
            SpriteRenderer background = FindBackground(frame);
            if (background == null || background.sprite == null)
            {
                frame.localPosition = new Vector3(i * DefaultFrameSpacing, frame.localPosition.y, frame.localPosition.z);
                continue;
            }

            float gapWorld = gapPixels / background.sprite.pixelsPerUnit;
            float width = background.bounds.size.x;
            float centerX = i == 0 ? 0f : nextLeftEdge + width * 0.5f;
            frame.position = new Vector3(centerX, frame.position.y, frame.position.z);
            nextLeftEdge = centerX + width * 0.5f + gapWorld;
            EditorUtility.SetDirty(frame);
        }
    }

    private static void FitBackgroundsTo1920x1080(Transform framesRoot)
    {
        for (int i = 0; i < framesRoot.childCount; i++)
        {
            Transform frame = framesRoot.GetChild(i);
            SpriteRenderer background = FindBackground(frame);
            if (background == null || background.sprite == null)
            {
                continue;
            }

            Vector2 spriteSize = background.sprite.bounds.size;
            float targetWidth = TargetViewHeight * (1920f / 1080f);
            float scale = Mathf.Min(targetWidth / spriteSize.x, TargetViewHeight / spriteSize.y);
            background.transform.localScale = new Vector3(scale, scale, 1f);
            background.transform.localPosition = new Vector3(0f, 0f, background.transform.localPosition.z);
            EditorUtility.SetDirty(background.transform);
        }
    }

    private static SpriteRenderer FindBackground(Transform frame)
    {
        Transform background = frame.Find("PreviewBackground");
        return background != null ? background.GetComponent<SpriteRenderer>() : null;
    }

    private static Sprite LoadSpriteAsset(string assetPath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
        {
            return sprite;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite subSprite = assets[i] as Sprite;
            if (subSprite != null)
            {
                return subSprite;
            }
        }

        return null;
    }
}
