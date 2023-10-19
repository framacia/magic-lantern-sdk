using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.UI;

public class IMUCameraRotation : MonoBehaviour
{
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
    Queue<Vector3> averagedImuAngularVelocityQueue = new Queue<Vector3>();

    private Vector3 averagedAccelerationVector;

    private void Start()
    {
        lastRotation = Quaternion.identity;
        camStartEuler = transform.localRotation.eulerAngles;
    }

    //If this runs on LateUpdate, transform is not updated through network, check for solutions
    private void Update()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        //Quaternion imuRotation = BNO055_Arduino.Instance.GetQuaternion();
        Quaternion imuRotation = BNO055Sensor.Instance.GetQuaternion();
        Quaternion remappedImuRotation = new Quaternion(imuRotation.y, imuRotation.z, imuRotation.x, imuRotation.w);
        //remappedImuRotation = Quaternion.AngleAxis(-90, Vector3.right) * remappedImuRotation;

        if (initialImuRotation == Vector3.zero)
        {
            initialImuRotation = remappedImuRotation.eulerAngles;
        }

        // Update the rotation of the cylinder based on the received qc
        correctedImuRotation = remappedImuRotation.eulerAngles - initialImuRotation;

        transform.localEulerAngles = correctedImuRotation + camStartEuler;
#else
        //correctedImuRotation = Vector3.zero;
#endif

        //Get average Angular Velocity of IMU rotation
        //Vector3 imuAngularVelocity = GetAngularVelocityVector(correctedImuRotation);
        //Vector3 accelerationVector = imuAngularVelocity * accelerationAmount;


    }

    Vector3 GetAngularVelocityVector(Vector3 rotation)
    {
        var deltaRot = Quaternion.Euler(rotation) * Quaternion.Inverse(lastRotation);
        var eulerRot = new Vector3(
            Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), 
            Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), 
            Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));

        lastRotation = Quaternion.Euler(rotation);

        return eulerRot / Time.deltaTime;
    }

    Vector3 GetAveragedAccelerationVector(Vector3 vector)
    {
        averagedImuAngularVelocityQueue.Enqueue(vector);

        if (averagedImuAngularVelocityQueue.Count >= averageFrames)
        {
            var vectorSum = Vector3.zero;

            foreach (Vector3 v in averagedImuAngularVelocityQueue)
            {
                vectorSum += v;
            }

            averagedAccelerationVector = vectorSum / averagedImuAngularVelocityQueue.Count;
            averagedImuAngularVelocityQueue.Clear();
        }

        return averagedAccelerationVector;
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