using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class collision : MonoBehaviour
{
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     Debug.Log("touching");
    // }
    bool hasPackage = false;
    [SerializeField] float DestroyDelay = 0.5f;
    [SerializeField] Color32 PackageColor = new Color32(1, 1, 1, 1);
    [SerializeField] Color32 noPackageColor = new Color32(1, 1, 1, 1);
    SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (GetComponent<DeliveryOrderCarTrigger>() != null)
        {
            return;
        }

        if (collision.tag == "Package")
        {
            Debug.Log("Take the packet");
            hasPackage = true;
            spriteRenderer.color = PackageColor;
            Destroy(collision.gameObject, DestroyDelay);
        }
        if (collision.tag == "Location" && hasPackage)
        {
            Debug.Log("You are finished");
            hasPackage = false;
            spriteRenderer.color = noPackageColor;
        }
    }
}
