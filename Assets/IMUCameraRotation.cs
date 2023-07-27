using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

public class IMUCameraRotation : MonoBehaviour
{

    #region "Native Plugin"
#if UNITY_IOS && !UNITY_EDITOR
    private const string PLUGIN_NAME = "__Internal";
#else
    private const string PLUGIN_NAME = "bno055";
#endif

    [DllImport(PLUGIN_NAME)]
    private static extern bool setMode(string device_path, int device_address, byte register_address, byte mode);

    [DllImport(PLUGIN_NAME)]
    private static extern IntPtr getQuaternion(string device_path, int device_address);

    public static Quaternion GetQuaternion(string devicePath, int deviceAddress)
    {
        IntPtr quaternionPtr = getQuaternion(devicePath, deviceAddress);
        float[] quaternionValues = new float[4];

        // Copy the data from the native pointer to the managed array
        Marshal.Copy(quaternionPtr, quaternionValues, 0, 4);

        return new Quaternion(quaternionValues[2], -quaternionValues[3], quaternionValues[1], quaternionValues[0]);
    }

    private string device_path = "/dev/i2c-5";
    private int device_address = 0x28;
    private byte register_address = 0x3d;
    private byte mode = 0x08;

    #endregion

    private Vector3 initialImuRotation = Vector3.zero;
    private Vector3 camStartEuler;

    private Vector3 correctedImuRotation;


    [SerializeField] float accelerationThreshold = 15f;
    [SerializeField] float accelerationAmount = 0.01f;
    [SerializeField] float updateRate = 120f;

    [SerializeField] TMPro.TextMeshProUGUI accelerationThresholdText;
    [SerializeField] TMPro.TextMeshProUGUI accelerationAmountText;
    [SerializeField] TMPro.TextMeshProUGUI averageFramesText;

    //PostProcessingController postProcessingController;

    //Previous frame rotation
    private Quaternion lastRotation;

    //Averaging stuff   
    [SerializeField] int averageFrames = 1; 
    private int count;
    private Vector3 averagedImuAngularVelocity;
    Queue<Vector3> averagedImuAngularVelocityQueue = new Queue<Vector3>();

    private Vector3 currentAccelerationVector;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        // Call the native plugin function and convert the returned pointer to a string
        bool result = setMode(device_path, device_address, register_address, mode);
        if (result)
        {
            Debug.Log("setMode succeeded!");
        }
        else
        {
            Debug.LogError("setMode failed!");
        }
#endif
        lastRotation = Quaternion.identity;

        //StartCoroutine(UpdateEulerCoroutine());
    }

    // Method to update Euler angles
    IEnumerator UpdateEulerCoroutine()
    {
        Quaternion imuRotation = IMUCameraRotation.GetQuaternion(device_path, device_address);
        Quaternion remappedImuRotation = new Quaternion(imuRotation.x, imuRotation.y, -imuRotation.z, imuRotation.w);

        if (initialImuRotation == Vector3.zero)
        {
            initialImuRotation = remappedImuRotation.eulerAngles;
            Debug.Log(initialImuRotation);
            yield return new WaitForSeconds(1f / updateRate);
            StartCoroutine(UpdateEulerCoroutine());
            yield break;
        }

        // Update the rotation of the cylinder based on the received qc
        // transform.rotation = qc;
        correctedImuRotation = remappedImuRotation.eulerAngles - initialImuRotation;

        //Get average Angular Velocity of IMU rotation
        Vector3 imuAngularVelocity = GetAngularVelocityVector(correctedImuRotation);
        averagedImuAngularVelocity = GetCurrentAccelerationVector(imuAngularVelocity);

        transform.localRotation = Quaternion.Euler(correctedImuRotation + camStartEuler);

        if (imuAngularVelocity.magnitude > accelerationThreshold)
        {
            transform.Rotate(averagedImuAngularVelocity * accelerationAmount);
            Debug.Log("Acceleration Vector: " + averagedImuAngularVelocity * accelerationAmount);
        }
        else
        {

        }

        yield return new WaitForSeconds(1f / updateRate);

        StartCoroutine(UpdateEulerCoroutine());
    }

    void UpdateEuler()
    {
        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        Quaternion q = IMUCameraRotation.GetQuaternion(device_path, device_address);
        Quaternion qc = new Quaternion(q.x, q.y, -q.z, q.w);

        if (initialImuRotation == Vector3.zero)
        {
            initialImuRotation = qc.eulerAngles;
            Debug.Log(initialImuRotation);
        }

        // Update the rotation of the cylinder based on the received qc
        // transform.rotation = qc;
        correctedImuRotation = qc.eulerAngles - initialImuRotation;

        //Apply rotation to gameobject
        transform.localRotation = Quaternion.Euler(camStartEuler + correctedImuRotation);

        timer.Stop();

        Debug.Log("Invoke " + timer.Elapsed);
    }

    Vector3 GetAngularVelocityVector(Vector3 rotation)
    {
        var deltaRot = Quaternion.Euler(rotation) * Quaternion.Inverse(lastRotation);
        var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));

        lastRotation = Quaternion.Euler(rotation);

        return eulerRot / Time.deltaTime;
    }

    Vector3 GetCurrentAccelerationVector(Vector3 vector)
    {
        //if (currentAccelerationVector == Vector3.zero)
        //{
        //    currentAccelerationVector = vector;
        //    Debug.Log(currentAccelerationVector);
        //}
        //return currentAccelerationVector;
   
        averagedImuAngularVelocityQueue.Enqueue(vector);


        if (averagedImuAngularVelocityQueue.Count >= averageFrames)
        {
            var vectorSum = Vector3.zero;

            foreach(Vector3 v in averagedImuAngularVelocityQueue)
            {
                vectorSum += v;
            }

            currentAccelerationVector = vectorSum / averagedImuAngularVelocityQueue.Count;
            averagedImuAngularVelocityQueue.Clear();
        }

        return currentAccelerationVector;
    }

    private void LateUpdate()
    {
        Quaternion imuRotation = IMUCameraRotation.GetQuaternion(device_path, device_address);
        Quaternion remappedImuRotation = new Quaternion(imuRotation.x, imuRotation.y, -imuRotation.z, imuRotation.w);

        if (initialImuRotation == Vector3.zero)
        {
            initialImuRotation = remappedImuRotation.eulerAngles;
            Debug.Log(initialImuRotation);
        }

        // Update the rotation of the cylinder based on the received qc
        // transform.rotation = qc;
        correctedImuRotation = remappedImuRotation.eulerAngles - initialImuRotation;

        //Get average Angular Velocity of IMU rotation
        Vector3 imuAngularVelocity = GetAngularVelocityVector(correctedImuRotation);


        //if (imuAngularVelocity.magnitude > accelerationThreshold)
        //{
            transform.localRotation = Quaternion.Euler(correctedImuRotation + camStartEuler + 
                (GetCurrentAccelerationVector(imuAngularVelocity) * accelerationAmount));

        //}
        //else
        //{
            //currentAccelerationVector = Vector3.zero;
            //transform.localRotation = Quaternion.Euler(correctedImuRotation + camStartEuler);
        //}


        //transform.Rotate(currentAccelerationVector);

    }

    public void SetAccelerationThreshold(float value)
    {
        accelerationThreshold = value;
        accelerationThresholdText.text = "Threshold " + value.ToString();
    }

    public void SetAccelerationAmount(float value)
    {
        accelerationAmount = value;
        accelerationAmountText.text = "Amount " + value.ToString();
    }

    public void SetAverageFrames(float value)
    {
        averageFrames = (int)value;
        averageFramesText.text = "Frames " + value.ToString();
    }
}