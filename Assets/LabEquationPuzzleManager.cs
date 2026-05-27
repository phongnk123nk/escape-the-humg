using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Lab/Lab Equation Puzzle Manager")]
public class LabEquationPuzzleManager : MonoBehaviour
{
    private static LabEquationPuzzleManager openPuzzleOwner;

    public static bool IsAnyEquationPuzzleOpen
    {
        get { return openPuzzleOwner != null; }
    }

    public LabInventorySystem inventoryManager;
    public GameObject puzzlePanel;
    public bool autoBuildPanel = true;
    public bool showAllEquationsAtOnce = true;
    public bool showEditModePreview = true;
    public UnityEvent OnPuzzleSolved;

    [Header("Runtime UI")]
    public TextMeshProUGUI equationText;
    public TextMeshProUGUI messageText;
    public Transform equationSlotRoot;
    public Transform inventoryDragRoot;
    public Button submitButton;
    public Button resetButton;
    public Button closeButton;
    public Button hintButton;
    public Button devSkipButton;
    public TMP_Dropdown equationDropdown;
    public float hintCooldownSeconds = 10f;
    public float hintHighlightDuration = 2.5f;

    [Header("Hint Message Preview")]
    [Tooltip("Ten object text goi y trong panel. Object nay hien trong Scene View de keo chinh vi tri.")]
    public string hintMessageObjectName = "HintMessageText";

    [Tooltip("Noi dung hien tam trong Edit Mode de de nhin va keo text, khi Play van duoc dieu khien bang SetMessage().")]
    public string hintMessagePreviewText = "Gợi ý: kéo dòng này để chỉnh vị trí";

    [Header("Puzzle Panel Scene Preview")]
    [Tooltip("Hien collider preview cua man giai do trong Scene View de keo chinh truc tiep.")]
    public bool showPuzzlePanelScenePreview = true;
    [Tooltip("Ten man preview se dat collider giai do len. Mac dinh la man may tinh.")]
    public string puzzlePanelPreviewViewName = "MachineView";
    [Tooltip("Ten object preview duoc tao trong Hierarchy.")]
    public string puzzlePanelPreviewObjectName = "PREVIEW Equation Puzzle Panel Collider";
    [Tooltip("Ti le doi tu pixel UI sang world unit trong preview.")]
    public float puzzlePanelPreviewPixelsPerUnit = 100f;
    [Tooltip("Vi tri preview cua khung giai do tinh theo local position cua man MachineView.")]
    public Vector2 puzzlePanelScenePreviewPosition = Vector2.zero;
    [Tooltip("Kich thuoc preview cua khung giai do tinh theo world unit.")]
    public Vector2 puzzlePanelScenePreviewSize = Vector2.zero;
    [Tooltip("Keo/resize collider preview se cap nhat kich thuoc khung puzzle khi Play.")]
    public bool syncPanelRectFromScenePreview = true;

    [Header("Dev Mode")]
    public bool devModeEnabled = false;
    public bool showDevSkipButton = true;

    [Header("Solved Reward")]
    public bool closePanelOnSolved = true;
    public bool grantRewardOnSolved = true;
    public LabInventorySystem.LabItemData solvedRewardItem = new LabInventorySystem.LabItemData
    {
        itemId = "lab_key",
        itemName = "Chia khoa",
        worldScale = Vector2.one,
        activeAtStart = false
    };

    [Header("Door Exit")]
    public bool createDoorExitZone = true;
    public string doorRequiredViewName = "BackView";
    public string doorRequiredItemId = "lab_key";
    public string nextSceneName = "BangXepHinh";
    public Vector2 doorExitPosition = new Vector2(0f, 0.55f);
    public Vector2 doorExitSize = new Vector2(1.45f, 2.35f);

    private readonly List<EquationDefinition> equations = new List<EquationDefinition>();
    private readonly List<EquationDropSlot> activeDropSlots = new List<EquationDropSlot>();
    private readonly List<int> currentCoefficients = new List<int>();
    private readonly List<int> activeAnswerCoefficients = new List<int>();

    private int currentEquationIndex;
    private bool solved;
    private bool rewardGranted;
    private bool buildingPanel;
    private Sprite roundedBoxSprite;
    private Texture2D roundedBoxTexture;
    private float lastHintTime = -999f;
    private GameObject doorExitPreviewObject;
    private Vector3 lastDoorExitPreviewLocalPosition;
    private Vector2 lastDoorExitPreviewColliderSize;
    private GameObject puzzlePanelPreviewObject;
    private Vector3 lastPuzzlePanelPreviewLocalPosition;
    private Vector2 lastPuzzlePanelPreviewColliderSize;

    private void Awake()
    {
        SetupPuzzle();
    }

    private void OnDestroy()
    {
        if (openPuzzleOwner == this)
        {
            openPuzzleOwner = null;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ClearDoorExitPreview();
            ClearPuzzlePanelScenePreview();
        }
#endif

        if (roundedBoxSprite != null)
        {
            DestroyGeneratedObject(roundedBoxSprite);
        }

        if (roundedBoxTexture != null)
        {
            DestroyGeneratedObject(roundedBoxTexture);
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += DelayedEditorSetup;
        }
#endif
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += DelayedEditorSetup;
        }
#endif
    }

#if UNITY_EDITOR
    private void DelayedEditorSetup()
    {
        if (this == null || Application.isPlaying)
        {
            return;
        }

        SetupPuzzle();
    }
