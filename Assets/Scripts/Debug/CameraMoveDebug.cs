using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;

public class CameraMoveDebug : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] bool isMouse = false;
    [SerializeField] TMP_Text vectorText;

    private void Start()
    {
        Invoke("DelayedStart", 0.5f);


    }

    private void DelayedStart()
    {
#if UNITY_ANDROID
        //Is it really necessary setting this??? Find a good place to add it 
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = 61;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        float xAxisValue;
        float zAxisValue;

        if (!isMouse)
        {
            xAxisValue = Input.GetAxis("HorizontalArrow");
            zAxisValue = Input.GetAxis("VerticalArrow");
        }
        else
        {
            xAxisValue = Input.GetAxis("Mouse X");
            zAxisValue = Input.GetAxis("Mouse Y");
        }

        if (gameObject != null)
        {
            /*gameObject.transform.Rotate(-zAxisValue * speed * Time.deltaTime,
                xAxisValue * speed * Time.deltaTime, 0);

            //Force Z rotation to 0
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

            //Translation
            Vector3 movementVector = (gameObject.transform.forward * Input.GetAxis("Vertical") * 10) +
                (gameObject.transform.right * Input.GetAxis("Horizontal") * 10);*/

            gameObject.transform.localPosition += new Vector3(Input.GetAxis("HorizontalArrow"), Input.GetAxis("VerticalArrow"), 0) * Time.deltaTime;
        }

        //Close app when user presses Android Home button
        if (Input.GetKey(KeyCode.Escape))
        { 
            Application.Quit(); 
        }

        if (vectorText)
        {
            vectorText.text = transform.position.ToString();
        }
    }
}
