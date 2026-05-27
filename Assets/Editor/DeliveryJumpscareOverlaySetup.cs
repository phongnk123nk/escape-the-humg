using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DeliveryJumpscareOverlaySetup
{
    private const string TargetScenePath = "Assets/Scenes/PhongTinHoc.unity";
    private const string MiniGamePath = "ComputerRoomNavigator/Hotspots/Frame49_ComputerScreenView/DeliveryCarMiniGame";
    private const string JumpscareSpritePath = "Assets/DeliveryCarImported/Delivery Driver Assets/jumscare.png";

    [MenuItem("Tools/Computer Room/Ensure Delivery Jumpscare Overlay")]
    public static void EnsureOverlayMenu()
    {
        if (IsPlayModeBusy())
        {
            Debug.LogWarning("Dung Play Mode truoc khi chay tool setup Delivery jumpscare.");
            return;
        }

        EnsureOverlay();
    }

    private static void EnsureOverlay()
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

        GameObject miniGameRoot = GameObject.Find(MiniGamePath);
        if (miniGameRoot == null)
        {
            return;
        }

        DeliveryOrderMiniGameManager manager = miniGameRoot.GetComponent<DeliveryOrderMiniGameManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<DeliveryOrderMiniGameManager>(miniGameRoot);
        }

        manager.blackoutRenderer = EnsureBlackout(miniGameRoot.transform);
        manager.jumpscareRenderer = EnsureJumpscare(miniGameRoot.transform);

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static bool IsPlayModeBusy()
    {
        return EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static SpriteRenderer EnsureBlackout(Transform parent)
    {
        Transform existing = parent.Find("DeliveryEndBlackout");
        GameObject obj = existing != null ? existing.gameObject : new GameObject("DeliveryEndBlackout");
        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(obj, "Create Delivery End Blackout");
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = new Vector3(0f, 0f, -0.18f);
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(14f, 8f, 1f);
        }

        DeliveryPlayableAreaVisual visual = obj.GetComponent<DeliveryPlayableAreaVisual>();
        if (visual == null)
        {
            visual = Undo.AddComponent<DeliveryPlayableAreaVisual>(obj);
        }

        visual.backgroundColor = Color.black;
        visual.sortingOrder = 2600;

        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        renderer.color = Color.black;
        renderer.sortingOrder = 2600;
        obj.SetActive(false);
        EditorUtility.SetDirty(obj);
        return renderer;
    }

    private static SpriteRenderer EnsureJumpscare(Transform parent)
    {
        Transform existing = parent.Find("DeliveryEndJumpscare");
        GameObject obj = existing != null ? existing.gameObject : new GameObject("DeliveryEndJumpscare");
        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(obj, "Create Delivery End Jumpscare");
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = new Vector3(0f, 0f, -0.22f);
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(4.8f, 4.4f, 1f);
        }

        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(obj);
        }

        Sprite sprite = LoadSpriteAsset(JumpscareSpritePath);
        if (sprite != null)
        {
            renderer.sprite = sprite;
        }

        renderer.color = Color.white;
        renderer.sortingOrder = 2610;
        obj.SetActive(false);
        EditorUtility.SetDirty(obj);
        return renderer;
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
