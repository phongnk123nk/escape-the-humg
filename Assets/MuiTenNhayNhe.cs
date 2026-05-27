using UnityEngine;

public class MuiTenNhayNhe : MonoBehaviour
{
    public float doCaoNhay = 0.15f;
    public float tocDoNhay = 2f;

    private Vector3 viTriBanDau;

    void Start()
    {
        viTriBanDau = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * tocDoNhay) * doCaoNhay;
        transform.position = viTriBanDau + new Vector3(0f, y, 0f);
    }
}