#endif

    private void SetupPuzzle()
    {
        if (buildingPanel)
        {
            return;
        }

        BuildEquationDefinitions();
        EnsureReferences();
        AutoAssignRewardSprite();
        UpdateDoorExitPreview();
        HidePuzzlePanelScenePreviewInPlay();

        bool shouldBuildPanel = puzzlePanel != null && autoBuildPanel && (equationSlotRoot == null || puzzlePanel.transform.childCount == 0);
        if (shouldBuildPanel)
        {
            BuildDefaultPanel();
        }

        EnsurePanelUpgrades();
        UpdatePuzzlePanelScenePreview();
        WireButtons();

        if (puzzlePanel != null)
        {
            if (Application.isPlaying)
            {
                puzzlePanel.SetActive(false);
            }
            else
            {
                puzzlePanel.SetActive(showEditModePreview);
            }
        }
    }

    [ContextMenu("Refresh Equation Puzzle Preview")]
    private void RefreshEquationPuzzlePreview()
    {
        if (puzzlePanel == null)
        {
            Debug.LogError("PuzzlePanel is missing", this);
            return;
        }

        BuildDefaultPanel();
        UpdatePuzzlePanelScenePreview();
        WireButtons();

        if (!Application.isPlaying)
        {
            puzzlePanel.SetActive(showEditModePreview);
        }
    }

    public void OpenPuzzle()
    {
        EnsureReferences();

        if (puzzlePanel == null)
        {
            Debug.LogError("PuzzlePanel is missing", this);
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogError("LabInventoryManager is missing", this);
            return;
        }

        if (autoBuildPanel && equationSlotRoot == null)
        {
            BuildDefaultPanel();
        }

        EnsurePanelUpgrades();
        WireButtons();
        EnsureEventSystem();
        puzzlePanel.SetActive(true);
        openPuzzleOwner = this;
        inventoryManager.SetInventoryBarSuppressed(true);
        SelectEquation(currentEquationIndex);
        RebuildInventoryDragItems();
        SetMessage("");
    }

    public void ClosePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (openPuzzleOwner == this)
        {
            openPuzzleOwner = null;
        }

        if (inventoryManager != null)
        {
            inventoryManager.SetInventoryBarSuppressed(false);
        }
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public void Submit()
    {
        if (currentEquationIndex < 0 || currentEquationIndex >= equations.Count)
        {
            return;
        }

        bool allSubstancesCorrect = true;
        for (int i = 0; i < activeDropSlots.Count; i++)
        {
            if (!activeDropSlots[i].HasCorrectItem())
            {
                allSubstancesCorrect = false;
                break;
            }
        }

        if (!allSubstancesCorrect)
        {
            SetMessage("Một số chất chưa đúng");
            return;
        }

        for (int i = 0; i < activeAnswerCoefficients.Count; i++)
        {
            if (currentCoefficients[i] != activeAnswerCoefficients[i])
            {
                SetMessage("Phương trình chưa cân bằng");
                return;
            }
        }

        CompletePuzzle();
    }

    public void ResetPuzzle()
    {
        SelectEquation(currentEquationIndex);
        SetMessage("");
    }

    public bool IsSolved()
    {
        return solved;
    }

    public void DevSkipPuzzle()
    {
        if (!devModeEnabled)
        {
            SetMessage("Chế độ dev đang tắt");
            return;
        }

        CompletePuzzle();
    }

    private void EnsureReferences()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<LabInventorySystem>();
        }
    }

    private void AutoAssignRewardSprite()
    {
#if UNITY_EDITOR
        if (solvedRewardItem == null)
        {
            solvedRewardItem = new LabInventorySystem.LabItemData();
        }

        if (string.IsNullOrWhiteSpace(solvedRewardItem.itemId))
        {
            solvedRewardItem.itemId = "lab_key";
        }

        if (string.IsNullOrWhiteSpace(solvedRewardItem.itemName))
        {
            solvedRewardItem.itemName = "Chia khoa";
        }

        if (solvedRewardItem.worldSprite != null || solvedRewardItem.inventorySprite != null)
        {
            return;
        }

        Sprite keySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/image/phong thi nghiem/chia khoa/chia khoa.png");
        if (keySprite == null)
        {
            return;
        }

        solvedRewardItem.worldSprite = keySprite;
        solvedRewardItem.inventorySprite = keySprite;
        EditorUtility.SetDirty(this);
#endif
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            EnsureDoorExitZone();
        }
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateDoorExitPreview();
            UpdatePuzzlePanelScenePreview();
        }
#endif
    }

    private void CompletePuzzle()
    {
        if (solved)
        {
            return;
        }

        solved = true;
        SetMessage("Đã cân bằng thành công");
        OnPuzzleSolved.Invoke();

        if (Application.isPlaying)
        {
            StartCoroutine(CompletePuzzleRoutine());
        }
    }

    private IEnumerator CompletePuzzleRoutine()
    {
        yield return new WaitForSeconds(0.15f);

        if (closePanelOnSolved)
        {
            ClosePuzzle();
        }

        if (!grantRewardOnSolved || rewardGranted)
        {
            yield break;
        }

        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<LabInventorySystem>();
        }

        if (inventoryManager == null)
        {
            Debug.LogError("LabInventoryManager is missing", this);
            yield break;
        }

        if (solvedRewardItem == null || (solvedRewardItem.worldSprite == null && solvedRewardItem.inventorySprite == null))
        {
            Debug.LogError("Solved reward key sprite is missing", this);
            yield break;
        }

        rewardGranted = true;
        inventoryManager.AwardItemFromScreenCenter(solvedRewardItem);
    }

    private void EnsureDoorExitZone()
    {
        if (!createDoorExitZone)
        {
            return;
        }

        LabKeyDoorExit existingDoorExit = FindFirstObjectByType<LabKeyDoorExit>(FindObjectsInactive.Include);
        if (existingDoorExit != null)
        {
            return;
        }

        GameObject zoneObject = new GameObject("Lab Key Door Exit Zone");
        zoneObject.transform.position = new Vector3(doorExitPosition.x, doorExitPosition.y, 0f);

        BoxCollider2D collider = zoneObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = doorExitSize;

        LabKeyDoorExit doorExit = zoneObject.AddComponent<LabKeyDoorExit>();
        doorExit.inventoryManager = inventoryManager;
        doorExit.sceneNavigator = FindFirstObjectByType<LabSceneNavigator>();
        doorExit.requiredViewName = doorRequiredViewName;
        doorExit.requiredItemId = doorRequiredItemId;
        doorExit.nextSceneName = nextSceneName;
    }

    private void UpdateDoorExitPreview()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            return;
        }

        if (!createDoorExitZone || string.IsNullOrWhiteSpace(doorRequiredViewName))
        {
            ClearDoorExitPreview();
            return;
        }

        LabSceneNavigator navigator = FindFirstObjectByType<LabSceneNavigator>();
        if (navigator == null)
        {
            ClearDoorExitPreview();
            return;
        }

        Transform previewRoot = FindPreviewScreenRoot(navigator.transform, doorRequiredViewName);
        if (previewRoot == null)
        {
            ClearDoorExitPreview();
            return;
        }

        if (doorExitPreviewObject == null || doorExitPreviewObject.transform.parent != previewRoot)
        {
            ClearDoorExitPreview();
            doorExitPreviewObject = new GameObject("PREVIEW Door Exit Collider - " + doorRequiredViewName);
            doorExitPreviewObject.transform.SetParent(previewRoot, false);
            doorExitPreviewObject.transform.localPosition = new Vector3(doorExitPosition.x, doorExitPosition.y, 0f);
            doorExitPreviewObject.transform.localScale = Vector3.one;
            doorExitPreviewObject.tag = "EditorOnly";

            BoxCollider2D box = doorExitPreviewObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = doorExitSize;
            LabBoxColliderPreview.Attach(
                box,
                new Color(1f, 0.85f, 0.15f, 0.22f),
                new Color(1f, 0.85f, 0.15f, 0.95f),
                "Door Exit Drop");

            RememberDoorExitPreview(box);
            return;
        }

        BoxCollider2D collider = doorExitPreviewObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            ClearDoorExitPreview();
            return;
        }

        bool moved = doorExitPreviewObject.transform.hasChanged
            || Vector3.SqrMagnitude(doorExitPreviewObject.transform.localPosition - lastDoorExitPreviewLocalPosition) > 0.000001f;
        bool resized = Vector2.SqrMagnitude(collider.size - lastDoorExitPreviewColliderSize) > 0.000001f;

        if (moved || resized)
        {
            Undo.RecordObject(this, "Move Door Exit Collider Preview");
            doorExitPosition = new Vector2(doorExitPreviewObject.transform.localPosition.x, doorExitPreviewObject.transform.localPosition.y);
            doorExitSize = new Vector2(Mathf.Max(0.05f, collider.size.x), Mathf.Max(0.05f, collider.size.y));
            collider.size = doorExitSize;
            RememberDoorExitPreview(collider);
            EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
        else if (new Vector2(doorExitPreviewObject.transform.localPosition.x, doorExitPreviewObject.transform.localPosition.y) != doorExitPosition
            || collider.size != doorExitSize)
        {
            doorExitPreviewObject.transform.localPosition = new Vector3(doorExitPosition.x, doorExitPosition.y, 0f);
            collider.size = doorExitSize;
            RememberDoorExitPreview(collider);
        }
#endif
    }

