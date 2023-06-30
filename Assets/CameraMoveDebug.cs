using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveDebug : MonoBehaviour
{
    Camera cam;
    [SerializeField] float speed = 1f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal");
        float zAxisValue = Input.GetAxis("Vertical");
        if (cam != null)
        {
            cam.transform.Rotate(-zAxisValue * speed, xAxisValue * speed, 0);

            //Force Z rotation to 0
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        }
    }
}
