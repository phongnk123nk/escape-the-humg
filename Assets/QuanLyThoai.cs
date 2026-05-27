using System.Collections;
using TMPro;
using UnityEngine;

// Lớp quản lý thoại và hiệu ứng liên quan (hộp thoại, bóng ma, gõ ký tự...)
// Lưu ý: chỉ thêm bình luận giải thích; không thay đổi logic hay xóa code.
public class QuanLyThoai : MonoBehaviour
{
    [Header("Tham chieu")]
    // Tham chiếu tới các GameObject và component UI cần dùng trong hệ thống thoại
    public GameObject khungThoai;
    public TextMeshProUGUI chuThoai;
    public GameObject bongMa;
    public GameObject nutTatMay;
    public GameObject muiTenBanGiaoVien;
    public GameObject nutBoQua;
    // Ghi chú: các GameObject trên được gán trong Inspector của Unity.
    // - `khungThoai`: container UI cho hộp thoại (panel)
    // - `chuThoai`: component TextMeshPro hiển thị văn bản thoại
    // - `bongMa`: sprite/visual đại diện cho nhân vật bóng ma
    // - `nutTatMay`, `muiTenBanGiaoVien`, `nutBoQua`: các nút/indicator điều khiển luồng trò chơi

    [Header("Thoai")]
    // Mảng chứa các câu thoại; dùng TextArea để dễ chỉnh trong Inspector
    [TextArea(2, 5)]
    public string[] thoaiNguoiChoi;

    [TextArea(2, 5)]
    public string[] thoaiBongMa;

    [Header("Thoi gian")]
    // Các hằng thời gian điều khiển delay, hiệu ứng gõ và fade
    public float thoiGianChoBanDau = 5f;
    public float thoiGianChoMaXuatHien = 2f;
    public float thoiGianMoiKyTu = 0.05f;
    public float thoiGianNghiSauMoiCau = 1.2f;
    public float thoiGianFadeInMa = 1.5f;
    public float thoiGianFadeOutMa = 1.2f;
    // Ghi chú thời gian: thay đổi các giá trị này sẽ làm thay đổi tốc độ và độ trễ của hiệu ứng thoại
    // - `thoiGianChoBanDau`: đợi trước khi bắt đầu luồng thoại
    // - `thoiGianChoMaXuatHien`: khoảng delay giữa kết thúc thoại người chơi và xuất hiện bóng ma
    // - `thoiGianMoiKyTu`: delay giữa từng ký tự (typewriter)
    // - `thoiGianNghiSauMoiCau`: pause giữa các câu của bóng ma
    // - `thoiGianFadeInMa`/`FadeOut`: thời gian fade sprite bongMa

    [Header("Mau sac")]
    // Màu chữ dùng để phân biệt thoại người chơi và thoại bóng ma
    public Color mauThoaiNguoiChoi = Color.white;
    public Color mauThoaiBongMa = new Color(0.8f, 0.1f, 0.1f, 1f);

    [Header("Rung chu ma")]
    // Cấu hình hiệu ứng rung chữ khi bóng ma thoại
    public bool rungChuMa = true;
    public float cuongDoRung = 2f;
    public float tocDoRung = 35f;
    // Hiệu ứng rung chữ: khi bóng ma thoại, chữ sẽ di chuyển nhẹ để tạo cảm giác bất ổn
    // - `cuongDoRung`: biên độ (pixel) dịch chuyển
    // - `tocDoRung`: tần số rung

    // Trạng thái nội bộ theo dõi tiến trình thoại và các flag
    private int chiSoHienTai = 0;
    private string[] danhSachThoaiHienTai;
    private bool dangChoClick = false;
    private bool dangThoaiNguoiChoi = false;
    private bool dangThoaiBongMa = false;
    private bool dangDanhChu = false;
    private bool daBoQua = false;

    // Trạng thái nội bộ:
    // - `chiSoHienTai`: index câu đang hiển thị trong mảng thoại
    // - `danhSachThoaiHienTai`: tham chiếu tới mảng thoại hiện dùng (người chơi hoặc bóng ma)
    // - các boolean dùng để điều phối logic input, animation, và ngắt coroutine

    // Lưu vị trí gốc của text và component sprite của bongMa
    private Vector3 viTriGocChu;
    private SpriteRenderer spriteRendererMa;
    private Color mauGocMa;

    // `viTriGocChu` lưu vị trí ban đầu của Text để có thể reset sau khi áp dụng hiệu ứng rung
    // `spriteRendererMa` và `mauGocMa` dùng để điều khiển hiệu ứng fade của bongMa

    void Start()
    {
        // Khởi tạo trạng thái ban đầu: tắt UI không cần thiết, lấy component
        if (khungThoai != null)
            khungThoai.SetActive(false);

        if (chuThoai != null)
            viTriGocChu = chuThoai.rectTransform.localPosition;

        if (bongMa != null)
        {
            spriteRendererMa = bongMa.GetComponent<SpriteRenderer>();

            if (spriteRendererMa != null)
                mauGocMa = spriteRendererMa.color;

            bongMa.SetActive(false);
        }

        if (nutTatMay != null)
            nutTatMay.SetActive(false);

        if (muiTenBanGiaoVien != null)
            muiTenBanGiaoVien.SetActive(false);

        if (nutBoQua != null)
            nutBoQua.SetActive(true);

        // Bắt đầu coroutine chính điều khiển luồng thoại
        StartCoroutine(ChayTrinhTuThoai());
    }

