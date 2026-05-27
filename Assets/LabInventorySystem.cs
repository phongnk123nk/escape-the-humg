using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
[AddComponentMenu("Lab/Lab Inventory System")]
public class LabInventorySystem : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public LabSceneNavigator sceneNavigator;

    [Header("World Items")]
    public List<LabItemData> items = new List<LabItemData>();
    public string defaultVisibleInScreenName = "CabinetOpenView";
    public Color worldItemTint = new Color(1f, 1f, 1f, 0.5f);
    public string worldSortingLayerName = "";
    public int worldItemSortingOrder = 20;

    [Header("World Item Click")]
    [Tooltip("Noi rong vung click runtime cua binh hoa chat, khong doi vi tri hay kich thuoc hinh anh.")]
    public float worldItemClickPadding = 0.18f;

    [Tooltip("Neu bam lech rat nhe quanh binh, van cho nhat binh de tranh bam mai khong an.")]
    public float worldItemClickFallbackRadius = 0.16f;

    [Header("Edit Mode Preview")]
    public bool showEditModeItemPreview = true;
    public string editPreviewScreenName = "CabinetOpenView";
    public bool syncPreviewTransformToInspector = true;
    public int editPreviewSortingOrder = 35;

    [Header("Inventory")]
    public int slotCount = 8;
    public Vector2 inventoryStartPosition = new Vector2(-3.5f, -4.2f);
    public Vector2 slotSize = new Vector2(0.75f, 0.75f);
    public float slotSpacing = 0.15f;
    public Sprite slotSprite;
    public Color normalSlotColor = new Color(0.12f, 0.12f, 0.12f, 0.72f);
    public Color selectedSlotColor = new Color(1f, 0.86f, 0.25f, 0.95f);
    public float selectedScaleMultiplier = 1.15f;
    public bool hideInventoryWhenEmpty = true;
    public string inventorySortingLayerName = "";
    public int slotSortingOrder = 100;
    public int itemIconSortingOrder = 110;

    [Header("Animation")]
    public float pickupShrinkDuration = 0.15f;
    public float inventoryShowDuration = 0.18f;
    public float iconPopDuration = 0.16f;

    [Header("Pickup Preview Animation")]
    public bool usePickupPreviewAnimation = true;
    public float moveToCenterDuration = 0.35f;
    public float previewHoldDuration = 0.55f;
    public float flyToInventoryDuration = 0.45f;
    public bool useCustomPreviewWorldPosition = false;
    public Vector2 previewWorldPosition = Vector2.zero;
    public Vector2 previewScreenPosition = new Vector2(0.5f, 0.55f);
    public float previewScale = 2.5f;
    public float rewardPreviewScale = 1.1f;
    public float inventoryIconScale = 1f;
    public AnimationCurve pickupMoveCurve;
    public AnimationCurve pickupScaleCurve;
    public int pickupAnimationSortingOrder = 100;

    [Header("Runtime Selection")]
    public string selectedItemId = "";
    public LabItemData selectedItem;
    public int selectedSlotIndex = -1;
    public event Action InventoryChanged;

    private readonly Dictionary<string, LabItemData> itemById = new Dictionary<string, LabItemData>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, GameObject> worldObjectByItemId = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> pickedUpItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> pickingUpItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly List<InventorySlotRuntime> slots = new List<InventorySlotRuntime>();

    private GameObject worldItemsRoot;
    private GameObject inventoryRoot;
    private Texture2D generatedSlotTexture;
    private Sprite generatedSlotSprite;
    private bool inventoryVisible;
    private bool inventoryAnimating;
    private bool inventoryBarSuppressed;
    private GameObject editPreviewRoot;
    private readonly List<ItemPreviewRuntime> editPreviewItems = new List<ItemPreviewRuntime>();
    private bool rebuildingEditPreview;

#if UNITY_EDITOR
    private bool editPreviewQueued;
