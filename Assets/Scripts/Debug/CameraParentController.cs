using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;
using System.Diagnostics;

public class CameraParentController : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] bool isMouse = false;
    [SerializeField] TMP_Text vectorText;
    [SerializeField] TMP_Text cpuUsage;

    PerformanceCounter cpuCounter;
    PerformanceCounter ramCounter;


    #region Singleton
    public static CameraParentController Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject.transform.parent.gameObject);
        }
        else
        {
            Instance = this;
            //transform.SetParent(null);
        }
    }
    #endregion

    private void Start()
    {
        Invoke("DelayedStart", 0.5f);
        DontDestroyOnLoad(gameObject.transform.parent);

        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        StartCoroutine(DisplayCpuUsage());
    }

    public string getCurrentCpuUsage()
    {
        return cpuCounter.NextValue().ToString();
    }

    public string getAvailableRAM()
    {
        return ramCounter.NextValue() + "MB";
    }

    private IEnumerator DisplayCpuUsage()
    {
        vectorText.text = getCurrentCpuUsage();
        yield return new WaitForSeconds(1);
        StartCoroutine(DisplayCpuUsage());
    }

    private void DelayedStart()
    {
#if !UNITY_EDITOR
        //Is it really necessary setting this??? Find a good place to add it 
#if UNITY_ANDROID
        QualitySettings.vSyncCount = 0;
#endif
        Application.targetFrameRate = 61;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        //Close app when user presses Android Home button
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        //if (vectorText)
        //{
        //    vectorText.text = transform.position.ToString();
        //}


    }

    private void Move()
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
            gameObject.transform.Rotate(-zAxisValue * speed * Time.deltaTime,
                xAxisValue * speed * Time.deltaTime, 0);

            //Force Z rotation to 0
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

            //Translation
            Vector3 movementVector = (gameObject.transform.forward * Input.GetAxis("Vertical") * 10) +
                (gameObject.transform.right * Input.GetAxis("Horizontal") * 10);

            //gameObject.transform.localPosition += new Vector3(Input.GetAxis("HorizontalArrow"), Input.GetAxis("VerticalArrow"), 0) * Time.deltaTime;

            gameObject.transform.localPosition += new Vector3(movementVector.x, 0, movementVector.z) * Time.deltaTime;
        }
    }
}