    void Update()
    {
        // Kiểm tra input và áp dụng hiệu ứng mỗi frame
        if (daBoQua) return;

        if (dangThoaiNguoiChoi && dangChoClick && !dangDanhChu && Input.GetMouseButtonDown(0))
        {
            CauThoaiNguoiChoiTiepTheo();
        }

        if (dangThoaiBongMa && rungChuMa && chuThoai != null)
        {
            float x = Mathf.Sin(Time.time * tocDoRung) * cuongDoRung;
            float y = Mathf.Cos(Time.time * tocDoRung * 0.8f) * cuongDoRung;
            chuThoai.rectTransform.localPosition = viTriGocChu + new Vector3(x, y, 0f);
        }
        else if (chuThoai != null)
        {
            chuThoai.rectTransform.localPosition = viTriGocChu;
        }
    }

    // Lưu ý về `Update`:
    // - Nếu `daBoQua` true, tất cả hành vi thoại bị vô hiệu hóa
    // - Khi đang ở trạng thái `dangThoaiBongMa` và `rungChuMa` bật, vị trí của text được cập nhật theo sin/cos để rung
    // - Khi không rung, position được reset về `viTriGocChu` để tránh dịch vị sai lệch

    IEnumerator ChayTrinhTuThoai()
    {
        // Coroutine chính: đợi thời gian ban đầu rồi bắt đầu thoại người chơi
        yield return new WaitForSeconds(thoiGianChoBanDau);

        if (daBoQua) yield break;

        BatDauThoaiNguoiChoi();
    }

    // Ghi chú: `ChayTrinhTuThoai` chỉ chịu trách nhiệm khởi đầu luồng; các bước tiếp theo do các phương thức khác quản lý.

    void BatDauThoaiNguoiChoi()
    {
        // Thiết lập để bắt đầu hiển thị thoại của người chơi
        dangThoaiNguoiChoi = true;
        dangThoaiBongMa = false;
        dangChoClick = true;
        dangDanhChu = false;

        chiSoHienTai = 0;
        danhSachThoaiHienTai = thoaiNguoiChoi;

        khungThoai.SetActive(true);
        chuThoai.color = mauThoaiNguoiChoi;
        chuThoai.text = danhSachThoaiHienTai[chiSoHienTai];
    }

    // Ghi chú: khi bắt đầu thoại người chơi, text hiển thị nguyên câu (không gõ từng ký tự).
    // `dangChoClick` cho phép người chơi click để chuyển câu; `dangDanhChu` false để ngăn việc skip bằng click trong lúc gõ.

    void CauThoaiNguoiChoiTiepTheo()
    {
        chiSoHienTai++;

        if (chiSoHienTai < danhSachThoaiHienTai.Length)
        {
            chuThoai.text = danhSachThoaiHienTai[chiSoHienTai];
        }
        else
        {
            dangChoClick = false;
            StartCoroutine(XuLySauThoaiNguoiChoi());
        }
    }

    // Ghi chú: `CauThoaiNguoiChoiTiepTheo` được gọi khi người chơi click; khi hết mảng thoại, chuyển sang xử lý tiếp theo.

    IEnumerator XuLySauThoaiNguoiChoi()
    {
        // Sau khi người chơi hoàn thành thoại, chờ rồi chuyển sang bóng ma
        khungThoai.SetActive(false);

        yield return new WaitForSeconds(thoiGianChoMaXuatHien);

        if (daBoQua) yield break;

        BatDauThoaiBongMa();
    }

    // Ghi chú: tắt `khungThoai` trước khi bóng ma xuất hiện để tránh overlapping UI.

    void BatDauThoaiBongMa()
    {
        // Bật trạng thái thoại của bóng ma và chạy coroutine xử lý kèm hiệu ứng
        dangThoaiNguoiChoi = false;
        dangThoaiBongMa = true;
        dangChoClick = false;

        StartCoroutine(ChayCanhMaXuatHienVaThoai());
    }

    // Ghi chú: bóng ma dùng hiệu ứng gõ ký tự và fade sprite; trong khi đó input click thường bị bỏ qua.

    IEnumerator ChayCanhMaXuatHienVaThoai()
    {
        // Hiển thị bongMa (fade-in), rồi hiển thị từng câu với hiệu ứng gõ
        if (bongMa != null)
        {
            bongMa.SetActive(true);
            yield return StartCoroutine(FadeInBongMa());
        }

        if (daBoQua) yield break;

        khungThoai.SetActive(true);
        chuThoai.color = mauThoaiBongMa;

        for (int i = 0; i < thoaiBongMa.Length; i++)
        {
            if (daBoQua) yield break;

            yield return StartCoroutine(HienTungKyTu(thoaiBongMa[i]));
            yield return new WaitForSeconds(thoiGianNghiSauMoiCau);
        }

        khungThoai.SetActive(false);
        chuThoai.rectTransform.localPosition = viTriGocChu;

        if (bongMa != null)
        {
            yield return StartCoroutine(FadeOutBongMa());
            bongMa.SetActive(false);
        }

        dangThoaiBongMa = false;

        HienLuaChon();
    }