#endif

    public string SelectedItemId
    {
        get { return selectedItemId; }
    }

    public LabItemData SelectedItem
    {
        get { return selectedItem; }
    }

    public List<LabItemData> GetCollectedItems()
    {
        List<LabItemData> collectedItems = new List<LabItemData>();
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && slots[i].item != null)
            {
                collectedItems.Add(slots[i].item);
            }
        }

        return collectedItems;
    }

    public void SetInventoryBarSuppressed(bool suppressed)
    {
        inventoryBarSuppressed = suppressed;

        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(!inventoryBarSuppressed && inventoryVisible);
        }
    }

    private void Reset()
    {
        slotCount = 8;
        inventoryStartPosition = new Vector2(-3.5f, -4.2f);
        slotSize = new Vector2(0.75f, 0.75f);
        slotSpacing = 0.15f;
        normalSlotColor = new Color(0.12f, 0.12f, 0.12f, 0.72f);
        selectedSlotColor = new Color(1f, 0.86f, 0.25f, 0.95f);
        selectedScaleMultiplier = 1.15f;
        hideInventoryWhenEmpty = true;
        worldItemTint = new Color(1f, 1f, 1f, 0.5f);
        worldItemClickPadding = 0.18f;
        worldItemClickFallbackRadius = 0.16f;
        showEditModeItemPreview = true;
        editPreviewScreenName = "CabinetOpenView";
        syncPreviewTransformToInspector = true;
        editPreviewSortingOrder = 35;
        pickupShrinkDuration = 0.15f;
        inventoryShowDuration = 0.18f;
        iconPopDuration = 0.16f;
        usePickupPreviewAnimation = true;
        moveToCenterDuration = 0.35f;
        previewHoldDuration = 0.55f;
        flyToInventoryDuration = 0.45f;
        useCustomPreviewWorldPosition = false;
        previewWorldPosition = Vector2.zero;
        previewScreenPosition = new Vector2(0.5f, 0.55f);
        previewScale = 2.5f;
        rewardPreviewScale = 1.1f;
        inventoryIconScale = 1f;
        pickupAnimationSortingOrder = 100;
    }

    private void OnValidate()
    {
        slotCount = Mathf.Max(1, slotCount);
        selectedScaleMultiplier = Mathf.Max(1f, selectedScaleMultiplier);
        worldItemClickPadding = Mathf.Max(0f, worldItemClickPadding);
        worldItemClickFallbackRadius = Mathf.Max(0f, worldItemClickFallbackRadius);
        moveToCenterDuration = Mathf.Max(0.01f, moveToCenterDuration);
        previewHoldDuration = Mathf.Max(0f, previewHoldDuration);
        flyToInventoryDuration = Mathf.Max(0.01f, flyToInventoryDuration);
        previewScale = Mathf.Max(0.01f, previewScale);
        rewardPreviewScale = Mathf.Max(0.01f, rewardPreviewScale);
        inventoryIconScale = Mathf.Max(0.01f, inventoryIconScale);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            QueueEditPreviewRebuild();
        }
#endif
    }

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (sceneNavigator == null)
        {
            sceneNavigator = FindFirstObjectByType<LabSceneNavigator>();
        }

        slotCount = Mathf.Max(1, slotCount);
        selectedScaleMultiplier = Mathf.Max(1f, selectedScaleMultiplier);

        if (!Application.isPlaying)
        {
            RebuildEditPreview();
            return;
        }

        CacheItems();
        CreateWorldItems();
        CreateInventory();
        SetInventoryVisible(!hideInventoryWhenEmpty, true);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateEditPreview();
            return;
        }

        RefreshWorldItemVisibility();
        HandlePointerClick();
    }

    private void OnDestroy()
    {
        ClearEditPreview();

        if (generatedSlotSprite != null)
        {
            DestroyGeneratedObject(generatedSlotSprite);
        }

        if (generatedSlotTexture != null)
        {
            DestroyGeneratedObject(generatedSlotTexture);
        }
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ClearEditPreview();
        }
