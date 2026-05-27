using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DeliveryCarSceneImporter
{
    private const string TargetScenePath = "Assets/Scenes/PhongTinHoc.unity";
    private const string ImportedDeliveryScenePath = "Assets/DeliveryCarImported/game/deliverycar.unity";
    private const string AutoRunMarkerPath = "Assets/Editor/DeliveryCarSceneImporter.run";
    private const string NavigatorName = "ComputerRoomNavigator";
    private const string Frame49Path = "Hotspots/Frame49_ComputerScreenView";
    private const string MiniGameRootName = "DeliveryCarMiniGame";
    private const string FoodIconName = "MiniGameIcon_Food";
    private const string PlayAreaBackgroundName = "DeliveryPlayableAreaBackground";
    private const string JumpscareSpritePath = "Assets/DeliveryCarImported/Delivery Driver Assets/jumscare.png";

    static DeliveryCarSceneImporter()
    {
        EditorApplication.delayCall += RunIfMarkerExists;
    }

    [MenuItem("Tools/Computer Room/Add Delivery Car Game To Food Icon Only")]
    public static void AddDeliveryCarGameToFoodIconOnly()
    {
        if (IsPlayModeBusy())
        {
            Debug.LogWarning("Dung Play Mode truoc khi chay tool setup Delivery Car.");
            return;
        }

        Scene targetScene = SceneManager.GetActiveScene();
        if (targetScene.path != TargetScenePath)
        {
            targetScene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        }

        AddRequiredTags();
        AssetDatabase.Refresh();

        Transform frame49 = FindFrame49Folder();
        if (frame49 == null)
        {
            Debug.LogWarning("Khong tim thay folder Frame49_ComputerScreenView de them DeliveryCarMiniGame.");
            return;
        }

        GameObject miniGameRoot = GetOrCreateDeliveryMiniGame(frame49);
        HookFoodIcon(frame49, miniGameRoot);

        EditorUtility.SetDirty(miniGameRoot);
        EditorSceneManager.MarkSceneDirty(targetScene);
        EditorSceneManager.SaveScene(targetScene);

        Debug.Log("Da gan mini-game Delivery Car vao MiniGameIcon_Food, khong tao scene moi.");
    }

    private static Transform FindFrame49Folder()
    {
        GameObject navigator = GameObject.Find(NavigatorName);
        if (navigator == null)
        {
            return null;
        }

        return navigator.transform.Find(Frame49Path);
    }

    private static GameObject GetOrCreateDeliveryMiniGame(Transform frame49)
    {
        Transform existing = frame49.Find(MiniGameRootName);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            EnsureDeliveryRuntimeSetup(existing.gameObject);
            return existing.gameObject;
        }

        if (!File.Exists(ImportedDeliveryScenePath))
        {
            Debug.LogWarning("Khong tim thay scene Delivery da import tai: " + ImportedDeliveryScenePath);
            return new GameObject(MiniGameRootName);
        }

        GameObject root = new GameObject(MiniGameRootName);
        Undo.RegisterCreatedObjectUndo(root, "Create Delivery Car Mini Game");
        root.transform.SetParent(frame49, false);
        root.transform.localPosition = new Vector3(0.05f, -0.05f, -0.25f);
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        GameObject content = new GameObject("ImportedDeliverySceneContent");
        Undo.RegisterCreatedObjectUndo(content, "Create Delivery Car Content");
        content.transform.SetParent(root.transform, false);
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;
        content.transform.localScale = Vector3.one * 0.055f;

        Scene currentScene = SceneManager.GetActiveScene();
        Scene importedScene = EditorSceneManager.OpenScene(ImportedDeliveryScenePath, OpenSceneMode.Additive);
        GameObject[] sceneRoots = importedScene.GetRootGameObjects();

        for (int i = 0; i < sceneRoots.Length; i++)
        {
            GameObject source = sceneRoots[i];
            if (source == null || source.name == "Main Camera")
            {
                continue;
            }

            GameObject clone = Object.Instantiate(source);
            Undo.RegisterCreatedObjectUndo(clone, "Clone Delivery Scene Object");
            clone.name = source.name;
            SceneManager.MoveGameObjectToScene(clone, currentScene);
            clone.transform.SetParent(content.transform, false);
            clone.transform.localPosition = source.transform.localPosition;
            clone.transform.localRotation = source.transform.localRotation;
            clone.transform.localScale = source.transform.localScale;
        }

        EditorSceneManager.CloseScene(importedScene, true);
        ApplyDeliverySorting(root, 2200);
        EnsureDeliveryRuntimeSetup(root);

        ComputerRoomMiniGameStartHidden startHidden = root.GetComponent<ComputerRoomMiniGameStartHidden>();
        if (startHidden == null)
        {
            startHidden = Undo.AddComponent<ComputerRoomMiniGameStartHidden>(root);
        }

        startHidden.hideOnPlay = true;
        root.SetActive(true);
        return root;
    }

    private static void EnsureDeliveryRuntimeSetup(GameObject miniGameRoot)
    {
        GameObject background = EnsurePlayAreaBackground(miniGameRoot);
        BoxCollider2D playArea = background.GetComponent<BoxCollider2D>();
        Transform car = FindDeepChild(miniGameRoot.transform, "Car");
        DeliveryOrderMiniGameManager orderManager = EnsureOrderManager(miniGameRoot);

        if (car != null)
        {
            DeliveryCarPlayAreaLimiter limiter = car.GetComponent<DeliveryCarPlayAreaLimiter>();
            if (limiter == null)
            {
                limiter = Undo.AddComponent<DeliveryCarPlayAreaLimiter>(car.gameObject);
            }

            limiter.playArea = playArea;
            EditorUtility.SetDirty(limiter);

            DeliveryOrderCarTrigger orderTrigger = car.GetComponent<DeliveryOrderCarTrigger>();
            if (orderTrigger == null)
            {
                orderTrigger = Undo.AddComponent<DeliveryOrderCarTrigger>(car.gameObject);
            }

            orderTrigger.manager = orderManager;
            EditorUtility.SetDirty(orderTrigger);
        }
    }

    private static DeliveryOrderMiniGameManager EnsureOrderManager(GameObject miniGameRoot)
    {
        DeliveryOrderMiniGameManager manager = miniGameRoot.GetComponent<DeliveryOrderMiniGameManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<DeliveryOrderMiniGameManager>(miniGameRoot);
        }

        Transform contentRoot = miniGameRoot.transform.Find("ImportedDeliverySceneContent");
        manager.contentRoot = contentRoot != null ? contentRoot : miniGameRoot.transform;
        manager.car = FindDeepChild(miniGameRoot.transform, "Car");
        manager.foodIcon = FindFoodIconComponent();
        manager.messageText = EnsureDeliveryMessageText(miniGameRoot);
        manager.blackoutRenderer = EnsureEndBlackout(miniGameRoot);
        manager.jumpscareRenderer = EnsureEndJumpscare(miniGameRoot);
        manager.ordersToWin = 3;
        manager.autoRefreshRoadPoints = true;

        manager.packageObjects.Clear();
        AddNamedObjectsToList(manager.contentRoot, "Package", manager.packageObjects);

        manager.locationObjects.Clear();
        AddNamedObjectsToList(manager.contentRoot, "Location", manager.locationObjects);

        manager.roadPoints.Clear();
        AddRoadSpawnTransformsToList(manager.contentRoot, manager.roadPoints);

        EditorUtility.SetDirty(manager);
        return manager;
    }

    private static SpriteRenderer EnsureEndBlackout(GameObject miniGameRoot)
    {
        Transform existing = miniGameRoot.transform.Find("DeliveryEndBlackout");
        GameObject blackoutObject;
        bool createdNew = existing == null;
        if (existing != null)
        {
            blackoutObject = existing.gameObject;
        }
        else
        {
            blackoutObject = new GameObject("DeliveryEndBlackout");
            Undo.RegisterCreatedObjectUndo(blackoutObject, "Create Delivery End Blackout");
            blackoutObject.transform.SetParent(miniGameRoot.transform, false);
        }

        if (createdNew)
        {
            blackoutObject.transform.localPosition = new Vector3(0f, 0f, -0.18f);
            blackoutObject.transform.localRotation = Quaternion.identity;
            blackoutObject.transform.localScale = new Vector3(14f, 8f, 1f);
        }

        DeliveryPlayableAreaVisual visual = blackoutObject.GetComponent<DeliveryPlayableAreaVisual>();
        if (visual == null)
        {
            visual = Undo.AddComponent<DeliveryPlayableAreaVisual>(blackoutObject);
        }

        visual.backgroundColor = Color.black;
        visual.sortingOrder = 2600;

        SpriteRenderer renderer = blackoutObject.GetComponent<SpriteRenderer>();
        renderer.color = Color.black;
        renderer.sortingOrder = 2600;
        blackoutObject.SetActive(false);
        EditorUtility.SetDirty(blackoutObject);
        return renderer;
    }

    private static SpriteRenderer EnsureEndJumpscare(GameObject miniGameRoot)
    {
        Transform existing = miniGameRoot.transform.Find("DeliveryEndJumpscare");
        GameObject jumpscareObject;
        bool createdNew = existing == null;
        if (existing != null)
        {
            jumpscareObject = existing.gameObject;
        }
        else
        {
            jumpscareObject = new GameObject("DeliveryEndJumpscare");
            Undo.RegisterCreatedObjectUndo(jumpscareObject, "Create Delivery End Jumpscare");
            jumpscareObject.transform.SetParent(miniGameRoot.transform, false);
        }

        if (createdNew)
        {
            jumpscareObject.transform.localPosition = new Vector3(0f, 0f, -0.22f);
            jumpscareObject.transform.localRotation = Quaternion.identity;
            jumpscareObject.transform.localScale = new Vector3(4.8f, 4.4f, 1f);
        }

        SpriteRenderer renderer = jumpscareObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(jumpscareObject);
        }

        Sprite jumpscareSprite = LoadSpriteAsset(JumpscareSpritePath);
        if (jumpscareSprite != null)
        {
            renderer.sprite = jumpscareSprite;
        }

        renderer.color = Color.white;
        renderer.sortingOrder = 2610;
        jumpscareObject.SetActive(false);
        EditorUtility.SetDirty(jumpscareObject);
        return renderer;
    }

    private static TextMesh EnsureDeliveryMessageText(GameObject miniGameRoot)
    {
        Transform existing = miniGameRoot.transform.Find("DeliveryOrderMessage");
        GameObject messageObject;
        bool createdNew = existing == null;
        if (existing != null)
        {
            messageObject = existing.gameObject;
        }
        else
        {
            messageObject = new GameObject("DeliveryOrderMessage");
            Undo.RegisterCreatedObjectUndo(messageObject, "Create Delivery Order Message");
            messageObject.transform.SetParent(miniGameRoot.transform, false);
        }

        if (createdNew)
        {
            messageObject.transform.localPosition = new Vector3(0f, 2.35f, -0.12f);
            messageObject.transform.localRotation = Quaternion.identity;
            messageObject.transform.localScale = Vector3.one * 0.22f;
        }

        TextMesh textMesh = messageObject.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = Undo.AddComponent<TextMesh>(messageObject);
        }

        if (string.IsNullOrEmpty(textMesh.text))
        {
            textMesh.text = "Bạn có 1 đơn hàng";
        }

        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        if (createdNew)
        {
            textMesh.fontSize = 38;
            textMesh.color = Color.white;
        }

        MeshRenderer renderer = messageObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 2300;
        }

        return textMesh;
    }

    private static GameObject EnsurePlayAreaBackground(GameObject miniGameRoot)
    {
        Transform existing = miniGameRoot.transform.Find(PlayAreaBackgroundName);
        GameObject backgroundObject;
        if (existing != null)
        {
            backgroundObject = existing.gameObject;
        }
        else
        {
            backgroundObject = new GameObject(PlayAreaBackgroundName);
            Undo.RegisterCreatedObjectUndo(backgroundObject, "Create Delivery Play Area Background");
            backgroundObject.transform.SetParent(miniGameRoot.transform, false);
        }

        backgroundObject.transform.SetSiblingIndex(0);
        backgroundObject.transform.localPosition = new Vector3(0f, 0f, 0.08f);
        backgroundObject.transform.localRotation = Quaternion.identity;
        if (backgroundObject.transform.localScale.x > 30f || backgroundObject.transform.localScale.y > 30f)
        {
            backgroundObject.transform.localScale = new Vector3(8.8f, 4.5f, 1f);
        }

        SpriteRenderer renderer = backgroundObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(backgroundObject);
        }

        renderer.color = new Color(1f, 1f, 1f, 0.38f);
        renderer.sortingOrder = 2190;

        BoxCollider2D collider = backgroundObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(backgroundObject);
        }

        collider.isTrigger = true;
        collider.size = Vector2.one;
        collider.offset = Vector2.zero;

        DeliveryPlayableAreaVisual visual = backgroundObject.GetComponent<DeliveryPlayableAreaVisual>();
        if (visual == null)
        {
            visual = Undo.AddComponent<DeliveryPlayableAreaVisual>(backgroundObject);
        }

        visual.backgroundColor = new Color(1f, 1f, 1f, 0.38f);
        visual.sortingOrder = 2190;

        EditorUtility.SetDirty(backgroundObject);
        return backgroundObject;
    }

    private static void HookFoodIcon(Transform frame49, GameObject miniGameRoot)
    {
        Transform foodIconTransform = frame49.Find(FoodIconName);
        if (foodIconTransform == null)
        {
            Debug.LogWarning("Khong tim thay MiniGameIcon_Food de gan DeliveryCarMiniGame.");
            return;
        }

        ComputerRoomMiniGameIcon foodIcon = foodIconTransform.GetComponent<ComputerRoomMiniGameIcon>();
        if (foodIcon == null)
        {
            foodIcon = Undo.AddComponent<ComputerRoomMiniGameIcon>(foodIconTransform.gameObject);
        }

        foodIcon.miniGamePanelToOpen = miniGameRoot;
        foodIcon.sceneNameToLoad = "";
        EditorUtility.SetDirty(foodIcon);
    }

    private static void ApplyDeliverySorting(GameObject root, int sortingBase)
    {
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject.name == PlayAreaBackgroundName)
            {
                renderers[i].sortingOrder = sortingBase - 10;
                continue;
            }

            renderers[i].sortingOrder = sortingBase + renderers[i].sortingOrder;
        }
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static ComputerRoomMiniGameIcon FindFoodIconComponent()
    {
        Transform frame49 = FindFrame49Folder();
        if (frame49 == null)
        {
            return null;
        }

        Transform foodIconTransform = frame49.Find(FoodIconName);
        return foodIconTransform != null ? foodIconTransform.GetComponent<ComputerRoomMiniGameIcon>() : null;
    }

    private static void AddNamedObjectsToList(Transform root, string namePrefix, System.Collections.Generic.List<GameObject> results)
    {
        if (root == null)
        {
            return;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child == root)
            {
                continue;
            }

            if (child.name.StartsWith(namePrefix))
            {
                results.Add(child.gameObject);
            }
        }
    }

    private static void AddNamedTransformsToList(Transform root, string namePrefix, System.Collections.Generic.List<Transform> results)
    {
        if (root == null)
        {
            return;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child == root)
            {
                continue;
            }

            if (child.name.StartsWith(namePrefix))
            {
                results.Add(child);
            }
        }
    }

    private static void AddRoadSpawnTransformsToList(Transform root, System.Collections.Generic.List<Transform> results)
    {
        if (root == null)
        {
            return;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child == root)
            {
                continue;
            }

            if (IsRoadSpawnPoint(child.name))
            {
                results.Add(child);
            }
        }
    }

    private static bool IsRoadSpawnPoint(string objectName)
    {
        return objectName.StartsWith("Road")
            || objectName.StartsWith("Corner")
            || objectName.StartsWith("Curve")
            || objectName.StartsWith("Intersection")
            || objectName.StartsWith("Bridge");
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

    private static void AddRequiredTags()
    {
        AddTagIfMissing("Boost");
        AddTagIfMissing("Package");
        AddTagIfMissing("Location");
    }

    private static void AddTagIfMissing(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
            {
                return;
            }
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void RunIfMarkerExists()
    {
        if (IsPlayModeBusy())
        {
            EditorApplication.delayCall += RunIfMarkerExists;
            return;
        }

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

        AddDeliveryCarGameToFoodIconOnly();
        AssetDatabase.Refresh();
    }

    private static bool IsPlayModeBusy()
    {
        return EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
