using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("Lab/Inventory Drag Item")]
public class InventoryDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private const string PreviewOverlayName = "Inventory Item Preview Overlay";
    public string itemId;
    public string itemName;
    public Sprite itemIcon;

    private Canvas dragCanvas;
    private GameObject dragVisual;
    private RectTransform dragVisualRect;
    private bool manualDragging;
    private bool manualDragStarted;
    private Vector2 manualDragStartPosition;
    private Coroutine highlightRoutine;
    private const float ManualDragThreshold = 8f;

    public void SetItem(string id, string displayName, Sprite icon)
    {
        itemId = NormalizeItemId(id);
        itemName = displayName;
        itemIcon = icon;
    }

    public void HighlightHint(float duration)
    {
        if (highlightRoutine != null)
        {
            StopCoroutine(highlightRoutine);
        }

        highlightRoutine = StartCoroutine(HighlightHintRoutine(duration));
    }

    private string NormalizeItemId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return "";
        }

        string compactId = id.Replace("_", "").Replace("-", "").ToLowerInvariant();
        if (compactId == "c3h8o3")
        {
            return "glycerol";
        }

        if (compactId == "c12h22o11")
        {
            return "sucrose";
        }

        if (compactId == "c10h14n2")
        {
            return "nicotine";
        }

        if (compactId == "h2o2")
        {
            return "hydrogen_peroxide";
        }

        return id;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ClosePreview();
        BeginDragVisual(eventData.position);
        eventData.pointerDrag = gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveDragVisual(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        TryDropAtScreenPosition(eventData.position);
        EndDragVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ShowPreview();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePreview();
        }
    }

    private void OnMouseDown()
    {
        manualDragging = true;
        manualDragStarted = false;
        manualDragStartPosition = Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        if (!manualDragging)
        {
            return;
        }

        if (!manualDragStarted && Vector2.Distance(manualDragStartPosition, Input.mousePosition) >= ManualDragThreshold)
        {
            ClosePreview();
            BeginDragVisual(Input.mousePosition);
            manualDragStarted = true;
        }

        if (manualDragStarted)
        {
            MoveDragVisual(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (!manualDragging)
        {
            return;
        }

        if (manualDragStarted)
        {
            TryDropAtScreenPosition(Input.mousePosition);
            EndDragVisual();
        }
        else
        {
            ShowPreview();
        }

        manualDragging = false;
        manualDragStarted = false;
    }

    private void BeginDragVisual(Vector2 screenPosition)
    {
        if (string.IsNullOrWhiteSpace(itemId) || itemIcon == null)
        {
            return;
        }

        dragCanvas = FindFirstObjectByType<Canvas>();
        if (dragCanvas == null)
        {
            Debug.LogWarning("InventoryDragItem: no Canvas found for drag visual.", this);
            return;
        }

        dragVisual = new GameObject("Dragging Item - " + itemId);
        dragVisual.transform.SetParent(dragCanvas.transform, false);
        dragVisual.transform.SetAsLastSibling();

        dragVisualRect = dragVisual.AddComponent<RectTransform>();
        dragVisualRect.sizeDelta = new Vector2(72f, 72f);

        Image image = dragVisual.AddComponent<Image>();
        image.sprite = itemIcon;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = true;

        MoveDragVisual(screenPosition);
    }

    private void MoveDragVisual(Vector2 screenPosition)
    {
        if (dragVisualRect == null || dragCanvas == null)
        {
            return;
        }

        RectTransform canvasRect = dragCanvas.transform as RectTransform;
        Camera canvasCamera = dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : dragCanvas.worldCamera;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvasCamera, out localPoint))
        {
            dragVisualRect.anchoredPosition = localPoint;
        }
    }

    private void TryDropAtScreenPosition(Vector2 screenPosition)
    {
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition,
                pointerDrag = gameObject
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            for (int i = 0; i < results.Count; i++)
            {
                EquationDropSlot dropSlot = results[i].gameObject.GetComponentInParent<EquationDropSlot>();
                if (dropSlot != null)
                {
                    dropSlot.ReceiveItem(itemId, itemName, itemIcon);
                    return;
                }
            }
        }

        TryDropOnWorldTarget(screenPosition);
    }

    private void TryDropOnWorldTarget(Vector2 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            camera = FindFirstObjectByType<Camera>();
        }

        if (camera == null)
        {
            return;
        }

        Vector3 world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(camera.transform.position.z)));
        Collider2D[] hits = Physics2D.OverlapPointAll(new Vector2(world.x, world.y));
        for (int i = 0; i < hits.Length; i++)
        {
            LabKeyDoorExit doorExit = hits[i] != null ? hits[i].GetComponentInParent<LabKeyDoorExit>() : null;
            if (doorExit != null && doorExit.TryUseDraggedItem(this))
            {
                return;
            }
        }

        LabKeyDoorExit[] doorExits = FindObjectsByType<LabKeyDoorExit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < doorExits.Length; i++)
        {
            if (doorExits[i] != null && doorExits[i].ContainsWorldPoint(new Vector2(world.x, world.y)) && doorExits[i].TryUseDraggedItem(this))
            {
                return;
            }
        }
    }

    private void EndDragVisual()
    {
        if (dragVisual != null)
        {
            Destroy(dragVisual);
        }

        dragVisual = null;
        dragVisualRect = null;
    }

    private IEnumerator HighlightHintRoutine(float duration)
    {
        Image image = GetComponent<Image>();
        if (image == null)
        {
            yield break;
        }

        Color originalColor = image.color;
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.1f, duration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float wave = (Mathf.Sin(elapsed * 10f) + 1f) * 0.5f;
            image.color = Color.Lerp(originalColor, new Color(1f, 0.92f, 0.2f, 1f), wave);
            transform.localScale = originalScale * Mathf.Lerp(1f, 1.25f, wave);
            yield return null;
        }

        image.color = originalColor;
        transform.localScale = originalScale;
        highlightRoutine = null;
    }

    private void ShowPreview()
    {
        if (itemIcon == null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        ClosePreview();

        GameObject overlay = new GameObject(PreviewOverlayName);
        overlay.transform.SetParent(canvas.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.001f);
        overlayImage.raycastTarget = true;

        InventoryItemPreviewOverlay closeOnClick = overlay.AddComponent<InventoryItemPreviewOverlay>();
        closeOnClick.Initialize(ClosePreview);

        GameObject iconObject = new GameObject("Preview Icon - " + itemId);
        iconObject.transform.SetParent(overlay.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(180f, 180f);

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = itemIcon;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = true;
        iconImage.color = Color.white;

        if (!string.IsNullOrWhiteSpace(itemName))
        {
            GameObject labelObject = new GameObject("Preview Label");
            labelObject.transform.SetParent(overlay.transform, false);
            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, -112f);
            labelRect.sizeDelta = new Vector2(260f, 36f);

            TMPro.TextMeshProUGUI label = labelObject.AddComponent<TMPro.TextMeshProUGUI>();
            label.text = itemName;
            label.fontSize = 22f;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
        }
    }

    private void ClosePreview()
    {
        GameObject existing = GameObject.Find(PreviewOverlayName);
        if (existing != null)
        {
            Destroy(existing);
        }
    }

    private sealed class InventoryItemPreviewOverlay : MonoBehaviour, IPointerClickHandler
    {
        private System.Action closeAction;

        public void Initialize(System.Action close)
        {
            closeAction = close;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            closeAction?.Invoke();
        }
    }
}
