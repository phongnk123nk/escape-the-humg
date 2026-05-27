using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driver : MonoBehaviour
{
    [SerializeField] float moveSpeed = 0.02f;
    [SerializeField] float steerSpeed = 0.02f;
    [SerializeField] float boostSpeed = 30f;
    [SerializeField] float normalSpeed = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // trả về các giá trị từ -1 đến 1 và sẽ 0 khi không có tác động
        float changeSteer = Input.GetAxis("Horizontal") *steerSpeed * Time.deltaTime;
        float changeMove = Input.GetAxis("Vertical") *moveSpeed * Time.deltaTime;
        transform.Translate(0, changeMove, 0);
        transform.Rotate(0, 0, -changeSteer);    
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        moveSpeed = normalSpeed;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag =="Boost")
        {
            moveSpeed = boostSpeed;
        }
    }
}
