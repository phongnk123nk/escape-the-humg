using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Icon mini-game tren man hinh may tinh Frame49.
/// Sau nay co the gan sceneNameToLoad hoac miniGamePanelToOpen trong Inspector.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
[AddComponentMenu("Computer Room/Mini Game Icon")]
public class ComputerRoomMiniGameIcon : MonoBehaviour
{
    public string iconName = "MiniGame";
    public string sceneNameToLoad = "";
    public GameObject miniGamePanelToOpen;

    [Header("Completion Result")]
    public string resultNumber = "1";
    public Color resultNumberColor = Color.white;
    public float resultNumberSize = 1.2f;
    public int resultNumberSortingOrder = 2100;
    public bool disableClickAfterResult = true;
    public bool completed;

    private SpriteRenderer spriteRenderer;
    private Collider2D iconCollider;

    private void Reset()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        collider2D.isTrigger = true;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        iconCollider = GetComponent<Collider2D>();
    }

    private void OnMouseDown()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (miniGamePanelToOpen != null)
        {
            miniGamePanelToOpen.SetActive(true);
            return;
        }

        if (!string.IsNullOrWhiteSpace(sceneNameToLoad))
        {
            SceneManager.LoadScene(sceneNameToLoad);
            return;
        }

        Debug.Log("Mini-game icon clicked: " + iconName, this);
    }

    public void ShowResultNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            number = resultNumber;
        }

        resultNumber = number;
        completed = true;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        Transform oldText = transform.Find("MiniGameResultNumber");
        GameObject textObject;
        if (oldText != null)
        {
            textObject = oldText.gameObject;
        }
        else
        {
            textObject = new GameObject("MiniGameResultNumber");
            textObject.transform.SetParent(transform, false);
        }

        textObject.transform.localPosition = Vector3.zero;
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        TextMesh textMesh = textObject.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = textObject.AddComponent<TextMesh>();
        }

        textMesh.text = resultNumber;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = resultNumberSize;
        textMesh.fontSize = 96;
        textMesh.color = resultNumberColor;

        MeshRenderer meshRenderer = textObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = resultNumberSortingOrder;
        }

        if (disableClickAfterResult)
        {
            if (iconCollider == null)
            {
                iconCollider = GetComponent<Collider2D>();
            }

            if (iconCollider != null)
            {
                iconCollider.enabled = false;
            }
        }
    }
}
