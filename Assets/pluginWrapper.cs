using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HelloWorldScript : MonoBehaviour
{
    public Vector3 rosStartEuler = Vector3.zero;
    public Vector3 camStartEuler;
    public Vector3 lastEuler;
    public float maxMag = 0;
    public float magthresh = 0.001f;

    public Image image;

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



    private void Start()
    {
        // Call the native plugin function and convert the returned pointer to a string
        bool result = setMode(device_path, device_address, register_address, mode);
        if (result)
        {
            Debug.Log("setMode succeeded!");
        }
        else
        {
            Debug.LogError("setMode failed!");
        };

        // float updateFrequency = 1f / 200f; // 30 Hz

        // StartCoroutine(UpdateEuler());
        // InvokeRepeating("UpdateEuler", 0f, updateFrequency);
    }

    void FixedUpdate()
    {
        Quaternion q = HelloWorldScript.GetQuaternion(device_path, device_address);
        Quaternion qc = new Quaternion(-q.x, q.y, q.z, q.w);
        // Debug.Log($"Quaternion: x = {qc.x}, y = {qc.y}, z = {qc.z}, w = {qc.w}");

        if (rosStartEuler == Vector3.zero)
        {
            rosStartEuler = qc.eulerAngles;
            lastEuler = rosStartEuler;
            Debug.Log(rosStartEuler);
            return;
        }

        // Update the rotation of the cylinder based on the received qc
        // transform.rotation = qc;
        Vector3 eulerDiff = qc.eulerAngles - rosStartEuler;
        float mag = (qc.eulerAngles - lastEuler).sqrMagnitude;
        if (mag > maxMag)
        {
            FPSCounter.vmag = maxMag;
            maxMag = mag;

        }
        // image.color = new Color(0, 0, 0, (mag > magthresh) ? 255 : 0);
        lastEuler = qc.eulerAngles;
        transform.localRotation = Quaternion.Euler(camStartEuler + eulerDiff);

    }
    // IEnumerator UpdateEuler()
    // {
    //     while (true)
    //     {
    //         Quaternion q = HelloWorldScript.GetQuaternion(device_path, device_address);
    //         Quaternion qc = new Quaternion(-q.x, q.y, q.z, q.w);
    //         // Debug.Log($"Quaternion: x = {qc.x}, y = {qc.y}, z = {qc.z}, w = {qc.w}");

    //         if (rosStartEuler == Vector3.zero)
    //         {
    //             rosStartEuler = qc.eulerAngles;
    //             lastEuler = rosStartEuler;
    //             Debug.Log(rosStartEuler);
    //             yield return new WaitForSeconds(1f / 1000f);
    //         }

    //         // Update the rotation of the cylinder based on the received qc
    //         // transform.rotation = qc;
    //         Vector3 eulerDiff = qc.eulerAngles - rosStartEuler;
    //         float mag = (qc.eulerAngles - lastEuler).sqrMagnitude;
    //         if (mag > maxMag)
    //         {
    //             FPSCounter.vmag = maxMag;
    //             maxMag = mag;

    //         }
    //         // image.color = new Color(0, 0, 0, (mag > magthresh) ? 255 : 0);
    //         lastEuler = qc.eulerAngles;
    //         transform.localRotation = Quaternion.Euler(camStartEuler + eulerDiff);

    //         yield return new WaitForSeconds(1f / 1000f);
    //     }
    // }

}