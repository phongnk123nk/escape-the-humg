
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    [SerializeField] GameObject followCamera;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = followCamera.transform.position + new Vector3(0, 0, -10);
    }
}
