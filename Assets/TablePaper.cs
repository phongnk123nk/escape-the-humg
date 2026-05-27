using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Lab/Table Paper")]
public class TablePaper : MonoBehaviour
{
    private const string PreviewOverlayName = "Table Paper Preview Overlay";

    public Sprite paperSprite;
    [TextArea]
    public string paperText;
    
    private bool isPreviewOpen = false;

    private void Start()
    {
        // Ensure there is a collider and it roughly matches the sprite size
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
        }

        if (sr != null && (box.size == Vector2.zero || box.size == Vector2.one))
        {
            box.size = sr.sprite != null ? sr.sprite.bounds.size : new Vector2(1f, 1f);
        }

        Debug.Log("TablePaper: Start initialized. Collider size: " + box.size, this);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main != null
                ? Camera.main.ScreenToWorldPoint(Input.mousePosition)
                : new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);

            Vector2 point = new Vector2(wp.x, wp.y);
            Collider2D hit = Physics2D.OverlapPoint(point);
            
            if (hit != null)
            {
                Debug.Log("TablePaper: Click detected on " + hit.gameObject.name, this);
                if (hit.gameObject == gameObject)
                {
                    Debug.Log("TablePaper: Click on TablePaper! isPreviewOpen=" + isPreviewOpen, this);
                    TogglePreview();
                }
            }
        }
    }

    private void TogglePreview()
    {
        if (isPreviewOpen)
        {
            ClosePreview();
        }
        else
        {
            ShowPreview();
        }
    }

    private void ShowPreview()
    {
        if (isPreviewOpen)
        {
            return; // Already open, ignore
        }

        if (paperSprite == null)
        {
            Debug.LogWarning("TablePaper: paperSprite is not assigned.", this);
            return;
        }

        // Remove any existing overlay first (without resetting flag)
        GameObject existing = GameObject.Find(PreviewOverlayName);
        if (existing != null)
        {
            Destroy(existing);
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            // Create a runtime Canvas so the preview can appear in Play mode
            GameObject canvasGo = new GameObject("TablePaper_RuntimeCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Make sure this canvas renders on top
            canvas.overrideSorting = true;
            canvas.sortingOrder = 10000;

            // Ensure an EventSystem exists
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("TablePaper: Created runtime Canvas with overrideSorting.", this);
        }
        else
        {
            // If an existing canvas was found, ensure it will render above other UI
            canvas.overrideSorting = true;
            canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 1000);
            Debug.Log("TablePaper: Using existing Canvas, set overrideSorting and sortingOrder.", this);
        }

        GameObject overlay = new GameObject(PreviewOverlayName);
        overlay.transform.SetParent(canvas.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.5f);
        overlayImage.raycastTarget = true;

        // close when clicking outside
        var closer = overlay.AddComponent<TablePaperOverlayClose>();
        closer.Initialize(ClosePreview);

        GameObject iconObject = new GameObject("Paper Preview Icon");
        iconObject.transform.SetParent(overlay.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(640f, 400f);

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = paperSprite;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = true;
        iconImage.color = Color.white;

        if (!string.IsNullOrWhiteSpace(paperText))
        {
            GameObject labelObject = new GameObject("Paper Text");
            labelObject.transform.SetParent(overlay.transform, false);
            RectTransform labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, -220f);
            labelRect.sizeDelta = new Vector2(760f, 120f);

            TMPro.TextMeshProUGUI label = labelObject.AddComponent<TMPro.TextMeshProUGUI>();
            label.text = paperText;
            label.fontSize = 26f;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
        }

        isPreviewOpen = true;
    }

    private void ClosePreview()
    {
        GameObject existing = GameObject.Find(PreviewOverlayName);
        if (existing != null)
        {
            Destroy(existing);
        }
        isPreviewOpen = false;
    }

    private sealed class TablePaperOverlayClose : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        private System.Action closeAction;

        public void Initialize(System.Action close)
        {
            closeAction = close;
        }

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            // if click is on the overlay background, close. If clicking the image (which has its own raycast), it won't reach here.
            closeAction?.Invoke();
        }
    }
}
