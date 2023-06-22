using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using ErrorStatus = RosMessageTypes.MagicLantern.ErrorStatusMsg;

public class RosErrorFlagReader : MonoBehaviour
{
    public string errorFlagTopic = "error_status_topic";
    public static bool noError;
    public bool LogErrors;

    private void Start()
    {
        Debug.Log("[RosErrorFlagReader] Start");
        ROSConnection.GetOrCreateInstance().Subscribe<ErrorStatus>(errorFlagTopic, ErrorFlagCallback);
    }

    private void ErrorFlagCallback(ErrorStatus message)
    {

        // Debug.Log("[RosErrorFlagReader] Received");
        if (message.no_error)
        {
            noError = true;
        }
        else
        {
            if (LogErrors)
            {
                Debug.LogError("[ROS Error]");
            }
            noError = false;
        }
    }
}