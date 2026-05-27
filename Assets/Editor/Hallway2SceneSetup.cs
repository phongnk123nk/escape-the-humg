using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class Hallway2SceneSetup
{
    private const string TargetScenePath = "Assets/Scenes/hanh lang 2.unity";
    private const string NavigatorName = "HallwayNavigator";
    private const string ImageFolder = "Assets/image/hanh lang 2";
    private const string ArrowSpritePath = "Assets/image/PhongTinHoc/arrow.png";
    private const string AutoRunMarkerPath = "Assets/Editor/Hallway2SceneSetup.run";
    private const float FrameGapPixels = 2f;
    private const float TargetViewHeight = 10f;

    static Hallway2SceneSetup()
    {
        EditorApplication.delayCall += EnsureWhenReady;
    }

    [MenuItem("Tools/Hallway/Setup Hanh Lang 2 Navigator")]
    public static void SetupMenu()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Dung Play Mode truoc khi setup hanh lang 2.");
            return;
        }

        SetupHallway2();
    }

    private static void EnsureWhenReady()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += EnsureWhenReady;
            return;
        }

        if (File.Exists(AutoRunMarkerPath))
        {
            SetupHallway2();
        }
    }

    private static void SetupHallway2()
    {
        if (File.Exists(AutoRunMarkerPath))
        {
            AssetDatabase.DeleteAsset(AutoRunMarkerPath);
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != TargetScenePath)
        {
            if (!File.Exists(TargetScenePath))
            {
                Debug.LogWarning("Khong tim thay scene hanh lang 2: " + TargetScenePath);
                return;
            }

            scene = OpenOrCreateTargetScene();
        }
        else if (new FileInfo(TargetScenePath).Length == 0)
        {
            scene = CreateFreshTargetScene();
        }

        EnsureMainCamera();

        GameObject navigatorObject = GameObject.Find(NavigatorName);
        if (navigatorObject == null)
        {
            navigatorObject = new GameObject(NavigatorName);
            Undo.RegisterCreatedObjectUndo(navigatorObject, "Create Hallway 2 Navigator");
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
            Undo.RegisterCreatedObjectUndo(framesObject, "Create Hallway 2 Frames Root");
            framesObject.transform.SetParent(navigatorObject.transform, false);
            framesRoot = framesObject.transform;
        }

        navigator.expectedSceneName = "hanh lang 2";
        navigator.startFrameIndex = 0;
        navigator.loopLastFrameToFirst = false;
        navigator.moveCameraToActiveFrame = true;
        navigator.autoFitBackgroundToCamera = false;
        navigator.fitScreenPercent = 1f;
        navigator.useFadeTransition = true;
        navigator.fadeOutDuration = 0.25f;
        navigator.fadeInDuration = 0.25f;
        navigator.framesRoot = framesRoot;
        if (navigator.targetCamera == null)
        {
            navigator.targetCamera = Camera.main;
        }

        Sprite arrowSprite = LoadSpriteAsset(ArrowSpritePath);
        List<Sprite> backgroundSprites = LoadOrderedBackgroundSprites();
        for (int i = 0; i < backgroundSprites.Count; i++)
        {
            bool isLastFrame = i == backgroundSprites.Count - 1;
            EnsureFrame(framesRoot, "Frame" + (i + 1).ToString("00"), backgroundSprites[i], arrowSprite, navigator, i, isLastFrame);
        }

        FitBackgroundsTo1920x1080(framesRoot);
        ArrangeFramesWithPixelGap(framesRoot, FrameGapPixels);
        navigator.RefreshFramesFromHierarchy();
        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da tao navigator hanh lang 2 voi " + backgroundSprites.Count + " frame.");
    }

    private static Scene OpenOrCreateTargetScene()
    {
        if (new FileInfo(TargetScenePath).Length == 0)
        {
            return CreateFreshTargetScene();
        }

        return EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
    }

    private static Scene CreateFreshTargetScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, TargetScenePath);
        return scene;
    }

    private static void EnsureMainCamera()
    {
        if (Camera.main != null)
        {
            return;
        }

        GameObject cameraObject = new GameObject("Main Camera");
        Undo.RegisterCreatedObjectUndo(cameraObject, "Create Hanh Lang 2 Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = TargetViewHeight * 0.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cameraObject.AddComponent<AudioListener>();
    }

    private static void EnsureFrame(Transform framesRoot, string frameName, Sprite backgroundSprite, Sprite arrowSprite, HallwayImageNavigator navigator, int frameIndex, bool isLastFrame)
    {
        Transform frame = framesRoot.Find(frameName);
        if (frame == null)
        {
            GameObject frameObject = new GameObject(frameName);
            Undo.RegisterCreatedObjectUndo(frameObject, "Create Hallway 2 Frame");
            frameObject.transform.SetParent(framesRoot, false);
            frameObject.transform.localPosition = new Vector3(frameIndex * 18f, 0f, 0f);
            frame = frameObject.transform;
        }

        SpriteRenderer background = EnsureBackground(frame, backgroundSprite);
        EnsureArrow(frame, arrowSprite, navigator, isLastFrame);

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
            Undo.RegisterCreatedObjectUndo(backgroundObject, "Create Hallway 2 Background");
            backgroundObject.transform.SetParent(frame, false);
            backgroundObject.transform.localPosition = Vector3.zero;
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

    private static void EnsureArrow(Transform frame, Sprite arrowSprite, HallwayImageNavigator navigator, bool isLastFrame)
    {
        string objectName = isLastFrame ? "ExitSceneClickArea" : "NextArrow";
        Transform existing = frame.Find(objectName);
        if (existing == null && isLastFrame)
        {
            existing = frame.Find("NextArrow");
        }

        GameObject arrowObject = existing != null ? existing.gameObject : new GameObject(objectName);
        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(arrowObject, "Create Hallway 2 Arrow");
            arrowObject.transform.SetParent(frame, false);
            arrowObject.transform.localPosition = new Vector3(0f, -2.85f, -0.1f);
            arrowObject.transform.localRotation = Quaternion.identity;
            arrowObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }

        arrowObject.name = objectName;
        if (isLastFrame)
        {
            arrowObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            arrowObject.transform.localScale = Vector3.one;
        }

        SpriteRenderer renderer = arrowObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(arrowObject);
        }

        renderer.sprite = isLastFrame ? null : arrowSprite;
        renderer.enabled = !isLastFrame;
        renderer.sortingOrder = 50;
        renderer.color = Color.white;

        BoxCollider2D collider = arrowObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(arrowObject);
        }

        collider.isTrigger = true;
        collider.size = isLastFrame ? new Vector2(12f, 6f) : new Vector2(2f, 1f);

        HallwayArrowHotspot hotspot = arrowObject.GetComponent<HallwayArrowHotspot>();
        if (hotspot == null)
        {
            hotspot = Undo.AddComponent<HallwayArrowHotspot>(arrowObject);
        }

        hotspot.action = isLastFrame ? HallwayHotspotAction.LoadScene : HallwayHotspotAction.GoNextFrame;
        hotspot.navigator = navigator;
        hotspot.videoClip = null;
        hotspot.nextSceneName = "";
        hotspot.boxCollider = collider;
        hotspot.spriteRenderer = renderer;
        hotspot.enableFloatAnimation = !isLastFrame;
        hotspot.floatAmplitude = 0.12f;
        hotspot.floatSpeed = 2f;

        EditorUtility.SetDirty(arrowObject);
        EditorUtility.SetDirty(hotspot);
    }

    private static List<Sprite> LoadOrderedBackgroundSprites()
    {
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 1; i < 100; i++)
        {
            Sprite sprite = LoadSpriteAsset(ImageFolder + "/anh " + i + ".png");
            if (sprite == null)
            {
                break;
            }

            sprites.Add(sprite);
        }

        return sprites;
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

    private static void ArrangeFramesWithPixelGap(Transform framesRoot, float gapPixels)
    {
        float nextLeftEdge = 0f;
        for (int i = 0; i < framesRoot.childCount; i++)
        {
            Transform frame = framesRoot.GetChild(i);
            SpriteRenderer background = FindBackground(frame);
            if (background == null || background.sprite == null)
            {
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
