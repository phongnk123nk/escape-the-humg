using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Gắn script này vào cùng GameObject chứa `SpriteRenderer` của logo bình thường.
// Script này giả định script logo cũ sẽ gọi `StartMorph()` khi sẵn sàng.
public class LogoMorphController : MonoBehaviour
{
    [Header("Tham chiếu")]
    [InspectorName("Logo Bình Thường")]
    public SpriteRenderer normalLogo; // logo gốc
    [InspectorName("Sprite Ma")]
    public Sprite ghostSprite; // sprite phiên bản ma (sẽ được cắt thành mảnh)

    [Header("Thời lượng hiệu ứng")]
    [InspectorName("Thời Lượng Biến Hình")]
    public float morphDuration = 0.6f;
    [InspectorName("Độ Trễ Tách")]
    public float splitDelay = 0.2f;
    [InspectorName("Số Lượng Mảnh")]
    public int pieceCount = 60;
    [InspectorName("Bán Kính Tung")]
    public float spreadRadius = 3f;
    [InspectorName("Thời Lượng Bay")]
    public float flightDuration = 1.2f;
    [InspectorName("Thời Lượng Lắp Ráp")]
    public float assembleDuration = 0.9f;

    [Header("Mũi tên (đích)")]
    [InspectorName("Mục Tiêu Mũi Tên")]
    public Transform arrowTarget; // vị trí/rotation/scale của mũi tên cuối cùng

    [Header("Cảnh tiếp theo")]
    [InspectorName("Tên Cảnh Tiếp Theo")]
    public string nextSceneName;

    List<GameObject> pieces = new List<GameObject>();

    // Phương thức public để script trước gọi khi đến lúc biến hình
    public void StartMorph()
    {
        StartCoroutine(MorphSequence());
    }

    IEnumerator MorphSequence()
    {
        // 1) Chuyển mờ sang sprite ma
        if (normalLogo == null) yield break;

        SpriteRenderer sr = normalLogo;
        float t = 0f;
        Color from = sr.color;
        Color to = from;
        to.a = 0f;

        // Làm mờ logo gốc rồi đổi sang sprite ma (cách đơn giản)
        while (t < morphDuration)
        {
            t += Time.deltaTime;
            float f = t / morphDuration;
            sr.color = Color.Lerp(from, to, f);
            yield return null;
        }

        // Thay bằng sprite ma và làm hiện dần
        sr.sprite = ghostSprite;
        t = 0f;
        sr.color = new Color(from.r, from.g, from.b, 0f);
        while (t < morphDuration)
        {
            t += Time.deltaTime;
            float f = t / morphDuration;
            sr.color = new Color(from.r, from.g, from.b, f);
            yield return null;
        }

        // chờ chút rồi tách thành mảnh
        yield return new WaitForSeconds(splitDelay);

        // 2) Tạo các mảnh từ vùng bounds của logo
        CreatePieces();

        // ẩn sprite chính
        sr.enabled = false;

        // 3) Tung các mảnh ra ngoài
        foreach (var p in pieces)
        {
            Vector3 dir = (p.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Random.onUnitSphere;
            Vector3 target = transform.position + dir * spreadRadius + Random.insideUnitSphere * 0.5f;
            StartCoroutine(MoveAndScale(p.transform, p.transform.position, target, flightDuration, true));
        }

        yield return new WaitForSeconds(flightDuration + 0.05f);

        // 4) Tập hợp thành mũi tên
        if (arrowTarget != null)
        {
            int i = 0;
            foreach (var p in pieces)
            {
                Vector3 tarPos = arrowTarget.position + (Random.insideUnitSphere * 0.05f);
                StartCoroutine(MoveAndScale(p.transform, p.transform.position, tarPos, assembleDuration, false, i * 0.01f));
                i++;
            }

            yield return new WaitForSeconds(assembleDuration + 0.05f);
        }

        // tuỳ chọn: căn chỉnh mảnh theo transform mũi tên, đợi rồi chuyển cảnh
        yield return new WaitForSeconds(0.3f);

        // Làm sạch các mảnh
        foreach (var p in pieces)
        {
            Destroy(p);
        }
        pieces.Clear();

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void CreatePieces()
    {
        // Cắt sprite ma thành lưới mảnh nhỏ bằng thuật toán grid slicing
        if (ghostSprite == null || normalLogo == null) return;

        Texture2D tex = ghostSprite.texture;
        if (tex == null) return;

        // Tính grid
        int cols = Mathf.CeilToInt(Mathf.Sqrt(pieceCount));
        int rows = Mathf.CeilToInt((float)pieceCount / cols);
        
        float texPieceW = (float)tex.width / cols;
        float texPieceH = (float)tex.height / rows;
        
        Bounds b = normalLogo.bounds;
        float worldPieceW = b.size.x / cols;
        float worldPieceH = b.size.y / rows;

        int created = 0;
        for (int y = 0; y < rows && created < pieceCount; y++)
        {
            for (int x = 0; x < cols && created < pieceCount; x++)
            {
                // Tạo rect cắt từ texture
                Rect rect = new Rect(x * texPieceW, (rows - y - 1) * texPieceH, texPieceW, texPieceH);
                
                // Tạo sprite từ rect
                Sprite pieceSprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), ghostSprite.pixelsPerUnit);
                
                // Vị trí trong thế giới
                Vector3 pos = b.min + new Vector3(
                    (x + 0.5f) * worldPieceW,
                    (y + 0.5f) * worldPieceH,
                    transform.position.z
                );
                
                // Tạo GameObject
                GameObject go = new GameObject("Piece_" + created);
                go.transform.position = pos;
                go.transform.parent = transform.parent;
                
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pieceSprite;
                sr.sortingOrder = normalLogo.sortingOrder;
                sr.color = normalLogo.color;
                
                pieces.Add(go);
                created++;
            }
        }
    }

    IEnumerator MoveAndScale(Transform tform, Vector3 from, Vector3 to, float dur, bool randomRotate, float delay = 0f)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float time = 0f;
        Quaternion startRot = tform.rotation;
        Quaternion endRot = randomRotate ? Random.rotation : startRot;
        Vector3 startScale = tform.localScale;
        Vector3 endScale = randomRotate ? Vector3.one * 0.3f : Vector3.one;

        while (time < dur)
        {
            time += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, time / dur);
            tform.position = Vector3.Lerp(from, to, f);
            tform.rotation = Quaternion.Slerp(startRot, endRot, f);
            tform.localScale = Vector3.Lerp(startScale, endScale, f);
            yield return null;
        }
        tform.position = to;
        tform.rotation = endRot;
        tform.localScale = endScale;
    }
}