#endif
    }

    public bool HasItem(string itemId)
    {
        return FindSlotWithItem(itemId) >= 0;
    }

    public bool IsItemSelected(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId)
            && string.Equals(selectedItemId, itemId, StringComparison.OrdinalIgnoreCase);
    }

    public void ClearSelection()
    {
        selectedItemId = "";
        selectedItem = null;
        selectedSlotIndex = -1;
        RefreshSlotVisuals();
    }

    public void PickupItemById(string itemId)
    {
        LabItemData item;
        if (!itemById.TryGetValue(itemId, out item))
        {
            Debug.LogWarning("LabInventorySystem: khong tim thay itemId '" + itemId + "'.", this);
            return;
        }

        GameObject worldObject;
        worldObjectByItemId.TryGetValue(item.itemId, out worldObject);
        PickupItem(item, worldObject);
    }

    public void AwardItemFromScreenCenter(LabItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("LabInventorySystem: reward item is missing.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(itemData.itemId))
        {
            Debug.LogWarning("LabInventorySystem: reward itemId is missing.", this);
            return;
        }

        if (itemData.worldSprite == null && itemData.inventorySprite == null)
        {
            Debug.LogWarning("LabInventorySystem: reward item sprite is missing.", this);
            return;
        }

        if (HasItem(itemData.itemId) || pickingUpItemIds.Contains(itemData.itemId))
        {
            return;
        }

        if (inventoryRoot == null || slots.Count == 0)
        {
            CreateInventory();
        }

        int slotIndex = FindFirstEmptySlot();
        if (slotIndex < 0)
        {
            Debug.LogWarning("Inventory is full", this);
            return;
        }

        itemById[itemData.itemId] = itemData;
        slots[slotIndex].reserved = true;

        if (!inventoryVisible)
        {
            ShowInventoryAnimated();
        }

        StartCoroutine(AwardItemFromScreenCenterRoutine(itemData, slotIndex));
    }

    public bool HasEmptySlot()
    {
        return GetFirstEmptySlotIndex() >= 0;
    }

    public int GetFirstEmptySlotIndex()
    {
        return FindFirstEmptySlot();
    }

    public void AddItemToInventoryAtSlot(LabItemData itemData, int slotIndex)
    {
        if (itemData == null || slotIndex < 0 || slotIndex >= slots.Count)
        {
            return;
        }

        InventorySlotRuntime slot = slots[slotIndex];
        slot.item = itemData;
        slot.reserved = false;
        slot.iconRenderer.sprite = itemData.inventorySprite != null ? itemData.inventorySprite : itemData.worldSprite;
        slot.iconRenderer.enabled = true;
        slot.iconObject.transform.localScale = GetInventoryIconLocalScale(itemData);

        InventoryDragItem dragItem = slot.slotObject.GetComponent<InventoryDragItem>();
        if (dragItem == null)
        {
            dragItem = slot.slotObject.AddComponent<InventoryDragItem>();
        }

        dragItem.SetItem(itemData.itemId, itemData.itemName, slot.iconRenderer.sprite);

        pickedUpItemIds.Add(itemData.itemId);
        RefreshSlotVisuals();
        if (InventoryChanged != null)
        {
            InventoryChanged.Invoke();
        }
    }

    private Vector3 GetPickupPreviewWorldPosition()
    {
        if (useCustomPreviewWorldPosition)
        {
            return new Vector3(previewWorldPosition.x, previewWorldPosition.y, 0f);
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return new Vector3(previewWorldPosition.x, previewWorldPosition.y, 0f);
        }

        float distance = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 world = mainCamera.ViewportToWorldPoint(new Vector3(previewScreenPosition.x, previewScreenPosition.y, distance));
        world.z = 0f;
        return world;
    }

    private void CacheItems()
    {
        itemById.Clear();

        for (int i = 0; i < items.Count; i++)
        {
            LabItemData item = items[i];
            if (item == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.itemId))
            {
                item.itemId = "item_" + i;
            }

            itemById[item.itemId] = item;
        }
    }

    private void CreateWorldItems()
    {
        worldItemsRoot = new GameObject("Lab World Items");
        worldItemsRoot.transform.SetParent(transform, false);

        for (int i = 0; i < items.Count; i++)
        {
            LabItemData item = items[i];
            if (item == null || item.worldSprite == null)
            {
                continue;
            }

            GameObject itemObject = new GameObject(SafeName(item.itemName, item.itemId, i));
            itemObject.transform.SetParent(worldItemsRoot.transform, false);
            itemObject.transform.position = new Vector3(item.worldPosition.x, item.worldPosition.y, 0f);
            itemObject.transform.localScale = new Vector3(item.worldScale.x, item.worldScale.y, 1f);

            SpriteRenderer renderer = itemObject.AddComponent<SpriteRenderer>();
            renderer.sprite = item.worldSprite;
            renderer.color = worldItemTint;
            ApplySorting(renderer, worldSortingLayerName, worldItemSortingOrder + i);

            BoxCollider2D collider = itemObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            Vector2 spriteBoundsSize = item.worldSprite.bounds.size;
            collider.size = spriteBoundsSize + Vector2.one * worldItemClickPadding;

            LabWorldItemClick click = itemObject.AddComponent<LabWorldItemClick>();
            click.Initialize(this, item);

            worldObjectByItemId[item.itemId] = itemObject;
            itemObject.SetActive(ShouldShowWorldItem(item));
        }
    }

    private void RefreshWorldItemVisibility()
    {
        foreach (KeyValuePair<string, GameObject> pair in worldObjectByItemId)
        {
            if (pair.Value == null || pickingUpItemIds.Contains(pair.Key))
            {
                continue;
            }

            LabItemData item;
            if (!itemById.TryGetValue(pair.Key, out item))
            {
                continue;
            }

            pair.Value.SetActive(ShouldShowWorldItem(item));
        }
    }

    private bool ShouldShowWorldItem(LabItemData item)
    {
        if (item == null || !item.activeAtStart || pickedUpItemIds.Contains(item.itemId) || HasItem(item.itemId))
        {
            return false;
        }

        string requiredScreen = string.IsNullOrWhiteSpace(item.visibleInScreenName) ? defaultVisibleInScreenName : item.visibleInScreenName;
        if (string.IsNullOrWhiteSpace(requiredScreen) || sceneNavigator == null)
        {
            return true;
        }

        return string.Equals(sceneNavigator.CurrentScreenName, requiredScreen, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateEditPreview()
    {
        if (!showEditModeItemPreview)
        {
            ClearEditPreview();
            return;
        }

        if (editPreviewRoot == null || editPreviewItems.Count != CountPreviewableItems() || MissingPreviewObject() || PreviewParentChanged())
        {
            RebuildEditPreview();
            return;
        }

        SyncEditPreviewTransforms();
    }

    private void RebuildEditPreview()
    {
        if (Application.isPlaying || rebuildingEditPreview)
        {
            return;
        }

        if (!showEditModeItemPreview)
        {
            ClearEditPreview();
            return;
        }

        rebuildingEditPreview = true;
        ClearEditPreview();
        CacheItems();

        Transform previewParent = FindNavigatorPreviewScreenRoot();
        if (previewParent == null)
        {
            previewParent = transform;
        }

        editPreviewRoot = new GameObject("PREVIEW Inventory Items - " + editPreviewScreenName);
        ApplyEditorOnlyFlags(editPreviewRoot);
        editPreviewRoot.transform.SetParent(previewParent, false);
        editPreviewRoot.transform.localPosition = Vector3.zero;

        for (int i = 0; i < items.Count; i++)
        {
            LabItemData item = items[i];
            if (item == null || item.worldSprite == null)
            {
                continue;
            }

            GameObject previewObject = new GameObject("PREVIEW Item - " + SafeName(item.itemName, item.itemId, i));
            ApplyEditorOnlyFlags(previewObject);
            previewObject.transform.SetParent(editPreviewRoot.transform, false);
            previewObject.transform.localPosition = new Vector3(item.worldPosition.x, item.worldPosition.y, 0f);
            previewObject.transform.localScale = new Vector3(item.worldScale.x, item.worldScale.y, 1f);

            SpriteRenderer renderer = previewObject.AddComponent<SpriteRenderer>();
            renderer.sprite = item.worldSprite;
            renderer.color = item.activeAtStart ? worldItemTint : new Color(worldItemTint.r, worldItemTint.g, worldItemTint.b, 0.18f);
            ApplySorting(renderer, worldSortingLayerName, editPreviewSortingOrder + i);

            editPreviewItems.Add(new ItemPreviewRuntime
            {
                item = item,
                previewObject = previewObject,
                lastLocalPosition = previewObject.transform.localPosition,
                lastLocalScale = previewObject.transform.localScale
            });
        }

        rebuildingEditPreview = false;
    }

    private void ClearEditPreview()
    {
        editPreviewItems.Clear();

        if (editPreviewRoot != null)
        {
            DestroyGeneratedObject(editPreviewRoot);
            editPreviewRoot = null;
        }
    }

    private void SyncEditPreviewTransforms()
    {
        if (Application.isPlaying || rebuildingEditPreview || !syncPreviewTransformToInspector)
        {
            return;
        }

        bool changedAny = false;

        for (int i = 0; i < editPreviewItems.Count; i++)
        {
            ItemPreviewRuntime preview = editPreviewItems[i];
            if (preview == null || preview.previewObject == null || preview.item == null)
            {
                continue;
            }

            Transform previewTransform = preview.previewObject.transform;
            if (!previewTransform.hasChanged
                && Vector3.SqrMagnitude(preview.lastLocalPosition - previewTransform.localPosition) <= 0.000001f
                && Vector3.SqrMagnitude(preview.lastLocalScale - previewTransform.localScale) <= 0.000001f)
            {
                continue;
            }

#if UNITY_EDITOR
            Undo.RecordObject(this, "Move Inventory Item Preview");
#endif

            preview.item.worldPosition = new Vector2(previewTransform.localPosition.x, previewTransform.localPosition.y);
            preview.item.worldScale = new Vector2(previewTransform.localScale.x, previewTransform.localScale.y);
            preview.lastLocalPosition = previewTransform.localPosition;
            preview.lastLocalScale = previewTransform.localScale;
            previewTransform.hasChanged = false;
            changedAny = true;
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

    private Transform FindNavigatorPreviewScreenRoot()
    {
        if (sceneNavigator == null)
        {
            sceneNavigator = FindFirstObjectByType<LabSceneNavigator>();
        }

        if (sceneNavigator == null)
        {
            return null;
        }

        string screenName = string.IsNullOrWhiteSpace(editPreviewScreenName) ? defaultVisibleInScreenName : editPreviewScreenName;
        Transform[] children = sceneNavigator.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child != null && child.name.StartsWith("PREVIEW ", StringComparison.OrdinalIgnoreCase) && child.name.IndexOf(screenName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return child;
            }
        }

        return null;
    }

    private int CountPreviewableItems()
    {
        int count = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].worldSprite != null)
            {
                count++;
            }
        }

        return count;
    }

    private bool MissingPreviewObject()
    {
        for (int i = 0; i < editPreviewItems.Count; i++)
        {
            if (editPreviewItems[i] == null || editPreviewItems[i].previewObject == null)
            {
                return true;
            }
        }

        return false;
    }

    private bool PreviewParentChanged()
    {
        if (editPreviewRoot == null)
        {
            return true;
        }

        Transform desiredParent = FindNavigatorPreviewScreenRoot();
        if (desiredParent == null)
        {
            desiredParent = transform;
        }

        return editPreviewRoot.transform.parent != desiredParent;
    }

    private void CreateInventory()
    {
        inventoryRoot = new GameObject("Lab Inventory Bar");
        inventoryRoot.transform.SetParent(transform, false);
        inventoryRoot.transform.position = Vector3.zero;

        Sprite resolvedSlotSprite = ResolveSlotSprite();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObject = new GameObject("Inventory Slot " + i);
            slotObject.transform.SetParent(inventoryRoot.transform, false);
            slotObject.transform.position = GetSlotWorldPosition(i);
            slotObject.transform.localScale = new Vector3(slotSize.x, slotSize.y, 1f);

            SpriteRenderer slotRenderer = slotObject.AddComponent<SpriteRenderer>();
            slotRenderer.sprite = resolvedSlotSprite;
            slotRenderer.color = normalSlotColor;
            ApplySorting(slotRenderer, inventorySortingLayerName, slotSortingOrder);

            BoxCollider2D slotCollider = slotObject.AddComponent<BoxCollider2D>();
            slotCollider.isTrigger = false;
            slotCollider.size = resolvedSlotSprite != null ? resolvedSlotSprite.bounds.size : Vector2.one;

            LabInventorySlotClick click = slotObject.AddComponent<LabInventorySlotClick>();
            click.Initialize(this, i);

            GameObject iconObject = new GameObject("Item Icon");
            iconObject.transform.SetParent(slotObject.transform, false);
            iconObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            iconObject.transform.localScale = Vector3.one * 0.82f;

            SpriteRenderer iconRenderer = iconObject.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = null;
            iconRenderer.color = Color.white;
            ApplySorting(iconRenderer, inventorySortingLayerName, itemIconSortingOrder);

            slots.Add(new InventorySlotRuntime
            {
                slotObject = slotObject,
                slotRenderer = slotRenderer,
                iconObject = iconObject,
                iconRenderer = iconRenderer,
                baseScale = slotObject.transform.localScale,
                item = null
            });
        }
    }

    private void HandlePointerClick()
    {
        if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
        {
            return;
        }

        Vector2 worldPoint;
        if (!TryGetPointerDownWorldPosition(out worldPoint))
        {
            return;
        }

        Collider2D[] pointHits = Physics2D.OverlapPointAll(worldPoint);
        if (TryHandleColliderHits(pointHits, worldPoint))
        {
            return;
        }

        if (worldItemClickFallbackRadius > 0f)
        {
            Collider2D[] nearbyHits = Physics2D.OverlapCircleAll(worldPoint, worldItemClickFallbackRadius);
            TryHandleColliderHits(nearbyHits, worldPoint);
        }
    }

    private bool TryHandleColliderHits(Collider2D[] hits, Vector2 worldPoint)
    {
        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        LabWorldItemClick nearestWorldItem = null;
        float nearestWorldItemDistance = float.MaxValue;
        LabInventorySlotClick nearestSlot = null;
        float nearestSlotDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            LabWorldItemClick worldItemClick = hit.GetComponent<LabWorldItemClick>();
            if (worldItemClick != null)
            {
                float distance = Vector2.SqrMagnitude((Vector2)worldItemClick.transform.position - worldPoint);
                if (distance < nearestWorldItemDistance)
                {
                    nearestWorldItemDistance = distance;
                    nearestWorldItem = worldItemClick;
                }

                continue;
            }

            LabInventorySlotClick slotClick = hit.GetComponent<LabInventorySlotClick>();
            if (slotClick != null)
            {
                float distance = Vector2.SqrMagnitude((Vector2)slotClick.transform.position - worldPoint);
                if (distance < nearestSlotDistance)
                {
                    nearestSlotDistance = distance;
                    nearestSlot = slotClick;
                }
            }
        }

        if (nearestWorldItem != null)
        {
            nearestWorldItem.Click();
            return true;
        }

        if (nearestSlot != null)
        {
            nearestSlot.Click();
            return true;
        }

        return false;
    }

    private bool TryGetPointerDownWorldPosition(out Vector2 worldPosition)
    {
        Vector2 screenPosition;
        if (!TryGetPointerDownScreenPosition(out screenPosition))
        {
            worldPosition = Vector2.zero;
            return false;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            worldPosition = Vector2.zero;
            return false;
        }

        Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z)));
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

    private void PickupItem(LabItemData item, GameObject worldObject)
    {
        if (item == null || HasItem(item.itemId) || pickingUpItemIds.Contains(item.itemId))
        {
            return;
        }

        int slotIndex = FindFirstEmptySlot();
        if (slotIndex < 0)
        {
            Debug.LogWarning("Inventory is full", this);
            return;
        }

        slots[slotIndex].reserved = true;

        if (inventoryRoot == null)
        {
            CreateInventory();
        }

        if (!inventoryVisible)
        {
            ShowInventoryAnimated();
        }

        if (usePickupPreviewAnimation)
        {
            StartCoroutine(PickupItemRoutine(item, worldObject, slotIndex));
            return;
        }

        StartCoroutine(PickupItemWithoutPreviewRoutine(item, worldObject, slotIndex));
    }

    private IEnumerator PickupItemWithoutPreviewRoutine(LabItemData item, GameObject worldObject, int slotIndex)
    {
        if (worldObject == null)
        {
            AddItemToInventoryAtSlot(item, slotIndex);
            StartCoroutine(AnimateSlotPop(slots[slotIndex].slotObject.transform));
            yield break;
        }

        pickingUpItemIds.Add(item.itemId);
        Collider2D collider = worldObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Vector3 originalScale = worldObject.transform.localScale;
        float duration = Mathf.Max(0.01f, pickupShrinkDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            worldObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, Smooth01(t));
            yield return null;
        }

        worldObject.SetActive(false);
        AddItemToInventoryAtSlot(item, slotIndex);
        pickingUpItemIds.Remove(item.itemId);
        StartCoroutine(AnimateSlotPop(slots[slotIndex].slotObject.transform));
    }

    private IEnumerator PickupItemRoutine(LabItemData itemData, GameObject worldItemObject, int slotIndex)
    {
        if (itemData == null || HasItem(itemData.itemId) || pickingUpItemIds.Contains(itemData.itemId))
        {
            if (slotIndex >= 0 && slotIndex < slots.Count)
            {
                slots[slotIndex].reserved = false;
            }

            yield break;
        }

        if (inventoryRoot == null)
        {
            CreateInventory();
        }

        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            if (slotIndex >= 0 && slotIndex < slots.Count)
            {
                slots[slotIndex].reserved = false;
            }

            Debug.LogWarning("Inventory is full", this);
            yield break;
        }

        pickingUpItemIds.Add(itemData.itemId);

        Vector3 startPosition = worldItemObject != null
            ? worldItemObject.transform.position
            : new Vector3(itemData.worldPosition.x, itemData.worldPosition.y, 0f);
        Vector3 startScale = worldItemObject != null
            ? worldItemObject.transform.localScale
            : new Vector3(itemData.worldScale.x, itemData.worldScale.y, 1f);

        Collider2D worldCollider = worldItemObject != null ? worldItemObject.GetComponent<Collider2D>() : null;
        SpriteRenderer worldRenderer = worldItemObject != null ? worldItemObject.GetComponent<SpriteRenderer>() : null;

        if (worldCollider != null)
        {
            worldCollider.enabled = false;
        }

        if (worldRenderer != null)
        {
            worldRenderer.enabled = false;
        }

        GameObject previewObject = new GameObject("Pickup Preview - " + SafeName(itemData.itemName, itemData.itemId, slotIndex));
        previewObject.transform.SetParent(transform, false);
        previewObject.transform.position = startPosition;
        previewObject.transform.localScale = startScale;

        SpriteRenderer previewRenderer = previewObject.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = itemData.inventorySprite != null ? itemData.inventorySprite : itemData.worldSprite;
        previewRenderer.color = Color.white;
        ApplySorting(previewRenderer, inventorySortingLayerName, pickupAnimationSortingOrder);

        Vector3 centerPosition = GetPickupPreviewWorldPosition();
        Vector3 previewTargetScale = Vector3.one * previewScale;
        yield return AnimatePickupPreview(previewObject.transform, startPosition, centerPosition, startScale, previewTargetScale, moveToCenterDuration, true);

        if (previewHoldDuration > 0f)
        {
            yield return new WaitForSeconds(previewHoldDuration);
        }

        Vector3 slotPosition = GetSlotWorldPosition(slotIndex);
        Vector3 iconTargetScale = GetInventoryIconWorldScale(itemData);
        yield return AnimatePickupPreview(previewObject.transform, centerPosition, slotPosition, previewTargetScale, iconTargetScale, flyToInventoryDuration, false);

        Destroy(previewObject);

        if (worldItemObject != null)
        {
            worldItemObject.SetActive(false);
        }

        AddItemToInventoryAtSlot(itemData, slotIndex);
        pickingUpItemIds.Remove(itemData.itemId);
        yield return AnimateSlotPop(slots[slotIndex].slotObject.transform);
    }

    private IEnumerator AwardItemFromScreenCenterRoutine(LabItemData itemData, int slotIndex)
    {
        if (itemData == null || slotIndex < 0 || slotIndex >= slots.Count)
        {
            yield break;
        }

        pickingUpItemIds.Add(itemData.itemId);

        Sprite iconSprite = itemData.inventorySprite != null ? itemData.inventorySprite : itemData.worldSprite;
        Vector3 centerPosition = GetPickupPreviewWorldPosition();
        Vector3 startScale = Vector3.one * rewardPreviewScale;

        GameObject previewObject = new GameObject("Reward Preview - " + SafeName(itemData.itemName, itemData.itemId, slotIndex));
        previewObject.transform.SetParent(transform, false);
        previewObject.transform.position = centerPosition;
        previewObject.transform.localScale = startScale;

        SpriteRenderer previewRenderer = previewObject.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = iconSprite;
        previewRenderer.color = Color.white;
        ApplySorting(previewRenderer, inventorySortingLayerName, pickupAnimationSortingOrder);

        if (previewHoldDuration > 0f)
        {
            yield return new WaitForSeconds(previewHoldDuration);
        }

        Vector3 slotPosition = GetSlotWorldPosition(slotIndex);
        Vector3 iconTargetScale = GetInventoryIconWorldScale(itemData);
        yield return AnimatePickupPreview(previewObject.transform, centerPosition, slotPosition, startScale, iconTargetScale, flyToInventoryDuration, false);

        Destroy(previewObject);
        AddItemToInventoryAtSlot(itemData, slotIndex);
        pickingUpItemIds.Remove(itemData.itemId);
        yield return AnimateSlotPop(slots[slotIndex].slotObject.transform);
    }

    private IEnumerator AnimatePickupPreview(Transform previewTransform, Vector3 fromPosition, Vector3 toPosition, Vector3 fromScale, Vector3 toScale, float duration, bool wobble)
    {
        if (previewTransform == null)
        {
            yield break;
        }

        float safeDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float rawT = Mathf.Clamp01(elapsed / safeDuration);
            float moveT = EvaluateCurveOrSmoothStep(pickupMoveCurve, rawT);
            float scaleT = EvaluateCurveOrSmoothStep(pickupScaleCurve, rawT);

            previewTransform.position = Vector3.LerpUnclamped(fromPosition, toPosition, moveT);
            previewTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, scaleT);

            if (wobble)
            {
                previewTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(rawT * Mathf.PI * 2f) * 6f);
            }
            else
            {
                previewTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(4f, 0f, rawT));
            }

            yield return null;
        }

        previewTransform.position = toPosition;
        previewTransform.localScale = toScale;
        previewTransform.rotation = Quaternion.identity;
    }

    public IEnumerator AnimateSlotPop(Transform slotTransform)
    {
        if (slotTransform == null)
        {
            yield break;
        }

        Vector3 baseScale = slotTransform.localScale;
        Vector3 popScale = baseScale * 1.1f;
        float halfDuration = 0.05f;

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            slotTransform.localScale = Vector3.Lerp(baseScale, popScale, Smooth01(t));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            slotTransform.localScale = Vector3.Lerp(popScale, baseScale, Smooth01(t));
            yield return null;
        }

        slotTransform.localScale = baseScale;
    }

    private void ShowInventoryAnimated()
    {
        if (inventoryRoot == null || inventoryAnimating)
        {
            return;
        }

        StartCoroutine(ShowInventoryRoutine());
    }

    private IEnumerator ShowInventoryRoutine()
    {
        inventoryAnimating = true;
        inventoryVisible = true;
        inventoryRoot.SetActive(!inventoryBarSuppressed);

        float duration = Mathf.Max(0.01f, inventoryShowDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.75f, 1f, Smooth01(Mathf.Clamp01(elapsed / duration)));
            inventoryRoot.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        inventoryRoot.transform.localScale = Vector3.one;
        inventoryAnimating = false;
    }

    private IEnumerator PopIconRoutine(InventorySlotRuntime slot)
    {
        if (slot == null || slot.iconObject == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, iconPopDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(0f, 0.82f, Overshoot01(t));
            slot.iconObject.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        slot.iconObject.transform.localScale = Vector3.one * 0.82f;
    }

    private void SetInventoryVisible(bool visible, bool immediate)
    {
        inventoryVisible = visible;

        if (inventoryRoot == null)
        {
            return;
        }

        inventoryRoot.SetActive(visible);
        if (inventoryBarSuppressed)
        {
            inventoryRoot.SetActive(false);
        }

        inventoryRoot.transform.localScale = immediate ? Vector3.one : inventoryRoot.transform.localScale;
    }

    private void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            return;
        }

        InventorySlotRuntime slot = slots[slotIndex];
        if (slot == null || slot.item == null)
        {
            ClearSelection();
            return;
        }

        selectedSlotIndex = slotIndex;
        selectedItem = slot.item;
        selectedItemId = slot.item.itemId;
        RefreshSlotVisuals();
    }

    private void RefreshSlotVisuals()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotRuntime slot = slots[i];
            if (slot == null || slot.slotObject == null || slot.slotRenderer == null)
            {
                continue;
            }

            bool selected = i == selectedSlotIndex && slot.item != null;
            slot.slotRenderer.color = selected ? selectedSlotColor : normalSlotColor;
            slot.slotObject.transform.localScale = slot.baseScale * (selected ? selectedScaleMultiplier : 1f);

            if (slot.iconRenderer != null)
            {
                slot.iconRenderer.enabled = slot.item != null;
            }
        }
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == null && !slots[i].reserved)
            {
                return i;
            }
        }

        return -1;
    }

    private int FindSlotWithItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return -1;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != null && string.Equals(slots[i].item.itemId, itemId, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        float step = slotSize.x + slotSpacing;
        return new Vector3(inventoryStartPosition.x + step * slotIndex, inventoryStartPosition.y, 0f);
    }

    private Vector3 GetInventoryIconLocalScale(LabItemData itemData)
    {
        Sprite iconSprite = itemData != null && itemData.inventorySprite != null ? itemData.inventorySprite : itemData != null ? itemData.worldSprite : null;
        if (iconSprite == null)
        {
            return Vector3.one * inventoryIconScale;
        }

        Vector2 spriteSize = iconSprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return Vector3.one * inventoryIconScale;
        }

        float fitScaleX = 0.82f / spriteSize.x;
        float fitScaleY = 0.82f / spriteSize.y;
        float scale = Mathf.Min(fitScaleX, fitScaleY) * inventoryIconScale;
        return new Vector3(scale, scale, 1f);
    }

    private Vector3 GetInventoryIconWorldScale(LabItemData itemData)
    {
        Vector3 localIconScale = GetInventoryIconLocalScale(itemData);
        return new Vector3(localIconScale.x * slotSize.x, localIconScale.y * slotSize.y, 1f);
    }

    private float EvaluateCurveOrSmoothStep(AnimationCurve curve, float t)
    {
        if (curve != null && curve.length > 0)
        {
            return curve.Evaluate(t);
        }

        return Smooth01(t);
    }

    private Sprite ResolveSlotSprite()
    {
        if (slotSprite != null)
        {
            return slotSprite;
        }

        if (generatedSlotSprite != null)
        {
            return generatedSlotSprite;
        }

        generatedSlotTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        generatedSlotTexture.name = "Generated Inventory Slot Texture";
        generatedSlotTexture.hideFlags = HideFlags.HideAndDontSave;
        generatedSlotTexture.SetPixel(0, 0, Color.white);
        generatedSlotTexture.Apply();

        generatedSlotSprite = Sprite.Create(generatedSlotTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        generatedSlotSprite.name = "Generated Inventory Slot Sprite";
        generatedSlotSprite.hideFlags = HideFlags.HideAndDontSave;

        return generatedSlotSprite;
    }

    private void ApplySorting(SpriteRenderer renderer, string sortingLayer, int sortingOrder)
    {
        if (renderer == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(sortingLayer))
        {
            renderer.sortingLayerName = sortingLayer;
        }

        renderer.sortingOrder = sortingOrder;
    }

    private void ApplyEditorOnlyFlags(GameObject generatedObject)
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
    private void QueueEditPreviewRebuild()
    {
        if (editPreviewQueued || !isActiveAndEnabled)
        {
            return;
        }

        editPreviewQueued = true;
        EditorApplication.delayCall += DelayedEditPreviewRebuild;
    }

    private void DelayedEditPreviewRebuild()
    {
        editPreviewQueued = false;

        if (this == null || Application.isPlaying || !isActiveAndEnabled)
        {
            return;
        }

        RebuildEditPreview();
    }
#endif

    private string SafeName(string itemName, string itemId, int index)
    {
        if (!string.IsNullOrWhiteSpace(itemName))
        {
            return itemName;
        }

        if (!string.IsNullOrWhiteSpace(itemId))
        {
            return itemId;
        }

        return "Item " + index;
    }

    private float Smooth01(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private float Overshoot01(float t)
    {
        float back = 1.70158f;
        t -= 1f;
        return 1f + t * t * ((back + 1f) * t + back);
    }

    [Serializable]
    public class LabItemData
    {
        public string itemId;
        public string itemName;
        public Sprite worldSprite;
        public Sprite inventorySprite;
        public Vector2 worldPosition;
        public Vector2 worldScale = Vector2.one;
        public bool activeAtStart = true;
        public string visibleInScreenName = "";
    }

    private sealed class InventorySlotRuntime
    {
        public GameObject slotObject;
        public SpriteRenderer slotRenderer;
        public GameObject iconObject;
        public SpriteRenderer iconRenderer;
        public Vector3 baseScale;
        public LabItemData item;
        public bool reserved;
    }

    private sealed class ItemPreviewRuntime
    {
        public LabItemData item;
        public GameObject previewObject;
        public Vector3 lastLocalPosition;
        public Vector3 lastLocalScale;
    }

    private sealed class LabWorldItemClick : MonoBehaviour
    {
        private LabInventorySystem inventory;
        private LabItemData item;

        public string ItemId
        {
            get { return item != null ? item.itemId : ""; }
        }

        public void Initialize(LabInventorySystem owner, LabItemData itemData)
        {
            inventory = owner;
            item = itemData;
        }

        public void Click()
        {
            if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
            {
                return;
            }

            if (inventory != null && item != null)
            {
                inventory.PickupItemById(item.itemId);
            }
        }

        private void OnMouseDown()
        {
            Click();
        }
    }

    private sealed class LabInventorySlotClick : MonoBehaviour
    {
        private LabInventorySystem inventory;
        private int slotIndex;

        public void Initialize(LabInventorySystem owner, int index)
        {
            inventory = owner;
            slotIndex = index;
        }

        public void Click()
        {
            if (LabEquationPuzzleManager.IsAnyEquationPuzzleOpen)
            {
                return;
            }

            if (inventory != null)
            {
                inventory.SelectSlot(slotIndex);
            }
        }

        private void OnMouseDown()
        {
            Click();
        }
    }
}