#if UNITY_EDITOR
    private Transform FindPreviewScreenRoot(Transform root, string screenName)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name.StartsWith("PREVIEW ", StringComparison.OrdinalIgnoreCase)
                && child.name.IndexOf(screenName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return child;
            }

            Transform nested = FindPreviewScreenRoot(child, screenName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private void RememberDoorExitPreview(BoxCollider2D collider)
    {
        if (doorExitPreviewObject == null || collider == null)
        {
            return;
        }

        lastDoorExitPreviewLocalPosition = doorExitPreviewObject.transform.localPosition;
        lastDoorExitPreviewColliderSize = collider.size;
        doorExitPreviewObject.transform.hasChanged = false;
    }

    private void ClearDoorExitPreview()
    {
        if (doorExitPreviewObject != null)
        {
            DestroyGeneratedObject(doorExitPreviewObject);
            doorExitPreviewObject = null;
        }
    }
#endif

    private void HidePuzzlePanelScenePreviewInPlay()
    {
        if (!Application.isPlaying || string.IsNullOrWhiteSpace(puzzlePanelPreviewObjectName))
        {
            return;
        }

        Transform preview = null;
        if (puzzlePanelPreviewObject != null)
        {
            preview = puzzlePanelPreviewObject.transform;
        }

        if (preview == null)
        {
            LabSceneNavigator navigator = FindFirstObjectByType<LabSceneNavigator>(FindObjectsInactive.Include);
            if (navigator != null)
            {
                preview = FindChildByName(navigator.transform, puzzlePanelPreviewObjectName);
            }
        }

        if (preview == null && puzzlePanel != null)
        {
            preview = FindChildByName(puzzlePanel.transform, puzzlePanelPreviewObjectName);
        }

        if (preview != null)
        {
            preview.gameObject.SetActive(false);
        }
    }

    private void UpdatePuzzlePanelScenePreview()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            HidePuzzlePanelScenePreviewInPlay();
            return;
        }

        if (puzzlePanel == null || !showPuzzlePanelScenePreview)
        {
            if (puzzlePanelPreviewObject != null)
            {
                puzzlePanelPreviewObject.SetActive(false);
            }

            return;
        }

        Transform frame = puzzlePanel.transform.Find("Glass Equation Module");
        if (frame == null)
        {
            frame = FindChildByName(puzzlePanel.transform, "Glass Equation Module");
        }

        InitializePuzzlePanelPreviewValues(frame as RectTransform);

        LabSceneNavigator navigator = FindFirstObjectByType<LabSceneNavigator>();
        Transform previewRoot = navigator != null
            ? FindPreviewScreenRoot(navigator.transform, puzzlePanelPreviewViewName)
            : null;

        if (previewRoot == null)
        {
            previewRoot = transform;
        }

        bool initializedPreviewObject = false;
        if (puzzlePanelPreviewObject == null || puzzlePanelPreviewObject.transform.parent != previewRoot)
        {
            if (puzzlePanelPreviewObject != null)
            {
                puzzlePanelPreviewObject.SetActive(false);
            }

            Transform existingPreview = previewRoot.Find(puzzlePanelPreviewObjectName);
            if (existingPreview != null)
            {
                puzzlePanelPreviewObject = existingPreview.gameObject;
            }
            else
            {
                puzzlePanelPreviewObject = new GameObject(puzzlePanelPreviewObjectName);
                puzzlePanelPreviewObject.transform.SetParent(previewRoot, false);
                puzzlePanelPreviewObject.tag = "EditorOnly";
            }

            puzzlePanelPreviewObject.transform.localPosition = new Vector3(
                puzzlePanelScenePreviewPosition.x,
                puzzlePanelScenePreviewPosition.y,
                0f);
            puzzlePanelPreviewObject.transform.localScale = Vector3.one;
            initializedPreviewObject = true;
        }

        puzzlePanelPreviewObject.SetActive(true);

        BoxCollider2D collider = puzzlePanelPreviewObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = puzzlePanelPreviewObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        if (puzzlePanelScenePreviewSize.x <= 0.01f || puzzlePanelScenePreviewSize.y <= 0.01f)
        {
            puzzlePanelScenePreviewSize = new Vector2(7.6f, 3.6f);
        }

        LabBoxColliderPreview.Attach(
            collider,
            new Color(0.15f, 0.75f, 1f, 0.18f),
            new Color(0.1f, 0.9f, 1f, 0.95f),
            "Equation Puzzle Panel");

        if (initializedPreviewObject || lastPuzzlePanelPreviewColliderSize == Vector2.zero)
        {
            puzzlePanelPreviewObject.transform.localPosition = new Vector3(
                puzzlePanelScenePreviewPosition.x,
                puzzlePanelScenePreviewPosition.y,
                0f);
            collider.size = puzzlePanelScenePreviewSize;
            RememberPuzzlePanelScenePreview(collider);
            return;
        }

        bool moved = puzzlePanelPreviewObject.transform.hasChanged
            || Vector3.SqrMagnitude(puzzlePanelPreviewObject.transform.localPosition - lastPuzzlePanelPreviewLocalPosition) > 0.000001f;
        bool resized = Vector2.SqrMagnitude(collider.size - lastPuzzlePanelPreviewColliderSize) > 0.000001f;

        if (moved || resized)
        {
            Undo.RecordObject(this, "Move Equation Puzzle Preview");
            puzzlePanelScenePreviewPosition = new Vector2(
                puzzlePanelPreviewObject.transform.localPosition.x,
                puzzlePanelPreviewObject.transform.localPosition.y);
            puzzlePanelScenePreviewSize = new Vector2(
                Mathf.Max(0.05f, collider.size.x),
                Mathf.Max(0.05f, collider.size.y));
            collider.size = puzzlePanelScenePreviewSize;

            if (syncPanelRectFromScenePreview)
            {
                ApplyPuzzlePanelPreviewToFrame(frame as RectTransform);
            }

            RememberPuzzlePanelScenePreview(collider);
            EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
        else if (new Vector2(puzzlePanelPreviewObject.transform.localPosition.x, puzzlePanelPreviewObject.transform.localPosition.y) != puzzlePanelScenePreviewPosition
            || collider.size != puzzlePanelScenePreviewSize)
        {
            puzzlePanelPreviewObject.transform.localPosition = new Vector3(
                puzzlePanelScenePreviewPosition.x,
                puzzlePanelScenePreviewPosition.y,
                0f);
            collider.size = puzzlePanelScenePreviewSize;

            if (syncPanelRectFromScenePreview)
            {
                ApplyPuzzlePanelPreviewToFrame(frame as RectTransform);
            }

            RememberPuzzlePanelScenePreview(collider);
        }
#endif
    }

