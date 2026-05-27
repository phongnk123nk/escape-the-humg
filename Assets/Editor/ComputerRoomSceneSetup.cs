using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Tool setup nhanh scene PhongTinHoc tu cac asset anh da co san.
/// </summary>
[InitializeOnLoad]
public static class ComputerRoomSceneSetup
{
    private const string ScenePath = "Assets/Scenes/PhongTinHoc.unity";
    private const string ImageFolder = "Assets/image/PhongTinHoc/";
    private const string LockSpritePath = "Assets/image/PhongTinHoc/o khoa/lock.png";
    private const string HorseIconPath = "Assets/image/PhongTinHoc/may tinh/game ngua 1.png";
    private const string FoodIconPath = "Assets/image/PhongTinHoc/may tinh/shopeefood 1.png";
    private const string ChessTilePrefabPath = "Assets/ChessKnightImported/Prefabs/TilePrefab.prefab";
    private const string ChessHighlightDotPrefabPath = "Assets/ChessKnightImported/Prefabs/HighlightDot.prefab";
    private const string ChessKnightSpritePath = "Assets/ChessKnightImported/asset/con_ma.png";
    private const string ChessGrassSpritePath = "Assets/ChessKnightImported/asset/bai co.jpg";
    private const string ChessBoardBackgroundPath = "Assets/ChessKnightImported/asset/anh 3.png";
    private const string AutoRunMarkerPath = "Assets/Editor/ComputerRoomSceneSetup.run";

    static ComputerRoomSceneSetup()
    {
        EditorApplication.delayCall += RunSetupIfMarkerExists;
    }

    [MenuItem("Tools/Computer Room/Setup PhongTinHoc Scene")]
    public static void SetupPhongTinHocScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        ComputerRoomNavigator navigator = Object.FindFirstObjectByType<ComputerRoomNavigator>();
        if (navigator == null)
        {
            GameObject navigatorObject = new GameObject("ComputerRoomNavigator");
            Undo.RegisterCreatedObjectUndo(navigatorObject, "Create ComputerRoomNavigator");
            navigator = navigatorObject.AddComponent<ComputerRoomNavigator>();
        }

        navigator.expectedSceneName = "PhongTinHoc";
        navigator.startView = ComputerRoomView.Frame44_MainDoorView;
        navigator.editPreviewView = ComputerRoomView.Frame44_MainDoorView;
        navigator.isolateEditModePreview = false;
        navigator.showAllViewsSeparatedInEditMode = true;
        navigator.showHotspotPreview = true;
        navigator.targetCamera = Camera.main;
        navigator.autoCreateBackgroundRenderer = true;
        navigator.editViewSpacing = new Vector2(11f, 6.5f);
        navigator.editViewGapPixels = 80f;
        navigator.useTransitionAnimation = true;
        navigator.fadeOutDuration = 0.18f;
        navigator.fadeInDuration = 0.18f;
        navigator.transitionColor = Color.black;

        AssignSprites(navigator);
        navigator.CreateBackgroundRenderer();
        navigator.SetupDefaultComputerRoomHotspots();

