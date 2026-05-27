using UnityEngine;
using UnityEngine.SceneManagement;

public class DevSkipPuzzle : MonoBehaviour
{
    [Header("Bật / tắt skip cho dev")]
    public bool batSkipDev = true;

    [Header("Phím để skip")]
    public KeyCode phimSkip = KeyCode.K;

    [Header("Tên scene sẽ chuyển tới sau khi skip")]
    public string tenSceneTiepTheo = "TenSceneTiepTheo";

    [Header("Có hiện nút skip trên màn hình không")]
    public bool hienNutSkip = true;

    private void Update()
    {
        if (!batSkipDev) return;

        if (Input.GetKeyDown(phimSkip))
        {
            SkipPuzzle();
        }
    }

    public void SkipPuzzle()
    {
        if (!batSkipDev) return;

        if (string.IsNullOrEmpty(tenSceneTiepTheo))
        {
            Debug.LogWarning("Chưa điền tên scene tiếp theo trong DevSkipPuzzle.");
            return;
        }

        Debug.Log("BỎ QUA: Bỏ qua puzzle, chuyển sang scene: " + tenSceneTiepTheo);
        SceneManager.LoadScene(tenSceneTiepTheo);
    }

    private void OnGUI()
    {
        if (!batSkipDev || !hienNutSkip) return;

        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 28;
        style.fontStyle = FontStyle.Bold;

        float width = 220f;
        float height = 70f;
        float x = Screen.width - width - 30f;
        float y = 30f;

        if (GUI.Button(new Rect(x, y, width, height), "BỎ QUA", style))
        {
            SkipPuzzle();
        }
    }
}
