using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraMoveDebug : MonoBehaviour
{
    [SerializeField] float speed = 1f;

    private void Start()
    {
        Invoke("DelayedStart", 0.5f);
    }

    private void DelayedStart()
    {
        //Is it really necessary setting this??? Find a good place to add it 
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = 61;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float xAxisValue = Input.GetAxis("HorizontalArrow");
        float zAxisValue = Input.GetAxis("VerticalArrow");

        if (gameObject != null)
        {
            //Rotation
            gameObject.transform.Rotate(-zAxisValue * speed * Time.deltaTime, 
                xAxisValue * speed * Time.deltaTime, 0); ;

            //Force Z rotation to 0
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

            //Translation
            Vector3 movementVector = (gameObject.transform.forward * Input.GetAxis("Vertical") * 10) + (gameObject.transform.right * Input.GetAxis("Horizontal") * 10);

            gameObject.transform.position += new Vector3(movementVector.x, 0, movementVector.z) * Time.deltaTime;
        }
    }
}