#if UNITY_EDITOR
    private void InitializePuzzlePanelPreviewValues(RectTransform frameRect)
    {
        float pixelsPerUnit = Mathf.Max(1f, puzzlePanelPreviewPixelsPerUnit);
        if (Mathf.Abs(pixelsPerUnit - puzzlePanelPreviewPixelsPerUnit) > 0.001f)
        {
            puzzlePanelPreviewPixelsPerUnit = pixelsPerUnit;
        }

        if (frameRect == null)
        {
            if (puzzlePanelScenePreviewSize.x <= 0.01f || puzzlePanelScenePreviewSize.y <= 0.01f)
            {
                puzzlePanelScenePreviewSize = new Vector2(7.6f, 3.6f);
            }

            return;
        }

        if (puzzlePanelScenePreviewSize.x <= 0.01f || puzzlePanelScenePreviewSize.y <= 0.01f)
        {
            puzzlePanelScenePreviewSize = new Vector2(
                Mathf.Max(0.05f, frameRect.sizeDelta.x / pixelsPerUnit),
                Mathf.Max(0.05f, frameRect.sizeDelta.y / pixelsPerUnit));
        }

        if (puzzlePanelScenePreviewPosition == Vector2.zero && frameRect.anchoredPosition.sqrMagnitude > 0.000001f)
        {
            puzzlePanelScenePreviewPosition = frameRect.anchoredPosition / pixelsPerUnit;
        }
    }

    private void ApplyPuzzlePanelPreviewToFrame(RectTransform frameRect)
    {
        if (frameRect == null)
        {
            return;
        }

        float pixelsPerUnit = Mathf.Max(1f, puzzlePanelPreviewPixelsPerUnit);
        Vector2 targetPosition = puzzlePanelScenePreviewPosition * pixelsPerUnit;
        Vector2 targetSize = puzzlePanelScenePreviewSize * pixelsPerUnit;

        if (Vector2.SqrMagnitude(frameRect.anchoredPosition - targetPosition) <= 0.000001f
            && Vector2.SqrMagnitude(frameRect.sizeDelta - targetSize) <= 0.000001f)
        {
            return;
        }

        Undo.RecordObject(frameRect, "Sync Equation Puzzle Panel Preview");
        frameRect.anchoredPosition = targetPosition;
        frameRect.sizeDelta = targetSize;
        EditorUtility.SetDirty(frameRect);
    }

    private void RememberPuzzlePanelScenePreview(BoxCollider2D collider)
    {
        if (puzzlePanelPreviewObject == null || collider == null)
        {
            return;
        }

        lastPuzzlePanelPreviewLocalPosition = puzzlePanelPreviewObject.transform.localPosition;
        lastPuzzlePanelPreviewColliderSize = collider.size;
        puzzlePanelPreviewObject.transform.hasChanged = false;
    }

    private void ClearPuzzlePanelScenePreview()
    {
        if (puzzlePanelPreviewObject != null)
        {
            DestroyGeneratedObject(puzzlePanelPreviewObject);
            puzzlePanelPreviewObject = null;
        }
    }
