using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;

public class BNO055_Arduino : MonoBehaviour
{
    SerialPort serialPort;
    Thread readThread;
    bool isRunning = false;
    double qx, qy, qz, qw;
    double qxPrev, qyPrev, qzPrev, qwPrev;
    Vector3 camStartEuler;
    private Vector3 initialImuRotation = Vector3.zero;

    public static BNO055_Arduino Instance { get; private set; }

    public float maxThreshold = 0.1f;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Modify the port name as needed
        string portName = "/dev/ttyACM0";
        int baudRate = 115200;

        serialPort = new SerialPort(portName, baudRate);
        serialPort.WriteTimeout = 100;
        serialPort.ReadTimeout = 500;
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true; 

        try
        {
            serialPort.Open();
            isRunning = true;
            Debug.Log("isRunning is true");
        }
        catch (Exception e)
        {
            Debug.LogError("Error opening serial port: " + e.Message);
        }

        readThread = new Thread(ReadSerialData);
        readThread.Start();
        Debug.Log("Thread Started");

        camStartEuler = transform.localRotation.eulerAngles;
    }

    void Update()
    {
        // Update the rotation of the GameObject based on received quaternion data
        if (isRunning) {
            if (qxPrev != 0 && qyPrev != 0 && qzPrev != 0) {
                double differenceX = Math.Abs(qxPrev - qx);
                double differenceY = Math.Abs(qyPrev - qy);
                double differenceZ = Math.Abs(qzPrev - qz);
                if ((float)differenceX >= maxThreshold || (float)differenceY >= maxThreshold || (float)differenceZ >= maxThreshold) {
                    qx = qxPrev;
                    qy = qyPrev;
                    qz = qzPrev;
                    qw = qwPrev;
                }
                Quaternion remappedImuRotation = new Quaternion((float)qy, -(float)qz, -(float)qx, (float)qw);
                if (initialImuRotation == Vector3.zero) {
                    initialImuRotation = remappedImuRotation.eulerAngles;
                }

                Vector3 correctedImuRotation = remappedImuRotation.eulerAngles - initialImuRotation;

                transform.localEulerAngles = correctedImuRotation + camStartEuler;
            } 
            qxPrev = qx;
            qyPrev = qy;
            qzPrev = qz;
            qwPrev = qw;
        }
        
        
    }

    void ReadSerialData()
    {
        while (isRunning)
        {  
            try
            {
                string data = serialPort.ReadLine();
                Debug.Log("data: "+ data);
                string[] values = data.Split(',');

                if (values.Length == 4 && double.TryParse(values[0], out qx) && double.TryParse(values[1], out qy)
                    && double.TryParse(values[2], out qz) && double.TryParse(values[3], out qw))
                {
                    // Debug.Log($"Quaternion values: qx={qx:F4}, qy={qy:F4}, qz={qz:F4}, qw={qw:F4}");
                }
                else
                {
                    Debug.LogError("Failed to parse data: " + data);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading serial data: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }

        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
    }
}


