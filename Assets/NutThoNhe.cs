using UnityEngine;

public class NutThoNhe : MonoBehaviour
{
    public float tocDoTho = 2f;
    public float alphaMin = 0.45f;
    public float alphaMax = 1f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null) return;

        float t = (Mathf.Sin(Time.time * tocDoTho) + 1f) / 2f;
        float alpha = Mathf.Lerp(alphaMin, alphaMax, t);

        Color mau = sr.color;
        mau.a = alpha;
        sr.color = mau;
    }
}