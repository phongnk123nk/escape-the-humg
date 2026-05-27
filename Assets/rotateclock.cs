using UnityEngine;

public class RotateClock : MonoBehaviour
{
    public float rotationSpeed = -50f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Giảm độ đậm và làm màu tối hơn
        sr.color = new Color(0.6f, 0.6f, 0.7f, 0.55f);
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}