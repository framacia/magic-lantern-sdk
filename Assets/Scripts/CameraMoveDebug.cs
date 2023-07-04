using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraMoveDebug : MonoBehaviour
{
    Camera cam;
    [SerializeField] float speed = 1f;
    [SerializeField] TextMeshProUGUI fpsCounterText;

    private void Start()
    {


        cam = GetComponent<Camera>();

        Invoke("DelayedStart", 0.5f);
    }

    private void DelayedStart()
    {
        //Is it really necessary setting this??? Find a good place to add it 
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = 61;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float xAxisValue = Input.GetAxis("HorizontalArrow");
        float zAxisValue = Input.GetAxis("VerticalArrow");

        if (cam != null)
        {
            //Rotation
            cam.transform.Rotate(-zAxisValue * speed, xAxisValue * speed, 0);

            //Force Z rotation to 0
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

            //Translation
            Vector3 movementVector = (cam.transform.forward * Input.GetAxis("Vertical") * 10) + (cam.transform.right * Input.GetAxis("Horizontal") * 10);

            cam.transform.position += new Vector3(movementVector.x, 0, movementVector.z) * Time.deltaTime;
        }

    }

    private void Update()
    {
        if (fpsCounterText != null)
        {
            fpsCounterText.text = (1.0f / Time.deltaTime).ToString("F1");
        }
    }
}