#endif

    private void EnsurePanelUpgrades()
    {
        if (puzzlePanel == null)
        {
            return;
        }

        Button[] buttons = puzzlePanel.GetComponentsInChildren<Button>(true);
        if (hintButton == null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && (buttons[i].name.Contains("GOI Y") || buttons[i].name.Contains("GỢI Ý")))
                {
                    hintButton = buttons[i];
                    break;
                }
            }
        }

        if (devSkipButton == null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && (buttons[i].name.Contains("DEV SKIP") || buttons[i].name.Contains("BỎ QUA")))
                {
                    devSkipButton = buttons[i];
                    break;
                }
            }
        }

        Transform frame = puzzlePanel.transform.Find("Glass Equation Module");
        if (frame == null)
        {
            frame = FindChildByName(puzzlePanel.transform, "Glass Equation Module");
        }

        if (frame == null)
        {
            return;
        }

        if (hintButton == null)
        {
            hintButton = CreateButton("GỢI Ý", frame, new Vector2(82f, 34f));
            PlaceBottomRightButton(hintButton, -220f);
        }

        if (devSkipButton == null)
        {
            devSkipButton = CreateButton("BỎ QUA", frame, new Vector2(112f, 34f));
            PlaceBottomRightButton(devSkipButton, -310f);
        }

        NormalizeStepperButtons(frame);
        EnsureHintMessageText(frame);
        devSkipButton.gameObject.SetActive(devModeEnabled && showDevSkipButton);
    }

    private void NormalizeStepperButtons(Transform root)
    {
        if (root == null)
        {
            return;
        }

        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || button.transform.parent == null)
            {
                continue;
            }

            if (!button.transform.parent.name.Contains("Coefficient Stepper"))
            {
                continue;
            }

            bool isUpButton = button.transform.GetSiblingIndex() == 0;
            string labelText = isUpButton ? "^" : "v";
            button.name = isUpButton ? "Increase Coefficient Button" : "Decrease Coefficient Button";

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = labelText;
                label.fontSize = 11f;
                label.enableAutoSizing = false;
                label.color = Color.black;
                label.fontStyle = FontStyles.Bold;
            }
        }
    }

    private void EnsureHintMessageText(Transform frame)
    {
        if (frame == null)
        {
            return;
        }

        if (messageText == null)
        {
            Transform existing = FindChildByName(frame, hintMessageObjectName);
            if (existing != null)
            {
                messageText = existing.GetComponent<TextMeshProUGUI>();
            }
        }

        if (messageText == null)
        {
            messageText = CreateText(Application.isPlaying ? "" : hintMessagePreviewText, frame, 20, TextAlignmentOptions.Center);
            messageText.color = Color.black;
            RectTransform rect = messageText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(-205f, 18f);
            rect.sizeDelta = new Vector2(360f, 34f);
        }

        messageText.name = hintMessageObjectName;
        messageText.raycastTarget = false;
        messageText.fontSize = 15f;
        messageText.enableAutoSizing = true;
        messageText.fontSizeMin = 9f;
        messageText.fontSizeMax = 15f;

        if (!Application.isPlaying && string.IsNullOrWhiteSpace(messageText.text))
        {
            messageText.text = hintMessagePreviewText;
        }

        RectTransform messageRect = messageText.rectTransform;
        if (IsLegacyOverlappingMessagePosition(messageRect))
        {
            messageRect.anchoredPosition = new Vector2(-205f, 18f);
            messageRect.sizeDelta = new Vector2(360f, 34f);
        }
    }

    private bool IsLegacyOverlappingMessagePosition(RectTransform rect)
    {
        if (rect == null)
        {
            return false;
        }

        return Mathf.Abs(rect.anchoredPosition.x) < 0.01f
            && Mathf.Abs(rect.anchoredPosition.y - 22f) < 0.01f
            && Mathf.Abs(rect.sizeDelta.x - 420f) < 0.01f;
    }

    private Transform FindChildByName(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindChildByName(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private void WireButtons()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(Submit);
            submitButton.onClick.AddListener(Submit);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetPuzzle);
            resetButton.onClick.AddListener(ResetPuzzle);
        }

        if (hintButton != null)
        {
            hintButton.onClick.RemoveListener(ShowNextHint);
            hintButton.onClick.AddListener(ShowNextHint);
        }

        if (devSkipButton != null)
        {
            devSkipButton.onClick.RemoveListener(DevSkipPuzzle);
            devSkipButton.onClick.AddListener(DevSkipPuzzle);
            devSkipButton.gameObject.SetActive(devModeEnabled && showDevSkipButton);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePuzzle);
            closeButton.onClick.AddListener(ClosePuzzle);
        }

        if (equationDropdown != null)
        {
            equationDropdown.onValueChanged.RemoveListener(SelectEquation);
            equationDropdown.onValueChanged.AddListener(SelectEquation);
        }
    }

    public void ShowNextHint()
    {
        if (Time.time - lastHintTime < hintCooldownSeconds)
        {
            float remaining = Mathf.Ceil(hintCooldownSeconds - (Time.time - lastHintTime));
            SetMessage("Chờ " + remaining + " giây để gợi ý tiếp");
            return;
        }

        lastHintTime = Time.time;

        for (int i = 0; i < activeDropSlots.Count; i++)
        {
            EquationDropSlot slot = activeDropSlots[i];
            if (slot == null || slot.HasCorrectItem())
            {
                continue;
            }

            if (HighlightInventoryItem(slot.requiredItemId))
            {
                SetMessage("Gợi ý: hãy tìm chất đang sáng trong kho đồ");
            }
            else
            {
                SetMessage("Chưa có chất cần thiết trong kho đồ");
            }

            return;
        }

        SetMessage("Tất cả chất đã đúng");
    }

    private bool HighlightInventoryItem(string requiredItemId)
    {
        if (inventoryDragRoot == null || string.IsNullOrWhiteSpace(requiredItemId))
        {
            return false;
        }

        InventoryDragItem[] dragItems = inventoryDragRoot.GetComponentsInChildren<InventoryDragItem>(true);
        for (int i = 0; i < dragItems.Length; i++)
        {
            if (dragItems[i] != null && string.Equals(dragItems[i].itemId, requiredItemId, StringComparison.OrdinalIgnoreCase))
            {
                dragItems[i].HighlightHint(hintHighlightDuration);
                return true;
            }
        }

        return false;
    }

    private void SelectEquation(int equationIndex)
    {
        currentEquationIndex = Mathf.Clamp(equationIndex, 0, equations.Count - 1);
        EquationDefinition equation = equations[currentEquationIndex];

        if (equationText != null)
        {
            equationText.text = equation.displayEquation;
        }

        ClearChildren(equationSlotRoot);
        activeDropSlots.Clear();
        currentCoefficients.Clear();
        activeAnswerCoefficients.Clear();

        if (showAllEquationsAtOnce)
        {
            for (int i = 0; i < equations.Count; i++)
            {
                CreateEquationRow(equations[i], i);
            }

            return;
        }

        for (int i = 0; i < equation.terms.Count; i++)
        {
            if (i > 0)
            {
                CreateOperatorLabel(i == equation.productStartIndex ? "->" : "+");
            }

            EquationTerm term = equation.terms[i];
            CreateTermControl(term, i);
        }
    }

    private void CreateEquationRow(EquationDefinition equation, int rowIndex)
    {
        GameObject rowObject = CreateUIObject("Equation Row " + (rowIndex + 1), equationSlotRoot);
        RectTransform rowRect = rowObject.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(690f, 54f);

        HorizontalLayoutGroup rowLayout = rowObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.spacing = 5f;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;

        for (int i = 0; i < equation.terms.Count; i++)
        {
            if (i > 0)
            {
                CreateOperatorLabel(i == equation.productStartIndex ? "->" : "+", rowObject.transform);
            }

            CreateTermControl(equation.terms[i], currentCoefficients.Count, rowObject.transform);
        }
    }

    private void CreateTermControl(EquationTerm term, int termIndex)
    {
        CreateTermControl(term, termIndex, equationSlotRoot);
    }

    private void CreateTermControl(EquationTerm term, int termIndex, Transform parent)
    {
        currentCoefficients.Add(1);
        activeAnswerCoefficients.Add(term.answerCoefficient);

        GameObject root = CreateUIObject("Term - " + term.label, parent);
        HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 4f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        GameObject coefficientBox = CreateUIObject("Coefficient", root.transform);
        RectTransform coefficientRect = coefficientBox.AddComponent<RectTransform>();
        coefficientRect.sizeDelta = new Vector2(34f, 34f);
        Image coefficientImage = coefficientBox.AddComponent<Image>();
        coefficientImage.sprite = GetRoundedBoxSprite();
        coefficientImage.type = Image.Type.Sliced;
        coefficientImage.color = new Color(0.9f, 0.9f, 0.88f, 0.95f);
        TextMeshProUGUI coefficientText = CreateText("1", coefficientBox.transform, 22, TextAlignmentOptions.Center);
        coefficientText.color = Color.black;
        coefficientText.fontStyle = FontStyles.Bold;
        StretchToParent(coefficientText.rectTransform, Vector2.zero, Vector2.zero);
        coefficientText.raycastTarget = false;

        GameObject stepperObject = CreateUIObject("Coefficient Stepper", root.transform);
        RectTransform stepperRect = stepperObject.AddComponent<RectTransform>();
        stepperRect.sizeDelta = new Vector2(14f, 34f);
        VerticalLayoutGroup stepperLayout = stepperObject.AddComponent<VerticalLayoutGroup>();
        stepperLayout.spacing = 2f;
        stepperLayout.childControlWidth = true;
        stepperLayout.childControlHeight = true;
        stepperLayout.childForceExpandWidth = true;
        stepperLayout.childForceExpandHeight = true;

        Button upButton = CreateStepperButton("^", stepperObject.transform);
        Button downButton = CreateStepperButton("v", stepperObject.transform);

        GameObject slotObject = CreateUIObject("Drop Slot - " + term.label, root.transform);
        RectTransform slotRect = slotObject.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(48f, 42f);
        Image border = slotObject.AddComponent<Image>();
        border.sprite = GetRoundedBoxSprite();
        border.type = Image.Type.Sliced;
        border.color = new Color(0.9f, 0.9f, 0.88f, 0.95f);

        GameObject iconObject = CreateUIObject("Icon", slotObject.transform);
        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(4f, 4f);
        iconRect.offsetMax = new Vector2(-4f, -12f);
        Image icon = iconObject.AddComponent<Image>();
        icon.enabled = false;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        TextMeshProUGUI label = CreateText("", slotObject.transform, 8, TextAlignmentOptions.Center);
        label.color = Color.black;
        label.enableAutoSizing = true;
        label.fontSizeMin = 5f;
        label.fontSizeMax = 8f;
        label.rectTransform.anchorMin = new Vector2(0f, 0f);
        label.rectTransform.anchorMax = new Vector2(1f, 0f);
        label.rectTransform.pivot = new Vector2(0.5f, 0f);
        label.rectTransform.anchoredPosition = new Vector2(0f, 1f);
        label.rectTransform.sizeDelta = new Vector2(0f, 10f);
        label.enabled = false;

        EquationDropSlot dropSlot = slotObject.AddComponent<EquationDropSlot>();
        dropSlot.requiredItemId = term.requiredItemId;
        dropSlot.nearCorrectItemIds = new List<string>(term.nearCorrectItemIds);
        dropSlot.slotIcon = icon;
        dropSlot.slotLabel = label;
        dropSlot.slotBorder = border;
        activeDropSlots.Add(dropSlot);

        Button coefficientButton = coefficientBox.AddComponent<Button>();
        coefficientButton.onClick.AddListener(() => ChangeCoefficient(termIndex, 1, coefficientText));
        upButton.onClick.AddListener(() => ChangeCoefficient(termIndex, 1, coefficientText));
        downButton.onClick.AddListener(() => ChangeCoefficient(termIndex, -1, coefficientText));
    }

    private void CreateOperatorLabel(string symbol)
    {
        CreateOperatorLabel(symbol, equationSlotRoot);
    }

    private void CreateOperatorLabel(string symbol, Transform parent)
    {
        TextMeshProUGUI operatorText = CreateText(symbol, parent, 44, TextAlignmentOptions.Center);
        operatorText.color = Color.black;
        operatorText.fontStyle = FontStyles.Bold;
        operatorText.rectTransform.sizeDelta = symbol == "->" ? new Vector2(54f, 48f) : new Vector2(28f, 48f);
    }

    private void ChangeCoefficient(int termIndex, int delta, TextMeshProUGUI coefficientText)
    {
        currentCoefficients[termIndex] = Mathf.Clamp(currentCoefficients[termIndex] + delta, 1, 99);
        coefficientText.text = currentCoefficients[termIndex].ToString();
    }

    private void HandleFormulaInput(int fallbackEquationIndex, string input)
    {
        string normalizedInput = NormalizeFormula(input);
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            SelectEquation(fallbackEquationIndex);
            SetMessage("");
            return;
        }

        for (int i = 0; i < equations.Count; i++)
        {
            if (NormalizeFormula(equations[i].displayEquation) == normalizedInput)
            {
                SelectEquation(i);
                SetMessage("");
                return;
            }
        }

        SelectEquation(fallbackEquationIndex);
        SetMessage("Không nhận ra công thức");
    }

    private string NormalizeFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return "";
        }

        return formula
            .Replace(" ", "")
            .Replace("=", "->")
            .Replace("â†’", "->")
            .Replace("â€“", "-")
            .Replace("â€”", "-")
            .ToUpperInvariant();
    }

    private void RebuildInventoryDragItems()
    {
        if (inventoryDragRoot == null || inventoryManager == null)
        {
            return;
        }

        ClearChildren(inventoryDragRoot);
        List<LabInventorySystem.LabItemData> collectedItems = inventoryManager.GetCollectedItems();

        for (int i = 0; i < collectedItems.Count; i++)
        {
            LabInventorySystem.LabItemData item = collectedItems[i];
            Sprite iconSprite = item.inventorySprite != null ? item.inventorySprite : item.worldSprite;

            GameObject itemObject = CreateUIObject("Drag Item - " + item.itemId, inventoryDragRoot);
            RectTransform rect = itemObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(30f, 30f);
            Image image = itemObject.AddComponent<Image>();
            image.sprite = iconSprite;
            image.preserveAspect = true;
            image.color = Color.white;

            InventoryDragItem dragItem = itemObject.AddComponent<InventoryDragItem>();
            dragItem.SetItem(item.itemId, item.itemName, iconSprite);
        }
    }

    private void BuildDefaultPanel()
    {
        buildingPanel = true;
        EnsureEventSystem();
        EnsurePanelCanvas();
        ClearChildren(puzzlePanel.transform);

        RectTransform panelRect = EnsureRectTransform(puzzlePanel);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image overlay = puzzlePanel.GetComponent<Image>();
        if (overlay == null)
        {
            overlay = puzzlePanel.AddComponent<Image>();
        }

        overlay.color = new Color(0f, 0f, 0f, 0.28f);

        GameObject frame = CreateUIObject("Glass Equation Module", puzzlePanel.transform);
        RectTransform frameRect = frame.AddComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.pivot = new Vector2(0.5f, 0.5f);
        frameRect.anchoredPosition = new Vector2(0f, 36f);
        frameRect.sizeDelta = new Vector2(760f, 360f);
        Image frameImage = frame.AddComponent<Image>();
        frameImage.color = new Color(0.78f, 0.82f, 0.82f, 0.58f);

        closeButton = CreateButton("X", frame.transform, new Vector2(34f, 30f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-10f, -10f);

        TextMeshProUGUI title = CreateText("CÂN BẰNG PHƯƠNG TRÌNH", frame.transform, 18, TextAlignmentOptions.Center);
        title.color = new Color(0f, 0f, 0f, 0.42f);
        title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.anchoredPosition = new Vector2(0f, -18f);
        title.text = "CÂN BẰNG PHƯƠNG TRÌNH";
        title.rectTransform.sizeDelta = new Vector2(520f, 28f);

        GameObject slotsRootObject = CreateUIObject("Equation Slots", frame.transform);
        RectTransform slotsRect = slotsRootObject.AddComponent<RectTransform>();
        slotsRect.anchorMin = new Vector2(0.5f, 0.63f);
        slotsRect.anchorMax = new Vector2(0.5f, 0.63f);
        slotsRect.pivot = new Vector2(0.5f, 0.5f);
        slotsRect.anchoredPosition = Vector2.zero;
        slotsRect.sizeDelta = new Vector2(720f, 178f);
        equationSlotRoot = slotsRootObject.transform;
        VerticalLayoutGroup slotsLayout = slotsRootObject.AddComponent<VerticalLayoutGroup>();
        slotsLayout.childAlignment = TextAnchor.MiddleCenter;
        slotsLayout.spacing = 8f;
        slotsLayout.childControlWidth = false;
        slotsLayout.childControlHeight = false;

        GameObject inventoryBack = CreateUIObject("Bottom Inventory Glass", puzzlePanel.transform);
        RectTransform inventoryBackRect = inventoryBack.AddComponent<RectTransform>();
        inventoryBackRect.anchorMin = new Vector2(0.5f, 0f);
        inventoryBackRect.anchorMax = new Vector2(0.5f, 0f);
        inventoryBackRect.pivot = new Vector2(0.5f, 0f);
        inventoryBackRect.anchoredPosition = new Vector2(0f, 34f);
        inventoryBackRect.sizeDelta = new Vector2(540f, 54f);
        Image inventoryBackImage = inventoryBack.AddComponent<Image>();
        inventoryBackImage.color = new Color(0.18f, 0.2f, 0.2f, 0.5f);
        inventoryBack.AddComponent<RectMask2D>();

        GameObject inventoryRootObject = CreateUIObject("Inventory Drag Items", inventoryBack.transform);
        RectTransform inventoryRect = inventoryRootObject.AddComponent<RectTransform>();
        StretchToParent(inventoryRect, new Vector2(8f, 6f), new Vector2(-8f, -6f));
        inventoryDragRoot = inventoryRootObject.transform;
        HorizontalLayoutGroup inventoryLayout = inventoryRootObject.AddComponent<HorizontalLayoutGroup>();
        inventoryLayout.spacing = 4f;
        inventoryLayout.childAlignment = TextAnchor.MiddleCenter;
        inventoryLayout.childControlWidth = false;
        inventoryLayout.childControlHeight = false;

        submitButton = CreateButton("NỘP", frame.transform, new Vector2(86f, 34f));
        RectTransform submitRect = submitButton.GetComponent<RectTransform>();
        submitRect.anchorMin = new Vector2(1f, 0f);
        submitRect.anchorMax = new Vector2(1f, 0f);
        submitRect.pivot = new Vector2(1f, 0f);
        submitRect.anchoredPosition = new Vector2(-18f, 18f);

        resetButton = CreateButton("LÀM LẠI", frame.transform, new Vector2(100f, 34f));
        RectTransform resetRect = resetButton.GetComponent<RectTransform>();
        resetRect.anchorMin = new Vector2(1f, 0f);
        resetRect.anchorMax = new Vector2(1f, 0f);
        resetRect.pivot = new Vector2(1f, 0f);
        resetRect.anchoredPosition = new Vector2(-112f, 18f);

        hintButton = CreateButton("GỢI Ý", frame.transform, new Vector2(82f, 34f));
        PlaceBottomRightButton(hintButton, -220f);

        devSkipButton = CreateButton("BỎ QUA", frame.transform, new Vector2(112f, 34f));
        PlaceBottomRightButton(devSkipButton, -310f);
        devSkipButton.gameObject.SetActive(devModeEnabled && showDevSkipButton);

        messageText = CreateText(Application.isPlaying ? "" : hintMessagePreviewText, frame.transform, 20, TextAlignmentOptions.Center);
        messageText.name = hintMessageObjectName;
        messageText.color = Color.black;
        messageText.raycastTarget = false;
        messageText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        messageText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        messageText.rectTransform.pivot = new Vector2(0.5f, 0f);
        messageText.rectTransform.anchoredPosition = new Vector2(-205f, 18f);
        messageText.rectTransform.sizeDelta = new Vector2(360f, 34f);

        SelectEquation(0);
        buildingPanel = false;
    }

    private void EnsurePanelCanvas()
    {
        if (puzzlePanel == null)
        {
            return;
        }

        Canvas canvas = puzzlePanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Equation Puzzle Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();
        puzzlePanel.transform.SetParent(canvasObject.transform, false);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (existingEventSystem != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void BuildEquationDefinitions()
    {
        if (equations.Count > 0)
        {
            return;
        }

        equations.Add(new EquationDefinition("C3H8O3 + O2 -> CO2 + H2O", 2,
            new EquationTerm("C3H8O3", "glycerol", 1),
            new EquationTerm("O2", "oxygen", 7, "peroxide", "ozone"),
            new EquationTerm("CO2", "carbon_dioxide", 3, "carbon_monoxide"),
            new EquationTerm("H2O", "water", 4, "hydrogen_peroxide")));

        equations.Add(new EquationDefinition("C12H22O11 + O2 -> CO2 + H2O", 2,
            new EquationTerm("C12H22O11", "sucrose", 1),
            new EquationTerm("O2", "oxygen", 12, "peroxide", "ozone"),
            new EquationTerm("CO2", "carbon_dioxide", 12, "carbon_monoxide"),
            new EquationTerm("H2O", "water", 11, "hydrogen_peroxide")));

        equations.Add(new EquationDefinition("C10H14N2 + O2 -> CO2 + H2O + NO2", 2,
            new EquationTerm("C10H14N2", "nicotine", 2),
            new EquationTerm("O2", "oxygen", 27, "peroxide", "ozone"),
            new EquationTerm("CO2", "carbon_dioxide", 20, "carbon_monoxide"),
            new EquationTerm("H2O", "water", 14, "hydrogen_peroxide"),
            new EquationTerm("NO2", "nitrogen_dioxide", 4, "nitric_oxide")));
    }

    private Button CreateButton(string text, Transform parent, Vector2 size)
    {
        GameObject buttonObject = CreateUIObject(text + " Button", parent);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedBoxSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(0.9f, 0.9f, 0.86f, 0.72f);
        Button button = buttonObject.AddComponent<Button>();
        TextMeshProUGUI label = CreateText(text, buttonObject.transform, 18, TextAlignmentOptions.Center);
        label.color = Color.black;
        label.fontStyle = FontStyles.Bold;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private void PlaceBottomRightButton(Button button, float anchoredX)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(anchoredX, 18f);
    }

    private Button CreateStepperButton(string text, Transform parent)
    {
        GameObject buttonObject = CreateUIObject(text + " Button", parent);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(14f, 16f);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedBoxSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(0.82f, 0.82f, 0.78f, 0.82f);
        Button button = buttonObject.AddComponent<Button>();
        TextMeshProUGUI label = CreateText(text, buttonObject.transform, 8, TextAlignmentOptions.Center);
        label.color = Color.black;
        label.fontSize = 11f;
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;
        StretchToParent(label.rectTransform, Vector2.zero, Vector2.zero);
        return button;
    }

    private TMP_InputField CreateFormulaInputField(string placeholderText, Transform parent, Vector2 size)
    {
        GameObject inputObject = CreateUIObject("Formula Input", parent);
        RectTransform inputRect = inputObject.AddComponent<RectTransform>();
        inputRect.sizeDelta = size;

        Image background = inputObject.AddComponent<Image>();
        background.sprite = GetRoundedBoxSprite();
        background.type = Image.Type.Sliced;
        background.color = new Color(0.9f, 0.9f, 0.86f, 0.82f);

        TMP_InputField inputField = inputObject.AddComponent<TMP_InputField>();

        TextMeshProUGUI text = CreateText("", inputObject.transform, 15, TextAlignmentOptions.Left);
        text.color = Color.black;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(8f, 2f);
        text.rectTransform.offsetMax = new Vector2(-8f, -2f);

        TextMeshProUGUI placeholder = CreateText(placeholderText, inputObject.transform, 15, TextAlignmentOptions.Left);
        placeholder.color = new Color(0f, 0f, 0f, 0.42f);
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.rectTransform.anchorMin = Vector2.zero;
        placeholder.rectTransform.anchorMax = Vector2.one;
        placeholder.rectTransform.offsetMin = new Vector2(8f, 2f);
        placeholder.rectTransform.offsetMax = new Vector2(-8f, -2f);

        inputField.textComponent = text;
        inputField.placeholder = placeholder;
        inputField.targetGraphic = background;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.characterLimit = 64;

        return inputField;
    }

    private Sprite GetRoundedBoxSprite()
    {
        if (roundedBoxSprite != null)
        {
            return roundedBoxSprite;
        }

        const int size = 32;
        const int radius = 7;
        roundedBoxTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        roundedBoxTexture.name = "Generated Rounded UI Box";
        roundedBoxTexture.hideFlags = HideFlags.HideAndDontSave;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = GetRoundedRectAlpha(x, y, size, radius);
                roundedBoxTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        roundedBoxTexture.Apply();
        roundedBoxSprite = Sprite.Create(roundedBoxTexture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        roundedBoxSprite.name = "Generated Rounded UI Box";
        roundedBoxSprite.hideFlags = HideFlags.HideAndDontSave;
        return roundedBoxSprite;
    }

    private float GetRoundedRectAlpha(int x, int y, int size, int radius)
    {
        int left = radius;
        int right = size - radius - 1;
        int bottom = radius;
        int top = size - radius - 1;

        int cx = Mathf.Clamp(x, left, right);
        int cy = Mathf.Clamp(y, bottom, top);
        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
        return distance <= radius ? 1f : 0f;
    }

    private TextMeshProUGUI CreateText(string text, Transform parent, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = new Color(0.92f, 0.96f, 1f, 1f);
        return tmp;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private RectTransform EnsureRectTransform(GameObject target)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = target.AddComponent<RectTransform>();
        }

        return rect;
    }

    private void StretchToParent(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            DestroyGeneratedObject(root.GetChild(i).gameObject);
        }
    }

    private void DestroyGeneratedObject(UnityEngine.Object target)
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(target);
            return;
        }
#endif

        Destroy(target);
    }

    [Serializable]
    private sealed class EquationDefinition
    {
        public string displayEquation;
        public int productStartIndex;
        public List<EquationTerm> terms = new List<EquationTerm>();

        public EquationDefinition(string equation, int firstProductIndex, params EquationTerm[] equationTerms)
        {
            displayEquation = equation;
            productStartIndex = firstProductIndex;
            terms.AddRange(equationTerms);
        }
    }

    [Serializable]
    private sealed class EquationTerm
    {
        public string label;
        public string requiredItemId;
        public int answerCoefficient;
        public List<string> nearCorrectItemIds = new List<string>();

        public EquationTerm(string termLabel, string itemId, int coefficient, params string[] nearIds)
        {
            label = termLabel;
            requiredItemId = itemId;
            answerCoefficient = coefficient;
            nearCorrectItemIds.AddRange(nearIds);
        }
    }
}

