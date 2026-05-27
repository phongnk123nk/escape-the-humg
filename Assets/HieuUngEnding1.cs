using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HieuUngEnding1 : MonoBehaviour
{
    public TextMeshProUGUI chuKet;

    [TextArea(2, 5)]
    public string[] cacDongChu;

    [Header("Thoi gian")]
    public float thoiGianChoTruocDongDau = 0f;
    public float thoiGianMoiDongTonTai = 3f;
    public float thoiGianAnDong = 0.6f;
    public float thoiGianChoSauDongCuoi = 3f;

    [Header("Scene quay ve")]
    public string tenSceneMenu = "main menu";

    void Start()
    {
        if (chuKet != null)
        {
            chuKet.text = "";
            Color c = chuKet.color;
            c.a = 0f;
            chuKet.color = c;
        }

        StartCoroutine(ChayEnding());
    }

    IEnumerator ChayEnding()
    {
        yield return new WaitForSeconds(thoiGianChoTruocDongDau);

        for (int i = 0; i < cacDongChu.Length; i++)
        {
            yield return StartCoroutine(HienDong(cacDongChu[i]));

            if (i == cacDongChu.Length - 1)
            {
                yield return new WaitForSeconds(thoiGianChoSauDongCuoi);
            }
            else
            {
                yield return new WaitForSeconds(thoiGianMoiDongTonTai);
                yield return StartCoroutine(AnDong());
            }
        }

        SceneManager.LoadScene(tenSceneMenu);
    }

    IEnumerator HienDong(string noiDung)
    {
        chuKet.text = noiDung;

        float t = 0f;
        while (t < thoiGianAnDong)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / thoiGianAnDong);
            DatAlphaChu(a);
            yield return null;
        }

        DatAlphaChu(1f);
    }

    IEnumerator AnDong()
    {
        float t = 0f;
        while (t < thoiGianAnDong)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / thoiGianAnDong);
            DatAlphaChu(a);
            yield return null;
        }

        DatAlphaChu(0f);
        chuKet.text = "";
    }

    void DatAlphaChu(float a)
    {
        if (chuKet == null) return;

        Color c = chuKet.color;
        c.a = a;
        chuKet.color = c;
    }
}