using TMPro;
using UnityEngine;

public class ThoaiBongMa : MonoBehaviour
{
    public GameObject bongMa;
    public GameObject khungThoai;
    public TextMeshProUGUI chuThoai;

    [TextArea(2, 5)]
    public string[] danhSachThoai;

    public float thoiGianXuatHien = 15f;

    private int chiSoHienTai = 0;
    private bool daBatDau = false;

    void Start()
    {
        if (bongMa != null)
            bongMa.SetActive(false);

        if (khungThoai != null)
            khungThoai.SetActive(false);
    }

    void Update()
    {
        if (!daBatDau)
        {
            thoiGianXuatHien -= Time.deltaTime;

            if (thoiGianXuatHien <= 0f)
            {
                BatDauThoai();
            }

            return;
        }

        if (khungThoai.activeSelf && Input.GetMouseButtonDown(0))
        {
            CauThoaiTiepTheo();
        }
    }

    void BatDauThoai()
    {
        daBatDau = true;

        if (bongMa != null)
            bongMa.SetActive(true);

        khungThoai.SetActive(true);
        chuThoai.text = danhSachThoai[chiSoHienTai];
    }

    void CauThoaiTiepTheo()
    {
        chiSoHienTai++;

        if (chiSoHienTai < danhSachThoai.Length)
        {
            chuThoai.text = danhSachThoai[chiSoHienTai];
        }
        else
        {
            khungThoai.SetActive(false);
        }
    }
}