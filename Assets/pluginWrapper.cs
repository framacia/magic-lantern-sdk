using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HelloWorldScript : MonoBehaviour
{


#if UNITY_IOS && !UNITY_EDITOR
    private const string PLUGIN_NAME = "__Internal";
#else
    private const string PLUGIN_NAME = "testimu";
#endif

    [DllImport(PLUGIN_NAME)]
    private static extern bool setMode(string device_path, int device_address, byte register_address, byte mode);

    [DllImport(PLUGIN_NAME)]
    private static extern IntPtr getQuaternion(string device_path, int device_address);

    public static Quaternion GetQuaternion(string devicePath, int deviceAddress) {
        IntPtr quaternionPtr = getQuaternion(devicePath, deviceAddress);
        float[] quaternionValues = new float[4];

        // Copy the data from the native pointer to the managed array
        Marshal.Copy(quaternionPtr, quaternionValues, 0, 4);
       
        return new Quaternion(quaternionValues[2], -quaternionValues[3], quaternionValues[1], quaternionValues[0]);
    }


    private string device_path = "/dev/i2c-5";
    private int device_address = 0x28;
    private byte register_address = 0x3d;
    private byte mode =  0x08;

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
        }

        float updateFrequency = 1f / 60f; // 30 Hz
        InvokeRepeating("UpdateEuler", 0f, updateFrequency);
    }

    // Method to update Euler angles at 30 Hz
    private void UpdateEuler()
    {
        Quaternion quaternion = HelloWorldScript.GetQuaternion(device_path, device_address);
        Debug.Log($"Quaternion: x = {quaternion.x}, y = {quaternion.y}, z = {quaternion.z}, w = {quaternion.w}");

        // Update the rotation of the cylinder based on the received quaternion
        transform.rotation = quaternion;
    }

}