using System.Collections;
using UnityEngine;

public class MoLogoBang : MonoBehaviour
{
    public GameObject logoMo;
    public GameObject logoLon;
    public GameObject khungPuzzle;

    public float thoiGianChoHienLogo = 2f;
    public float thoiGianChoTruocKhiMoPuzzle = 1f;

    private bool daDuocBam = false;

    void Start()
    {
        if (logoMo != null) logoMo.SetActive(false);
        if (logoLon != null) logoLon.SetActive(false);
        if (khungPuzzle != null) khungPuzzle.SetActive(false);

        StartCoroutine(HienLogoMoSau2Giay());
    }

    IEnumerator HienLogoMoSau2Giay()
    {
        yield return new WaitForSeconds(thoiGianChoHienLogo);

        if (logoMo != null)
            logoMo.SetActive(true);
    }

    public void BamLogo()
    {
        if (daDuocBam) return;
        daDuocBam = true;

        StartCoroutine(HienLogoLonRoiMoPuzzle());
    }

    IEnumerator HienLogoLonRoiMoPuzzle()
    {
        if (logoMo != null)
            logoMo.SetActive(false);

        if (logoLon != null)
            logoLon.SetActive(true);

        yield return new WaitForSeconds(thoiGianChoTruocKhiMoPuzzle);

        if (logoLon != null)
            logoLon.SetActive(false);

        if (khungPuzzle != null)
            khungPuzzle.SetActive(true);
    }
}