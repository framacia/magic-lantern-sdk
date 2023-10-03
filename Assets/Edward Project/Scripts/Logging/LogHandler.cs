//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LogHandler : MonoBehaviour
//{
//    public static LogHandler Instance;
//    private Logger log;

//    public void Start()
//    {
//        DontDestroyOnLoad(this);
//        Instance = this;
//        log = new Logger();
//    }

//    public void LogMessage(string msg)
//    {
//        log.LogMessage(msg);
//    }
//    public void OnDestroy()
//    {
//        log.Close();
//        DriveFileUploader.Instance.PoolUploadRequest(Logger.LastValidPath);
//    }
//}
