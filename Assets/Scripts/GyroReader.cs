using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GyroReader : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private const string PLUGIN_NAME = "bno055";

    [DllImport(PLUGIN_NAME)]
    private static extern bool setMode(string device_path, int device_address, byte register_address, byte mode);

    [DllImport(PLUGIN_NAME)]
    private static extern IntPtr getQuaternion(string device_path, int device_address);

    public static Quaternion GetQuaternion()
    {
        Quaternion q = GetQuaternion(device_path, device_address);
        return new Quaternion(q.x, -q.y, -q.z, q.w);
    }

    public static Quaternion GetQuaternion(string devicePath, int deviceAddress)
    {
        IntPtr quaternionPtr = getQuaternion(devicePath, deviceAddress);
        float[] quaternionValues = new float[4];

        // Copy the data from the native pointer to the managed array
        Marshal.Copy(quaternionPtr, quaternionValues, 0, 4);

        return new Quaternion(quaternionValues[2], quaternionValues[3], quaternionValues[1], quaternionValues[0]);
    }

    private void Awake()
    {
        // Call the native plugin function and convert the returned pointer to a string
        bool result = setMode(device_path, device_address, register_address, mode);
        if (result)
        {
            GyroReader.IsReady = true;
            Debug.Log("setMode succeeded!");
        }
        else
        {
            Debug.LogError("setMode failed!");
        };
    }
   
#endif
#if UNITY_EDITOR
    public static Quaternion GetQuaternion()
    {
        return Quaternion.identity;
    }
#endif

    [SerializeField] static string device_path = "/dev/i2c-5";
    [SerializeField] static int device_address = 0x28;
    [SerializeField] static byte register_address = 0x3d;
    [SerializeField] static byte mode = 0x08;

    public static bool IsReady = false;
}