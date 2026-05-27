using UnityEngine;

public class ArrowFloat : MonoBehaviour
{
    [Header("Di chuyển ngang trái phải")]
    public float moveDistance = 0.3f;  // Khoảng cách di chuyển ngang
    public float moveSpeed = 2.5f;      // Tốc độ di chuyển

    [Header("Phóng to thu nhỏ nhẹ")]
    public bool useScalePulse = true;
    public float scaleAmount = 0.05f;
    public float scaleSpeed = 2.5f;

    private Vector3 startPosition;
    private Vector3 startScale;

    void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    void Update()
    {
        // Di chuyển ngang trái phải
        float newX = startPosition.x + Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = new Vector3(newX, startPosition.y, startPosition.z);

        // Phồng nhẹ cho nổi bật
        if (useScalePulse)
        {
            float scaleOffset = Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
            transform.localScale = startScale + new Vector3(scaleOffset, scaleOffset, 0);
        }
    }
}