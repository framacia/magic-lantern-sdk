using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockIMU : MonoBehaviour
{
    [SerializeField] float RotationSpeed = 20;
    [SerializeField] bool useMouse = false;
    [SerializeField] bool useArrows = true;

    // Update is called once per frame
    void LateUpdate()
    {
        //If Debug, get mouse or arrows
        if (useMouse)
            MouseMovement();

        if (useArrows)
            ArrowMovement();
    }

    void MouseMovement()
    {
        transform.Rotate((Input.GetAxis("Mouse Y") * RotationSpeed * Time.deltaTime),
    (-Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime), 0, Space.World);
    }

    void ArrowMovement()
    {
        //    gameObject.transform.Rotate(-Input.GetAxis("VerticalArrow") * RotationSpeed * Time.deltaTime,
        //Input.GetAxis("HorizontalArrow") * RotationSpeed * Time.deltaTime, 0);


        transform.eulerAngles += new Vector3(
            -Input.GetAxis("VerticalArrow") * RotationSpeed * Time.deltaTime,
            Input.GetAxis("HorizontalArrow") * RotationSpeed * Time.deltaTime,
            0);
    }

}
