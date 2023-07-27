using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

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

    private Vector3 rosStartEuler = Vector3.zero;
    private Vector3 camStartEuler;

    private Vector3 eulerDiff;


    [SerializeField] float accelerationThreshold = 15f;
    [SerializeField] float accelerationAmount = 0.01f;
    [SerializeField] float updateRate = 120f;

    [SerializeField] TMPro.TextMeshProUGUI accelerationThresholdText;
    [SerializeField] TMPro.TextMeshProUGUI accelerationAmountText;

    //PostProcessingController postProcessingController;

    //Previous frame rotation
    private Quaternion lastRotation;

    //Averaging stuff   
    int averageFrames = 5; 
    private int count;
    private Vector3 averagedAngularVelocity;

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

        StartCoroutine(UpdateEulerCoroutine());
    }

    // Method to update Euler angles
    IEnumerator UpdateEulerCoroutine()
    {
        Quaternion q = IMUCameraRotation.GetQuaternion(device_path, device_address);
        Quaternion qc = new Quaternion(q.x, q.y, -q.z, q.w);

        if (rosStartEuler == Vector3.zero)
        {
            rosStartEuler = qc.eulerAngles;
            Debug.Log(rosStartEuler);
            yield return new WaitForSeconds(1f / updateRate);
            StartCoroutine(UpdateEulerCoroutine());
            yield break;
        }

        // Update the rotation of the cylinder based on the received qc
        // transform.rotation = qc;
        eulerDiff = qc.eulerAngles - rosStartEuler;

        //Get average Angular Velocity of IMU rotation
        Vector3 eulerDiffVelocity = GetAngularVelocityVector(eulerDiff);
        averagedAngularVelocity = GetAverageVector(eulerDiffVelocity);

        ////If angular velocity bigger than threshold, apply blur effect
        //if (eulerDiffVelocity.magnitude > accelerationThreshold)
        //{
        //    //postProcessingController?.ChangeDoFFocalLength(eulerDiffVelocity.magnitude * 10);
        //}

        Vector3 accelerationVector = Vector3.zero;

        //If angular velocity bigger than threshold, apply acceleration to rotation vector
        if (averagedAngularVelocity.magnitude > accelerationThreshold)
        {
            //accelerationVector = eulerDiff * accelerationAmount;
            //Debug.Log(accelerationVector);
            Debug.Log(averagedAngularVelocity.magnitude);
        }

        //Apply rotation to gameobject
        transform.localRotation = Quaternion.Euler(eulerDiff + accelerationVector + camStartEuler);

        yield return new WaitForSeconds(1f / updateRate);

        StartCoroutine(UpdateEulerCoroutine());
    }

    void UpdateEuler()
    {
        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        Quaternion q = IMUCameraRotation.GetQuaternion(device_path, device_address);
        Quaternion qc = new Quaternion(q.x, q.y, -q.z, q.w);

        if (rosStartEuler == Vector3.zero)
        {
            rosStartEuler = qc.eulerAngles;
            Debug.Log(rosStartEuler);
        }

        // Update the rotation of the cylinder based on the received qc
        // transform.rotation = qc;
        eulerDiff = qc.eulerAngles - rosStartEuler;

        //Apply rotation to gameobject
        transform.localRotation = Quaternion.Euler(camStartEuler + eulerDiff);

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

    Vector3 GetAverageVector(Vector3 vector)
    {
        count++;

        if (count > averageFrames)
        {
            averagedAngularVelocity = averagedAngularVelocity + (vector - averagedAngularVelocity) / (averageFrames + 1);
            return averagedAngularVelocity;
        }
        else
        {
            //NOTE: The MovingAverage will not have a value until at least "MovingAverageLength" values are known (10 values per your requirement)
            averagedAngularVelocity += vector;

            //This will calculate ONLY the very first value of the MovingAverage,
            if (count == averageFrames)
            {
                averagedAngularVelocity = averagedAngularVelocity / count;

            }
            return averagedAngularVelocity;
        }
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
}