using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ComputerRoomMiniGamesCompleteArrowSetup
{
    private const string TargetScenePath = "Assets/Scenes/PhongTinHoc.unity";
    private const string Frame49Path = "ComputerRoomNavigator/Hotspots/Frame49_ComputerScreenView";
    private const string ArrowSpritePath = "Assets/image/PhongTinHoc/arrow.png";

    [MenuItem("Tools/Computer Room/Ensure Frame49 Complete Arrow")]
    public static void EnsureMenu()
    {
        if (IsPlayModeBusy())
        {
            Debug.LogWarning("Dung Play Mode truoc khi chay tool setup Frame49 arrow.");
            return;
        }

        EnsureArrow();
    }

    private static void EnsureArrow()
    {
        if (IsPlayModeBusy())
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != TargetScenePath)
        {
            if (!System.IO.File.Exists(TargetScenePath))
            {
                return;
            }

            scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        }

        GameObject frame49 = GameObject.Find(Frame49Path);
        if (frame49 == null)
        {
            return;
        }

        GameObject arrow = EnsureArrowObject(frame49.transform);
        ComputerRoomMiniGamesCompleteArrow watcher = frame49.GetComponent<ComputerRoomMiniGamesCompleteArrow>();
        if (watcher == null)
        {
            watcher = Undo.AddComponent<ComputerRoomMiniGamesCompleteArrow>(frame49);
        }

        Transform horse = frame49.transform.Find("MiniGameIcon_Horse");
        Transform food = frame49.transform.Find("MiniGameIcon_Food");
        watcher.horseIcon = horse != null ? horse.GetComponent<ComputerRoomMiniGameIcon>() : null;
        watcher.foodIcon = food != null ? food.GetComponent<ComputerRoomMiniGameIcon>() : null;
        watcher.arrowToFrame46 = arrow;

        EditorUtility.SetDirty(watcher);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static bool IsPlayModeBusy()
    {
        return EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static GameObject EnsureArrowObject(Transform frame49)
    {
        Transform existing = frame49.Find("GoToFrame46AfterMiniGames");
        GameObject arrow = existing != null ? existing.gameObject : new GameObject("GoToFrame46AfterMiniGames");
        bool createdNew = existing == null;
        if (createdNew)
        {
            Undo.RegisterCreatedObjectUndo(arrow, "Create Frame49 Complete Arrow");
            arrow.transform.SetParent(frame49, false);
            arrow.transform.localPosition = new Vector3(0f, -2.75f, -0.1f);
            arrow.transform.localRotation = Quaternion.identity;
            arrow.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
        }

        BoxCollider2D collider = arrow.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(arrow);
        }

        collider.isTrigger = true;
        collider.size = new Vector2(2f, 1f);

        ComputerRoomHotspot hotspot = arrow.GetComponent<ComputerRoomHotspot>();
        if (hotspot == null)
        {
            hotspot = Undo.AddComponent<ComputerRoomHotspot>(arrow);
        }

        hotspot.hotspotName = "GoToFrame46AfterMiniGames";
        hotspot.visibleOnView = ComputerRoomView.Frame49_ComputerScreenView;
        hotspot.targetView = ComputerRoomView.Frame46_BackToDoorView;
        hotspot.canUseWhenPuzzleLocked = true;
        hotspot.requiresPuzzleSolved = false;
        hotspot.startsComputerPuzzle = false;
        hotspot.isExitDoor = false;
        hotspot.isContinueAfterPuzzle = false;
        hotspot.boxCollider = collider;

        SpriteRenderer renderer = arrow.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(arrow);
        }

        Sprite arrowSprite = LoadSpriteAsset(ArrowSpritePath);
        if (arrowSprite != null)
        {
            renderer.sprite = arrowSprite;
        }

        renderer.color = Color.white;
        renderer.sortingOrder = 95;
        hotspot.debugRenderer = renderer;

        RemoveLabel(arrow.transform);

        EditorUtility.SetDirty(arrow);
        EditorUtility.SetDirty(hotspot);
        return arrow;
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

    private static void RemoveLabel(Transform arrow)
    {
        Transform existing = arrow.Find("Label");
        if (existing == null)
        {
            return;
        }

        Undo.DestroyObjectImmediate(existing.gameObject);
    }
}
