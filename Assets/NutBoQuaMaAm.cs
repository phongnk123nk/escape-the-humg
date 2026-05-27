using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class NutBoQuaMaAm : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI chu;

    [Header("Mau sac")]
    public Color mauBinhThuong = new Color(0.65f, 0.15f, 0.15f, 0.75f);
    public Color mauHover = new Color(1f, 0.25f, 0.25f, 1f);

    [Header("Nhay mo")]
    public float tocDoNhay = 2f;
    public float alphaMin = 0.35f;
    public float alphaMax = 0.9f;

    [Header("Rung nhe")]
    public bool rung = true;
    public float cuongDoRung = 1.5f;
    public float tocDoRung = 20f;

    private RectTransform rectChu;
    private Vector2 viTriGoc;
    private bool dangHover = false;

    void Start()
    {
        if (chu == null)
            chu = GetComponentInChildren<TextMeshProUGUI>();

        if (chu != null)
        {
            rectChu = chu.GetComponent<RectTransform>();
            viTriGoc = rectChu.anchoredPosition;
        }
    }

    void Update()
    {
        if (chu == null) return;

        // Nhay mo alpha
        float t = (Mathf.Sin(Time.time * tocDoNhay) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(alphaMin, alphaMax, t);

        Color mauDangDung = dangHover ? mauHover : mauBinhThuong;
        mauDangDung.a = dangHover ? 1f : alpha;
        chu.color = mauDangDung;

        // Rung nhe
        if (rung && rectChu != null)
        {
            float x = Mathf.Sin(Time.time * tocDoRung) * cuongDoRung;
            float y = Mathf.Cos(Time.time * (tocDoRung * 0.8f)) * (cuongDoRung * 0.5f);
            rectChu.anchoredPosition = viTriGoc + new Vector2(x, y);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dangHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dangHover = false;
    }
}