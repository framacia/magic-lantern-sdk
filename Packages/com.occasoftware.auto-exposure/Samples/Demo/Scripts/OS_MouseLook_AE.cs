using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OccaSoftware.Exposure.Demo
{
    /// <summary>
    /// A simple mouselook script.
    /// </summary>
    public class OS_MouseLook_AE : MonoBehaviour
    {
        Vector2 rotation = Vector2.zero;
        public float rotationSpeed = 3;
        public float moveSpeed = 10;

        void Update()
        {
            if (Input.GetMouseButton(1))
            {
                rotation.y += Input.GetAxis("Mouse X");
                rotation.x += -Input.GetAxis("Mouse Y");
                transform.eulerAngles = rotation * rotationSpeed;
            }

            if (Input.GetKey(KeyCode.E))
                transform.Translate(transform.up * moveSpeed * Time.deltaTime, Space.World);

            if (Input.GetKey(KeyCode.Q))
                transform.Translate(-transform.up * moveSpeed * Time.deltaTime, Space.World);

            transform.Translate(
                transform.forward * Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime,
                Space.World
            );
            transform.Translate(
                transform.right * Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime,
                Space.World
            );
        }
    }
}
