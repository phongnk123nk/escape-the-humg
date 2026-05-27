using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuanLyXepHinh : MonoBehaviour
{
    [Header("Ảnh logo dùng để cắt")]
    public Sprite logoMaMiSprite;
    
    [Header("Ảnh logo hiển thị khi giải xong")]
    public Sprite logoBinhThuongSprite;

    [Header("Ảnh logo ma để biến hình")]
    [InspectorName("Sprite Ma Biến Hình")]
    public Sprite logoMaBienHinhSprite; // sprite ma khác dùng cho morph sau puzzle

    [Header("Object trong scene")]
    [Tooltip("GameObject logo nhỏ sẽ hiện sau vài giây, bấm vào để bắt đầu")] 
    public GameObject logoMo;
    [Tooltip("GameObject logo lớn hiện ngay sau khi bấm logo nhỏ (hiện trong vài giây)")]
    public GameObject logoLon;
    [Tooltip("Transform cha chứa các mảnh puzzle (tiles sẽ được tạo vào đây)")]
    public Transform khungPuzzle;

    [Header("Cài đặt puzzle")]
    [Tooltip("Số ô mỗi chiều (3 cho puzzle 3x3)")]
    public int gridSize = 3;
    [Tooltip("Kích thước mỗi ô (đơn vị world units)")]
    public float tileSize = 1.4f;
    [Tooltip("Thời gian (giây) LogoLon hiển thị trước khi bắt đầu puzzle")]
    public float thoiGianLogoLonHien = 1f;
    [Tooltip("Thời gian (giây) chờ trước khi hiện LogoMo khi vào scene")]
    public float thoiGianLogoMoXuatHien = 2f;
    [Header("Animation")]
    [Tooltip("Thời gian (giây) animation xuất hiện (appear)")]
    public float appearDuration = 0.25f;
    [Tooltip("Thời gian (giây) animation ẩn (disappear)")]
    public float disappearDuration = 0.2f;

    [Header("Khoảng cách giữa các ô")]
    [Tooltip("Khoảng cách giữa các ô, tính bằng pixels (sẽ chuyển sang world units)")]
    public float gapPixels = 5f;

    [Header("Viền phát sáng")]
    public bool batVienPhatSang = true;
    public Color mauVienPhatSang = Color.red;
    public float glowScaleThem = 0.08f;
    public float glowAlpha = 0.55f;

    [Header("Vị trí puzzle")]
    [Tooltip("Tọa độ world center của khung puzzle (logo kết quả sẽ xuất hiện tại đây)")]
    public Vector3 puzzleCenter = Vector3.zero;

    [Header("Dev Skip")]
    [Tooltip("Bật tính năng phím tắt để xếp puzzle tự động (dev)")]
    public bool batDevSkip = true;
    [Tooltip("Phím để skip (mặc định K)")]
    public KeyCode phimSkip = KeyCode.K;

    [Header("Biến Hình Logo (Morph)")]
    [InspectorName("Thời Lượng Biến Hình")]
    public float morphDuration = 0.6f;
    [InspectorName("Độ Trễ Tách")]
    public float splitDelay = 0.2f;
    [InspectorName("Số Lượng Mảnh")]
    public int morphPieceCount = 60;
    [InspectorName("Bán Kính Tung")]
    public float spreadRadius = 15f;
    [InspectorName("Thời Lượng Bay")]
    public float flightDuration = 2f;
    [InspectorName("Thời Lượng Lắp Ráp")]
    public float assembleDuration = 0.9f;
    [InspectorName("Mục Tiêu Mũi Tên")]
    public Transform arrowTarget; // vị trí cuối cùng là mũi tên
    [InspectorName("Tên Cảnh Tiếp Theo")]
    public string nextSceneName = ""; // cảnh tiếp theo sau khi xong (tuỳ chọn)

    private List<ManhGhepPuzzle> danhSachManh = new List<ManhGhepPuzzle>();
    private int emptyIndex = 8;  // Vị trí ô trống (index 8 là góc phải dưới)
    
    private Vector3[] gridPositions = new Vector3[9];  // Vị trí world của từng slot 0-8
    private Dictionary<int, ManhGhepPuzzle> currentIndexToTile = new Dictionary<int, ManhGhepPuzzle>();  // currentIndex -> mảnh

    private bool daTaoPuzzle = false;
    private bool puzzleDangHoatDong = false;

    // Biến cho morph sequence
    private List<GameObject> morphPieces = new List<GameObject>();
    private SpriteRenderer logoLonSprite = null;
    private Vector3 savedLogoLonPosition = Vector3.zero; // lưu vị trí logoLon trước khi ẩn
    private Transform savedLogoLonParent = null; // lưu parent của logoLon

    private void Start()
    {
        if (logoMo != null)
            logoMo.SetActive(false);

        if (logoLon != null)
            logoLon.SetActive(false);

        if (khungPuzzle != null)
            khungPuzzle.gameObject.SetActive(false);

        StartCoroutine(HienLogoMoSauVaiGiay());
    }

    private void Update()
    {
        if (batDevSkip && Input.GetKeyDown(phimSkip))
        {
            DevSolvePuzzle();
        }
    }

    private IEnumerator HienLogoMoSauVaiGiay()
    {
        yield return new WaitForSeconds(thoiGianLogoMoXuatHien);

        if (logoMo != null)
        {
            StartCoroutine(AnimateShowObject(logoMo, appearDuration));
        }
    }

    public void BamVaoLogoMo()
    {
        if (daTaoPuzzle) return;

        StartCoroutine(ChayLogoLonRoiTaoPuzzle());
    }

    private IEnumerator ChayLogoLonRoiTaoPuzzle()
    {
        daTaoPuzzle = true;

        // Nếu có cả logoMo và logoLon, chuyển đổi trực tiếp từ logoMo -> logoLon
        if (logoMo != null && logoLon != null)
        {
            // đảm bảo logoLon inactive và được đặt ở đúng vị trí
            logoLon.SetActive(true);
            // animate transform from logoMo to logoLon
            yield return StartCoroutine(AnimateTransformBetween(logoMo, logoLon, appearDuration));
            // giữ logoLon hiện trong thời gian định trước
            yield return new WaitForSeconds(thoiGianLogoLonHien);
            // animate transform from logoLon back to puzzle start (hide)
            yield return StartCoroutine(AnimateHideObject(logoLon, disappearDuration));
        }
        else
        {
            if (logoMo != null) yield return StartCoroutine(AnimateHideObject(logoMo, disappearDuration));
            if (logoLon != null) yield return StartCoroutine(AnimateShowObject(logoLon, appearDuration));
            yield return new WaitForSeconds(thoiGianLogoLonHien);
            if (logoLon != null) yield return StartCoroutine(AnimateHideObject(logoLon, disappearDuration));
        }

        TaoPuzzleTuDong();
    }

    private IEnumerator ChayMorphSequence()
    {
        // 1) Chuyển mờ từ logo bình thường sang logo ma
        if (logoLonSprite == null || logoMaMiSprite == null)
        {
            Debug.LogWarning("Logo sprite chưa được gán. Bỏ qua morph sequence.");
            yield break;
        }

        // Giữ logo bình thường hiển thị trong thoiGianLogoLonHien
        yield return new WaitForSeconds(thoiGianLogoLonHien);

        // Fade out logo bình thường
        float t = 0f;
        Color from = logoLonSprite.color;
        Color to = from;
        to.a = 0f;

        while (t < morphDuration)
        {
            t += Time.deltaTime;
            float f = t / morphDuration;
            logoLonSprite.color = Color.Lerp(from, to, f);
            yield return null;
        }

        // Thay bằng sprite ma và fade in
        logoLonSprite.sprite = logoMaMiSprite;
        t = 0f;
        logoLonSprite.color = new Color(from.r, from.g, from.b, 0f);
        while (t < morphDuration)
        {
            t += Time.deltaTime;
            float f = t / morphDuration;
            logoLonSprite.color = new Color(from.r, from.g, from.b, f);
            yield return null;
        }

        // Chờ chút rồi tách thành mảnh
        yield return new WaitForSeconds(splitDelay);

        // 2) Tạo các mảnh từ sprite ma
        TaoManhMorph();

        // Ẩn sprite ma
        logoLonSprite.enabled = false;

        // 3) Tung các mảnh ra ngoài
        foreach (var p in morphPieces)
        {
            Vector3 dir = (p.transform.position - logoLon.transform.position).normalized;
            if (dir == Vector3.zero) dir = Random.onUnitSphere;
            Vector3 target = logoLon.transform.position + dir * spreadRadius + Random.insideUnitSphere * 0.5f;
            StartCoroutine(DiChuyenManhMorph(p.transform, p.transform.position, target, flightDuration, true));
        }

        yield return new WaitForSeconds(flightDuration + 0.05f);

        // 4) Hợp lại thành mũi tên
        if (arrowTarget != null)
        {
            int i = 0;
            foreach (var p in morphPieces)
            {
                Vector3 tarPos = arrowTarget.position + (Random.insideUnitSphere * 0.05f);
                StartCoroutine(DiChuyenManhMorph(p.transform, p.transform.position, tarPos, assembleDuration, false, i * 0.01f));
                i++;
            }

            yield return new WaitForSeconds(assembleDuration + 0.05f);
        }

        // Chờ chút rồi xóa các mảnh morph
        yield return new WaitForSeconds(0.3f);

        foreach (var p in morphPieces)
        {
            Destroy(p);
        }
        morphPieces.Clear();
    }

    private IEnumerator ChayMorphSequenceAfterPuzzle()
    {
        // Flow sau khi giải puzzle xong:
        // 1) LogoBinhThuong chuyển dần sang LogoMa
        // 2) LogoMa vỡ thành mảnh nhỏ
        // 3) Mảnh bay loạn ra xung quanh khắp màn hình (2 giây)
        // 4) Đợi 2 giây mảnh đứng yên
        // 5) Mảnh tụ lại thành mũi tên
        // 6) Ẩn mảnh, hiện mũi tên thật
        // 7) Gắn OnMouseDown để chuyển scene

        if (logoLonSprite == null || logoMaBienHinhSprite == null)
        {
            Debug.LogWarning("Logo sprite chưa được gán. Bỏ qua morph sequence.");
            yield break;
        }

        // 1) Fade out logo bình thường, fade in logo ma
        float t = 0f;
        Color from = logoLonSprite.color;
        Color to = from;
        to.a = 0f;

        // Fade out
        while (t < morphDuration)
        {
            t += Time.deltaTime;
            float f = t / morphDuration;
            logoLonSprite.color = Color.Lerp(from, to, f);
            yield return null;
        }

        // Thay sprite thành ma
        logoLonSprite.sprite = logoMaBienHinhSprite;
        t = 0f;
        logoLonSprite.color = new Color(from.r, from.g, from.b, 0f);

        // Fade in logo ma
        while (t < morphDuration)
        {
            t += Time.deltaTime;
            float f = t / morphDuration;
            logoLonSprite.color = new Color(from.r, from.g, from.b, f);
            yield return null;
        }

        // Chờ chút rồi tách
        yield return new WaitForSeconds(splitDelay);

        // 2) Tạo các mảnh từ sprite ma biến hình
        TaoManhMorphTuSpriteMa(logoMaBienHinhSprite);

        // Ẩn sprite ma gốc
        logoLonSprite.enabled = false;

        // 3) Tung các mảnh ra ngoài khắp màn hình (bay loạn xạ trong 2 giây)
        foreach (var p in morphPieces)
        {
            if (p == null) continue;
            
            // Bay ngẫu nhiên khắp màn hình
            Vector3 randomDir = Random.onUnitSphere;
            float randomDist = Random.Range(spreadRadius * 0.7f, spreadRadius);
            Vector3 target = savedLogoLonPosition + randomDir * randomDist;
            
            StartCoroutine(DiChuyenManhMorph(p.transform, p.transform.position, target, flightDuration, true));
        }

        // 4) Đợi mảnh bay loạn xạ 2 giây
        yield return new WaitForSeconds(flightDuration);

        // 5) Hợp lại thành mũi tên
        if (arrowTarget != null)
        {
            int i = 0;
            foreach (var p in morphPieces)
            {
                if (p == null) continue;
                
                Vector3 tarPos = arrowTarget.position + (Random.insideUnitSphere * 0.05f);
                StartCoroutine(DiChuyenManhMorph(p.transform, p.transform.position, tarPos, assembleDuration, false, i * 0.01f));
                i++;
            }

            yield return new WaitForSeconds(assembleDuration + 0.05f);
        }

        // 6) Ẩn mảnh morph
        yield return new WaitForSeconds(0.3f);

        foreach (var p in morphPieces)
        {
            if (p != null)
                Destroy(p);
        }
        morphPieces.Clear();

        // 7) Hiện mũi tên thật và gắn OnMouseDown
        if (arrowTarget != null && arrowTarget.gameObject != null && arrowTarget.gameObject.activeSelf == false)
        {
            arrowTarget.gameObject.SetActive(true);
            
            // Gắn script xử lý click vào mũi tên
            if (arrowTarget.GetComponent<DiDenBangXepHinh>() == null)
            {
                arrowTarget.gameObject.AddComponent<DiDenBangXepHinh>();
            }
        }

        Debug.Log("✓ Morph sequence hoàn tất!");
    }

    private void TaoManhMorph()
    {
        // Cắt sprite ma thành lưới mảnh nhỏ
        if (logoMaMiSprite == null || logoLon == null) return;

        Texture2D tex = logoMaMiSprite.texture;
        if (tex == null) return;

        // Tính grid
        int cols = Mathf.CeilToInt(Mathf.Sqrt(morphPieceCount));
        int rows = Mathf.CeilToInt((float)morphPieceCount / cols);
        
        float texPieceW = (float)tex.width / cols;
        float texPieceH = (float)tex.height / rows;
        
        Bounds b = logoLonSprite.bounds;
        float worldPieceW = b.size.x / cols;
        float worldPieceH = b.size.y / rows;

        int created = 0;
        for (int y = 0; y < rows && created < morphPieceCount; y++)
        {
            for (int x = 0; x < cols && created < morphPieceCount; x++)
            {
                // Tạo rect cắt từ texture
                Rect rect = new Rect(x * texPieceW, (rows - y - 1) * texPieceH, texPieceW, texPieceH);
                
                // Tạo sprite từ rect
                Sprite pieceSprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), logoMaMiSprite.pixelsPerUnit);
                
                // Vị trí trong thế giới
                Vector3 pos = b.min + new Vector3(
                    (x + 0.5f) * worldPieceW,
                    (y + 0.5f) * worldPieceH,
                    logoLon.transform.position.z
                );
                
                // Tạo GameObject
                GameObject go = new GameObject("MorphPiece_" + created);
                go.transform.position = pos;
                go.transform.parent = logoLon.transform.parent;
                
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pieceSprite;
                sr.sortingOrder = logoLonSprite.sortingOrder;
                sr.color = logoLonSprite.color;
                
                morphPieces.Add(go);
                created++;
            }
        }
    }

    private void TaoManhMorphTuSpriteMa(Sprite spriteLogoMa)
    {
        // Cắt sprite ma bất kỳ thành lưới mảnh nhỏ
        if (spriteLogoMa == null || logoLonSprite == null) return;

        Texture2D tex = spriteLogoMa.texture;
        if (tex == null) return;

        // Clear mảnh cũ
        morphPieces.Clear();

        // Tính grid
        int cols = Mathf.CeilToInt(Mathf.Sqrt(morphPieceCount));
        int rows = Mathf.CeilToInt((float)morphPieceCount / cols);
        
        float texPieceW = (float)tex.width / cols;
        float texPieceH = (float)tex.height / rows;
        
        Bounds b = logoLonSprite.bounds;
        float worldPieceW = b.size.x / cols;
        float worldPieceH = b.size.y / rows;

        int created = 0;
        for (int y = 0; y < rows && created < morphPieceCount; y++)
        {
            for (int x = 0; x < cols && created < morphPieceCount; x++)
            {
                // Tạo rect cắt từ texture
                Rect rect = new Rect(x * texPieceW, (rows - y - 1) * texPieceH, texPieceW, texPieceH);
                
                // Tạo sprite từ rect
                Sprite pieceSprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), spriteLogoMa.pixelsPerUnit);
                
                // Vị trí trong thế giới
                Vector3 pos = b.min + new Vector3(
                    (x + 0.5f) * worldPieceW,
                    (y + 0.5f) * worldPieceH,
                    savedLogoLonPosition.z
                );
                
                // Tạo GameObject
                GameObject go = new GameObject("MorphPiece_" + created);
                go.transform.position = pos;
                go.transform.parent = savedLogoLonParent;
                
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pieceSprite;
                sr.sortingOrder = logoLonSprite.sortingOrder;
                sr.color = logoLonSprite.color;
                
                morphPieces.Add(go);
                created++;
            }
        }
    }

    private IEnumerator DiChuyenManhMorph(Transform tform, Vector3 from, Vector3 to, float dur, bool randomRotate, float delay = 0f)
    {
        if (tform == null) yield break;
        
        if (delay > 0f) yield return new WaitForSeconds(delay);
        
        float time = 0f;
        Quaternion startRot = tform.rotation;
        Quaternion endRot = randomRotate ? Random.rotation : startRot;
        Vector3 startScale = tform.localScale;
        Vector3 endScale = randomRotate ? Vector3.one * 0.3f : Vector3.one;

        while (time < dur && tform != null)
        {
            time += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, time / dur);
            if (tform != null)
            {
                tform.position = Vector3.Lerp(from, to, f);
                tform.rotation = Quaternion.Slerp(startRot, endRot, f);
                tform.localScale = Vector3.Lerp(startScale, endScale, f);
            }
            yield return null;
        }
        
        if (tform != null)
        {
            tform.position = to;
            tform.rotation = endRot;
            tform.localScale = endScale;
        }
    }

    private void TaoPuzzleTuDong()
    {
        if (logoMaMiSprite == null)
        {
            Debug.LogError("Chưa kéo Logo Ma Mi Sprite vào QuanLyXepHinh.");
            return;
        }

        if (khungPuzzle == null)
        {
            Debug.LogError("Chưa kéo KhungPuzzle vào QuanLyXepHinh.");
            return;
        }

        khungPuzzle.gameObject.SetActive(true);

        XoaPuzzleCu();

        Texture2D texture = logoMaMiSprite.texture;

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        int cellWidth = textureWidth / gridSize;
        int cellHeight = textureHeight / gridSize;

        float gapWorld = gapPixels / 100f;

        // Tính toán vị trí grid trong world
        float totalSize = gridSize * tileSize + (gridSize - 1) * gapWorld;
        Vector3 startPos = puzzleCenter - new Vector3(totalSize / 2f - tileSize / 2f, -totalSize / 2f + tileSize / 2f, 0);

        // Tạo 9 vị trí slot (0-8)
        for (int index = 0; index < 9; index++)
        {
            int x = index % gridSize;
            int y = index / gridSize;
            gridPositions[index] = startPos + new Vector3(
                x * (tileSize + gapWorld),
                -y * (tileSize + gapWorld),
                0
            );
        }

        // Tạo 8 mảnh (correctIndex 0-7), bỏ slot 8
        for (int correctIndex = 0; correctIndex < 8; correctIndex++)
        {
            int gridX = correctIndex % gridSize;
            int gridY = correctIndex / gridSize;

            // Cắt rect từ texture (Y từ dưới lên để phù hợp UV)
            Rect rect = new Rect(
                gridX * cellWidth,
                (gridSize - 1 - gridY) * cellHeight,
                cellWidth,
                cellHeight
            );

            Sprite spriteManh = Sprite.Create(
                texture,
                rect,
                new Vector2(0.5f, 0.5f),
                logoMaMiSprite.pixelsPerUnit
            );

            GameObject obj = new GameObject("ManhGhep_" + correctIndex);
            obj.transform.SetParent(khungPuzzle);
            obj.transform.localScale = Vector3.one * 0.001f; // start tiny for appear anim
            obj.transform.position = gridPositions[correctIndex];  // Ban đầu ở đúng vị trí

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = spriteManh;
            sr.sortingOrder = 20;

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            ManhGhepPuzzle manh = obj.AddComponent<ManhGhepPuzzle>();
            manh.quanLy = this;
            manh.correctIndex = correctIndex;
            manh.currentIndex = correctIndex;  // Ban đầu ở đúng vị trí

            // Scale mảnh
            float spriteWorldWidth = sr.bounds.size.x;
            float scaleCan = tileSize / spriteWorldWidth;
            obj.transform.localScale = new Vector3(scaleCan, scaleCan, 1f);

            danhSachManh.Add(manh);
            currentIndexToTile[manh.currentIndex] = manh;

            if (batVienPhatSang)
            {
                TaoVienPhatSang(obj, sr);
            }
        }

        // Xáo trộn puzzle
        XaoTronPuzzle(50);

        // Animate appear for all tiles
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            if (manh != null)
                StartCoroutine(AnimateScaleTo(manh.gameObject.transform, tileSize / 1f, appearDuration));
        }

        // Cập nhật lại currentIndexToTile sau khi xáo trộn để đảm bảo đúng
        currentIndexToTile.Clear();
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            currentIndexToTile[manh.currentIndex] = manh;
        }
        
        // Kiểm tra xem puzzle có bị xáo trộn chưa
        bool isPuzzleShuffled = false;
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            if (manh.currentIndex != manh.correctIndex)
            {
                isPuzzleShuffled = true;
                break;
            }
        }
        
        if (isPuzzleShuffled)
        {
            Debug.Log("✓ Puzzle đã được xáo trộn thành công!");
        }
        else
        {
            Debug.LogWarning("⚠ Cảnh báo: Puzzle chưa bị xáo trộn! Tất cả mảnh vẫn ở vị trí gốc.");
        }

        puzzleDangHoatDong = true;
    }

    private void TaoVienPhatSang(GameObject obj, SpriteRenderer spriteGoc)
    {
        GameObject vien = new GameObject("VienPhatSang");
        vien.transform.SetParent(obj.transform);
        vien.transform.localPosition = Vector3.zero;
        vien.transform.localRotation = Quaternion.identity;
        vien.transform.localScale = Vector3.one * (1f + glowScaleThem);

        SpriteRenderer srVien = vien.AddComponent<SpriteRenderer>();
        srVien.sprite = spriteGoc.sprite;
        srVien.sortingOrder = spriteGoc.sortingOrder - 1;

        Color c = mauVienPhatSang;
        c.a = glowAlpha;
        srVien.color = c;
    }

    private void XaoTronPuzzle(int soLanDiChuyen)
    {
        List<int> moveHistory = new List<int>();
        
        for (int i = 0; i < soLanDiChuyen; i++)
        {
            // Tìm tất cả slot kề emptyIndex
            List<int> slotKe = TimSlotKe(emptyIndex);

            if (slotKe.Count == 0)
                break;

            // Chọn ngẫu nhiên một slot kề
            int randomSlot = slotKe[Random.Range(0, slotKe.Count)];

            // Tìm mảnh ở slot đó - QUAN TRỌNG: Phải tìm mảnh bằng currentIndex
            ManhGhepPuzzle manh = null;
            foreach (ManhGhepPuzzle m in danhSachManh)
            {
                if (m.currentIndex == randomSlot)
                {
                    manh = m;
                    break;
                }
            }

            if (manh != null)
            {
                DiChuyenManh(manh);
                moveHistory.Add(randomSlot);
            }
        }
        
        // Debug: Kiểm tra sau xáo trộn
        Debug.Log($"Xáo trộn hoàn thành: {moveHistory.Count} bước. Empty Index: {emptyIndex}");
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            Debug.Log($"Mảnh {manh.correctIndex}: currentIndex={manh.currentIndex}, position={manh.transform.position}");
        }
    }

    private List<int> TimSlotKe(int slotIndex)
    {
        List<int> result = new List<int>();

        int x = slotIndex % gridSize;
        int y = slotIndex / gridSize;

        // 4 hướng: trái, phải, trên, dưới
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
            {
                int neighborSlot = ny * gridSize + nx;
                result.Add(neighborSlot);
            }
        }

        return result;
    }

    public void ThuDiChuyen(ManhGhepPuzzle manh)
    {
        if (!puzzleDangHoatDong) return;
        if (manh == null) return;

        // Kiểm tra xem currentIndex của mảnh có kề emptyIndex không
        if (!CoKe(manh.currentIndex, emptyIndex))
        {
            return;
        }

        // Di chuyển mảnh
        DiChuyenManh(manh);

        // Kiểm tra hoàn thành
        KiemTraHoanThanh();
    }

    private bool CoKe(int index1, int index2)
    {
        int x1 = index1 % gridSize;
        int y1 = index1 / gridSize;
        int x2 = index2 % gridSize;
        int y2 = index2 / gridSize;

        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) == 1;
    }

    private void DiChuyenManh(ManhGhepPuzzle manh)
    {
        int viTriCuaManh = manh.currentIndex;

        // Cập nhật dictionary
        currentIndexToTile.Remove(viTriCuaManh);
        currentIndexToTile[emptyIndex] = manh;

        // Mảnh trượt sang ô trống
        manh.currentIndex = emptyIndex;
        manh.transform.position = gridPositions[emptyIndex];

        // Ô trống chuyển sang vị trí cũ của mảnh
        emptyIndex = viTriCuaManh;
        
        // Debug
        //Debug.Log($"Di chuyển mảnh {manh.correctIndex}: {viTriCuaManh} → {manh.currentIndex}, vị trí: {manh.transform.position}");
    }

    private void KiemTraHoanThanh()
    {
        // Kiểm tra tất cả mảnh có currentIndex == correctIndex không
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            if (manh.currentIndex != manh.correctIndex)
            {
                return;
            }
        }

        // Kiểm tra emptyIndex == 8 (ô trống ở vị trí ban đầu)
        if (emptyIndex != 8)
        {
            return;
        }

        puzzleDangHoatDong = false;
        OnPuzzleSolved();
    }

    private void OnPuzzleSolved()
    {
        Debug.Log("Puzzle solved! Checking all tiles:");
        
        // Kiểm tra lại để chắc chắn tất cả đúng
        bool allCorrect = true;
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            Debug.Log($"Mảnh {manh.correctIndex}: currentIndex={manh.currentIndex}, correctIndex={manh.correctIndex}, Match={manh.currentIndex == manh.correctIndex}");
            if (manh.currentIndex != manh.correctIndex)
            {
                allCorrect = false;
            }
        }
        Debug.Log($"EmptyIndex: {emptyIndex} (nên là 8)");
        Debug.Log($"All tiles correct? {allCorrect}");
        
        if (!allCorrect)
        {
            Debug.LogWarning("Cảnh báo: Không phải tất cả mảnh đều ở vị trí đúng!");
            return;
        }
        
        // Ẩn tất cả 8 mảnh puzzle với animation
        StartCoroutine(HideTilesThenShowLogo());
        
    }

    private IEnumerator HideTilesThenShowLogo()
    {
        // Animate each tile moving/scaling/fading into the final logo area
        Vector3 finalPos = puzzleCenter;
        // compute final scale for tiles to morph into logo: use logo final scale
        float gapWorld = gapPixels / 100f;
        float totalSize = gridSize * tileSize + (gridSize - 1) * gapWorld;
        // final logo scale (world size to sprite size)
        float logoSpriteWidth = logoBinhThuongSprite != null ? logoBinhThuongSprite.bounds.size.x : 1f;
        float logoFinalScale = (logoSpriteWidth > 0f) ? (totalSize / logoSpriteWidth) : 1f;

        List<Coroutine> running = new List<Coroutine>();
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            if (manh != null)
            {
                Vector3 targetScale = Vector3.one * logoFinalScale;
                running.Add(StartCoroutine(AnimateTileTo(manh.transform, finalPos, targetScale, disappearDuration)));
            }
        }

        // wait for animations
        yield return new WaitForSeconds(disappearDuration + 0.02f);

        // Deactivate tiles after animation
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            if (manh != null)
                manh.gameObject.SetActive(false);
        }

        // Hiển thị logo bình thường (fade in)
        if (logoBinhThuongSprite != null)
        {
            HienLogoBinhThuong();
            // fade in final logo
            GameObject finalLogo = GameObject.Find("LogoBinhThuong");
            if (finalLogo != null)
            {
                StartCoroutine(AnimateShowObject(finalLogo, appearDuration));
                // Lấy SpriteRenderer
                logoLonSprite = finalLogo.GetComponent<SpriteRenderer>();
                
                // Lưu vị trí và parent trước khi morph
                if (logoLonSprite != null)
                {
                    savedLogoLonPosition = finalLogo.transform.position;
                    savedLogoLonParent = finalLogo.transform.parent;
                }
            }
            Debug.Log("Logo bình thường đã hiển thị!");
            
            // Đợi 1 giây rồi chạy morph sequence
            yield return new WaitForSeconds(1f);
            
            // Chạy morph sequence
            yield return StartCoroutine(ChayMorphSequenceAfterPuzzle());
        }
        else
        {
            Debug.LogWarning("Chưa kéo Logo Bình Thường Sprite vào QuanLyXepHinh.");
        }
    }
    
    private void HienLogoBinhThuong()
    {
        Debug.Log("Bắt đầu hiển thị logo bình thường...");

        // Tạo GameObject mới để hiển thị logo bình thường (không phụ thuộc vào parent)
        GameObject logoObject = new GameObject("LogoBinhThuong");
        logoObject.transform.SetParent(null);
        logoObject.transform.position = puzzleCenter; // Đặt ở tâm puzzle (world space)
        logoObject.transform.rotation = Quaternion.identity;
        logoObject.transform.localScale = Vector3.one;

        SpriteRenderer sr = logoObject.AddComponent<SpriteRenderer>();
        sr.sprite = logoBinhThuongSprite;
        sr.sortingOrder = 30;  // Cao hơn mảnh puzzle (20)

        Debug.Log($"Logo sprite size: {logoBinhThuongSprite.bounds.size}");

        // Tính kích thước tổng thể của puzzle (world units)
        float gapWorld = gapPixels / 100f;
        float totalSize = gridSize * tileSize + (gridSize - 1) * gapWorld;

        Debug.Log($"Total puzzle size: {totalSize}");

        // Lấy kích thước sprite (tại scale 1)
        Bounds spriteBounds = logoBinhThuongSprite.bounds;
        float spriteWorldWidth = spriteBounds.size.x;
        float spriteWorldHeight = spriteBounds.size.y;

        Debug.Log($"Sprite world dimensions: {spriteWorldWidth} x {spriteWorldHeight}");

        // Tính scale để logo vừa khung puzzle
        float scaleX = (spriteWorldWidth > 0) ? (totalSize / spriteWorldWidth) : 1f;
        float scaleY = (spriteWorldHeight > 0) ? (totalSize / spriteWorldHeight) : 1f;
        float scale = Mathf.Min(scaleX, scaleY);

        Debug.Log($"Calculated scale: {scale} (scaleX={scaleX}, scaleY={scaleY})");

        logoObject.transform.localScale = new Vector3(scale, scale, 1f);

        // Đặt Z sao cho không bị che bởi tile (tile z may be 0), giữ z của puzzleCenter
        Vector3 pos = logoObject.transform.position;
        logoObject.transform.position = new Vector3(pos.x, pos.y, pos.z);

        Debug.Log($"Logo bình thường created at: {logoObject.transform.position}, scale: {logoObject.transform.localScale}");
    }

    public void DevSolvePuzzle()
    {
        if (danhSachManh.Count == 0)
        {
            Debug.LogWarning("Chưa có puzzle để solve.");
            return;
        }

        // Xóa map cũ
        currentIndexToTile.Clear();

        // Đưa từng mảnh về đúng slot dựa correctIndex
        foreach (ManhGhepPuzzle manh in danhSachManh)
        {
            manh.currentIndex = manh.correctIndex;
            manh.transform.position = gridPositions[manh.correctIndex];
            currentIndexToTile[manh.correctIndex] = manh;
        }

        // Ô trống về vị trí ban đầu
        emptyIndex = 8;

        puzzleDangHoatDong = false;

        // Gọi OnPuzzleSolved để ẩn mảnh và hiển thị logo bình thường
        OnPuzzleSolved();
        
        Debug.Log("DEV SOLVE: Puzzle đã được xếp hoàn chỉnh và logo bình thường được hiển thị.");
    }

    private void XoaPuzzleCu()
    {
        danhSachManh.Clear();
        currentIndexToTile.Clear();
        emptyIndex = 8;

        if (khungPuzzle != null)
        {
            for (int i = khungPuzzle.childCount - 1; i >= 0; i--)
            {
                Destroy(khungPuzzle.GetChild(i).gameObject);
            }
        }
    }

    // --- Animation helpers ---
    private IEnumerator AnimateShowObject(GameObject go, float duration)
    {
        if (go == null) yield break;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        go.SetActive(true);

        Vector3 initial = Vector3.one * 0.001f;
        Vector3 target = Vector3.one;
        float t = 0f;
        go.transform.localScale = initial;

        Color initialColor = sr != null ? sr.color : Color.white;
        if (sr != null)
        {
            Color c = initialColor; c.a = 0f; sr.color = c;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            go.transform.localScale = Vector3.Lerp(initial, target, p);
            if (sr != null)
            {
                Color c = initialColor; c.a = Mathf.Lerp(0f, initialColor.a, p); sr.color = c;
            }
            yield return null;
        }

        go.transform.localScale = target;
        if (sr != null) sr.color = initialColor;
    }

    private IEnumerator AnimateHideObject(GameObject go, float duration)
    {
        if (go == null) yield break;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        Vector3 initial = go.transform.localScale;
        Vector3 target = Vector3.one * 0.001f;
        float t = 0f;
        Color initialColor = sr != null ? sr.color : Color.white;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            go.transform.localScale = Vector3.Lerp(initial, target, p);
            if (sr != null)
            {
                Color c = initialColor; c.a = Mathf.Lerp(initialColor.a, 0f, p); sr.color = c;
            }
            yield return null;
        }
        go.transform.localScale = target;
        if (sr != null)
        {
            Color c = initialColor; c.a = 0f; sr.color = c;
        }
        go.SetActive(false);
    }

    private IEnumerator AnimateScaleTo(Transform tr, float targetTileSize, float duration)
    {
        if (tr == null) yield break;
        Vector3 initial = tr.localScale;
        // compute target scale so that sprite fits tileSize: assume sprite was scaled earlier to match tileSize in x at localScale 1
        Vector3 target = Vector3.one * (targetTileSize / tileSize);
        // But many tiles were scaled during creation; we'll lerp the localScale to the existing scale based on desired final 1:1
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            tr.localScale = Vector3.Lerp(initial, Vector3.one * (tileSize / (GetSpriteWidth(tr) > 0 ? GetSpriteWidth(tr) : tileSize)), p);
            // also fade in sprite if exists
            SpriteRenderer sr = tr.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color; c.a = Mathf.Lerp(0f, 1f, p); sr.color = c;
            }
            yield return null;
        }
        tr.localScale = Vector3.one * (tileSize / (GetSpriteWidth(tr) > 0 ? GetSpriteWidth(tr) : tileSize));
        SpriteRenderer srFinal = tr.GetComponent<SpriteRenderer>();
        if (srFinal != null)
        {
            Color c = srFinal.color; c.a = 1f; srFinal.color = c;
        }
    }

    private IEnumerator AnimateTileTo(Transform tr, Vector3 targetPos, Vector3 targetScale, float duration)
    {
        if (tr == null) yield break;
        SpriteRenderer sr = tr.GetComponent<SpriteRenderer>();
        Vector3 initialPos = tr.position;
        Vector3 initialScale = tr.localScale;
        Color initialColor = sr != null ? sr.color : Color.white;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            tr.position = Vector3.Lerp(initialPos, targetPos, p);
            tr.localScale = Vector3.Lerp(initialScale, targetScale, p);
            if (sr != null)
            {
                Color c = initialColor; c.a = Mathf.Lerp(initialColor.a, 0f, p); sr.color = c;
            }
            yield return null;
        }
        tr.position = targetPos;
        tr.localScale = targetScale;
        if (sr != null)
        {
            Color c = initialColor; c.a = 0f; sr.color = c;
        }
    }

    private IEnumerator AnimateTransformBetween(GameObject src, GameObject dst, float duration)
    {
        if (src == null || dst == null) yield break;
        SpriteRenderer srSrc = src.GetComponent<SpriteRenderer>();
        SpriteRenderer srDst = dst.GetComponent<SpriteRenderer>();

        Vector3 startPos = src.transform.position;
        Vector3 endPos = dst.transform.position;
        Vector3 startScale = src.transform.localScale;
        Vector3 endScale = dst.transform.localScale;
        Color startColor = srSrc != null ? srSrc.color : Color.white;
        Color endColor = srDst != null ? srDst.color : Color.white;

        // ensure dst starts invisible
        if (srDst != null)
        {
            Color c = endColor; c.a = 0f; srDst.color = c;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            // animate src towards dst transform
            if (src != null)
            {
                src.transform.position = Vector3.Lerp(startPos, endPos, p);
                src.transform.localScale = Vector3.Lerp(startScale, endScale, p);
                if (srSrc != null)
                {
                    Color c = startColor; c.a = Mathf.Lerp(startColor.a, 0f, p); srSrc.color = c;
                }
            }
            if (srDst != null)
            {
                Color c2 = endColor; c2.a = Mathf.Lerp(0f, endColor.a, p); srDst.color = c2;
            }
            yield return null;
        }

        // finalize
        if (src != null)
        {
            if (srSrc != null)
            {
                Color c = startColor; c.a = 0f; srSrc.color = c;
            }
            src.SetActive(false);
        }
        if (dst != null && srDst != null)
        {
            srDst.color = endColor;
        }
    }

    private float GetSpriteWidth(Transform tr)
    {
        SpriteRenderer sr = tr.GetComponent<SpriteRenderer>();
        if (sr == null) return 1f;
        return sr.bounds.size.x / tr.localScale.x;
    }
}