    // Ghi chú chi tiết cho flow trên:
    // 1) Bật `bongMa` và fade-in để tạo cảm giác xuất hiện
    // 2) Bật `khungThoai` và đổi màu chữ sang `mauThoaiBongMa`
    // 3) Duyệt mảng `thoaiBongMa`, hiển thị từng câu bằng `HienTungKyTu` (typewriter)
    // 4) Sau khi kết thúc, ẩn UI và fade-out bongMa trước khi hiển thị lựa chọn

    IEnumerator HienTungKyTu(string cauThoai)
    {
        // Typewriter effect: hiển thị từng ký tự của câu thoại
        dangDanhChu = true;
        chuThoai.text = "";

        for (int i = 0; i < cauThoai.Length; i++)
        {
            if (daBoQua) yield break;

            chuThoai.text += cauThoai[i];
            yield return new WaitForSeconds(thoiGianMoiKyTu);
        }

        dangDanhChu = false;
    }

    // Ghi chú về `HienTungKyTu`:
    // - Thiết kế để có thể bị ngắt bởi `daBoQua` (người chơi bỏ qua)
    // - Sau khi hoàn tất, `dangDanhChu` được reset để cho phép các hành vi khác (nếu cần)

    IEnumerator FadeInBongMa()
    {
        // Fade-in sprite của bongMa từ alpha 0 -> alpha gốc
        if (spriteRendererMa == null)
            yield break;

        Color mauHienTai = spriteRendererMa.color;
        float alphaCuoi = mauGocMa.a > 0 ? mauGocMa.a : 1f;

        spriteRendererMa.color = new Color(mauHienTai.r, mauHienTai.g, mauHienTai.b, 0f);

        float t = 0f;

        while (t < thoiGianFadeInMa)
        {
            if (daBoQua) yield break;

            t += Time.deltaTime;
            float alphaMoi = Mathf.Lerp(0f, alphaCuoi, t / thoiGianFadeInMa);
            spriteRendererMa.color = new Color(mauHienTai.r, mauHienTai.g, mauHienTai.b, alphaMoi);
            yield return null;
        }

        spriteRendererMa.color = new Color(mauHienTai.r, mauHienTai.g, mauHienTai.b, alphaCuoi);
    }

    // Ghi chú: fade được thực hiện theo t/totalTime để mượt; kiểm tra `daBoQua` trong vòng lặp để cho phép
    // người chơi hủy bỏ giữa chừng.

    IEnumerator FadeOutBongMa()
    {
        // Fade-out sprite của bongMa từ alpha hiện tại -> 0
        if (spriteRendererMa == null)
            yield break;

        Color mauHienTai = spriteRendererMa.color;
        float alphaDau = mauHienTai.a;

        float t = 0f;

        while (t < thoiGianFadeOutMa)
        {
            t += Time.deltaTime;
            float alphaMoi = Mathf.Lerp(alphaDau, 0f, t / thoiGianFadeOutMa);
            spriteRendererMa.color = new Color(mauHienTai.r, mauHienTai.g, mauHienTai.b, alphaMoi);
            yield return null;
        }

        spriteRendererMa.color = new Color(mauHienTai.r, mauHienTai.g, mauHienTai.b, 0f);
    }

    // Ghi chú: fade-out không kiểm tra `daBoQua` vì khi tới đây luồng đã hoàn tất; nếu cần, có thể thêm kiểm tra tương tự như fade-in.

    public void BamBoQua()
    {
        // Xử lý khi người chơi bấm "Bỏ qua": dừng tất cả coroutine và reset UI
        if (daBoQua) return;

        daBoQua = true;
        StopAllCoroutines();

        dangThoaiNguoiChoi = false;
        dangThoaiBongMa = false;
        dangChoClick = false;
        dangDanhChu = false;

        if (khungThoai != null)
            khungThoai.SetActive(false);

        if (chuThoai != null)
            chuThoai.rectTransform.localPosition = viTriGocChu;

        if (bongMa != null)
            bongMa.SetActive(false);

        HienLuaChon();
    }

    // Ghi chú: `BamBoQua` là điểm an toàn để ngắt mọi coroutine và đưa game về trạng thái lựa chọn.

    void HienLuaChon()
    {
        // Hiển thị các nút lựa chọn khi luồng thoại hoàn tất
        if (nutTatMay != null)
            nutTatMay.SetActive(true);

        if (muiTenBanGiaoVien != null)
            muiTenBanGiaoVien.SetActive(true);

        if (nutBoQua != null)
            nutBoQua.SetActive(false);
    }
}