        if (navigator.backgroundRenderer != null)
        {
            navigator.backgroundRenderer.sprite = navigator.frame44MainDoor;
            navigator.backgroundRenderer.sortingOrder = -100;
            EditorUtility.SetDirty(navigator.backgroundRenderer);
        }

        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Da setup xong ComputerRoomNavigator trong scene PhongTinHoc.");
    }

    [MenuItem("Tools/Computer Room/Arrange PhongTinHoc Like Lab Preview")]
    public static void ArrangePhongTinHocLikeLabPreview()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        ComputerRoomNavigator navigator = Object.FindFirstObjectByType<ComputerRoomNavigator>();
        if (navigator == null)
        {
            SetupPhongTinHocScene();
            navigator = Object.FindFirstObjectByType<ComputerRoomNavigator>();
        }

        if (navigator == null)
        {
            Debug.LogWarning("Khong tim thay ComputerRoomNavigator de sap xep preview.");
            return;
        }

        navigator.showAllViewsSeparatedInEditMode = true;
        navigator.isolateEditModePreview = false;
        navigator.editViewSpacing = new Vector2(11f, 6.5f);
        navigator.editViewGapPixels = 80f;
        navigator.ArrangeAllViewsForEditingMenu();

        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da sap xep PhongTinHoc thanh layout preview 3x2 giong PhongThiNghiem.");
    }

    [MenuItem("Tools/Computer Room/Add Door Links Only")]
    public static void AddDoorLinksOnly()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        ComputerRoomNavigator navigator = Object.FindFirstObjectByType<ComputerRoomNavigator>();
        if (navigator == null)
        {
            Debug.LogWarning("Khong tim thay ComputerRoomNavigator de them door links.");
            return;
        }

        navigator.AddOnlyFrame44Frame50DoorHotspots();
        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da them link Frame44 <-> Frame50 theo yeu cau, khong sap xep lai layout.");
    }

    [MenuItem("Tools/Computer Room/Add Door Lock Puzzle Only")]
    public static void AddDoorLockPuzzleOnly()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        ComputerRoomNavigator navigator = Object.FindFirstObjectByType<ComputerRoomNavigator>();
        if (navigator == null)
        {
            Debug.LogWarning("Khong tim thay ComputerRoomNavigator de them o khoa.");
            return;
        }

        Sprite lockSprite = LoadSprite(LockSpritePath);
        navigator.AddOnlyDoorLockPuzzle(lockSprite);
        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da them o khoa va puzzle nhap 2 so vao Frame50_DoorCloseView, khong thay doi layout hien co.");
    }

    [MenuItem("Tools/Computer Room/Add Frame49 Mini Game Icons Only")]
    public static void AddFrame49MiniGameIconsOnly()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        ComputerRoomNavigator navigator = Object.FindFirstObjectByType<ComputerRoomNavigator>();
        if (navigator == null)
        {
            Debug.LogWarning("Khong tim thay ComputerRoomNavigator de them icon mini-game.");
            return;
        }

        Sprite horseIcon = LoadSprite(HorseIconPath);
        Sprite foodIcon = LoadSprite(FoodIconPath);
        navigator.AddOnlyFrame49MiniGameIcons(horseIcon, foodIcon);
        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da them 2 icon mini-game vao Frame49_ComputerScreenView, khong thay doi layout hien co.");
    }

    [MenuItem("Tools/Computer Room/Add Chess Knight Game To Horse Icon Only")]
    public static void AddChessKnightGameToHorseIconOnly()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        ComputerRoomNavigator navigator = Object.FindFirstObjectByType<ComputerRoomNavigator>();
        if (navigator == null)
        {
            Debug.LogWarning("Khong tim thay ComputerRoomNavigator de gan mini-game con ngua.");
            return;
        }

        Sprite horseIcon = LoadSprite(HorseIconPath);
        Sprite foodIcon = LoadSprite(FoodIconPath);
        navigator.AddOnlyFrame49MiniGameIcons(horseIcon, foodIcon);

        GameObject tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ChessTilePrefabPath);
        GameObject highlightDotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ChessHighlightDotPrefabPath);
        Sprite knightSprite = LoadSprite(ChessKnightSpritePath);
        Sprite grassSprite = LoadSprite(ChessGrassSpritePath);
        Sprite boardBackgroundSprite = LoadSprite(ChessBoardBackgroundPath);

        if (tilePrefab == null)
        {
            Debug.LogWarning("Khong tim thay TilePrefab chess knight tai: " + ChessTilePrefabPath);
            return;
        }

        navigator.AddOnlyChessKnightMiniGame(tilePrefab, highlightDotPrefab, knightSprite, grassSprite, boardBackgroundSprite);
        EditorUtility.SetDirty(navigator);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Da gan man choi ChessKnight vao icon con ngua trong Frame49, khong them scene moi.");
    }

    // Ham nay dung de goi tu Unity batchmode:
    // -executeMethod ComputerRoomSceneSetup.SetupPhongTinHocSceneBatch
    public static void SetupPhongTinHocSceneBatch()
    {
        SetupPhongTinHocScene();
        AssetDatabase.SaveAssets();
        EditorApplication.Exit(0);
    }

    public static void ArrangePhongTinHocBatch()
    {
        ArrangePhongTinHocLikeLabPreview();
        AssetDatabase.SaveAssets();
        EditorApplication.Exit(0);
    }

    private static void AssignSprites(ComputerRoomNavigator navigator)
    {
        navigator.frame44MainDoor = LoadSprite(ImageFolder + "Frame 44.png");
        navigator.frame45MainComputer = LoadSprite(ImageFolder + "Frame 45.png");
        navigator.frame46BackToDoor = LoadSprite(ImageFolder + "Frame 46.png");
        navigator.frame47ComputerDesk = LoadSprite(ImageFolder + "Frame 47.png");
        navigator.frame49ComputerScreen = LoadSprite(ImageFolder + "Frame 49.png");
        navigator.frame50DoorClose = LoadSprite(ImageFolder + "Frame 50.png");
        navigator.defaultArrowSprite = LoadSprite(ImageFolder + "arrow.png");
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (direct != null)
        {
            return direct;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                return sprite;
            }
        }

        Debug.LogWarning("Khong tim thay Sprite tai path: " + path);
        return null;
    }

    private static void RunSetupIfMarkerExists()
    {
        if (!File.Exists(AutoRunMarkerPath))
        {
            return;
        }

        File.Delete(AutoRunMarkerPath);
        string metaPath = AutoRunMarkerPath + ".meta";
        if (File.Exists(metaPath))
        {
            File.Delete(metaPath);
        }

        AddChessKnightGameToHorseIconOnly();
        AssetDatabase.Refresh();
    }
}
