//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ReplayController : MonoBehaviour
//{
//    //------ Drive link
//    public DriveFileDownloader logDownloader;

//    //------ Skeletons
//    public Dictionary<string, BoneController> currentSkeletons = new Dictionary<string, BoneController>();
//    public GameObject replaySkeletonPrefab;

//    //------ Lantern
//    public Transform replayLantern;

//    //------ Simulation variables
//    bool isSimulating = false;
//    WaitForSeconds simWait;
//    string rawData;

//    public string fileNameWithExtension;

//    // Start is called before the first frame update
//    void Start()
//    {
//        logDownloader.onFileDownload += OnDownload;
//        logDownloader.RequestDownloadFile(fileNameWithExtension);
//        simWait = new WaitForSeconds(DataLogger.UpdateTime);
//    }

//    IEnumerator Simulate(){

//        do{
//            string[] splitData = rawData.Split("\n", 2);
//            rawData = splitData[1];
//            Debug.Log(splitData[0]);
//            string[] instruction = splitData[0].Split("_", 2);
//            switch(instruction[0]){
//                case "posRot":
//                    yield return simWait;
//                    UpdateCameraPosition(instruction[1]);
//                    break;
//                case "skeleton":
//                    UpdateAmountOfSkeletons(instruction[1]);
//                    break;
//                case "skPosRot":
//                    UpdatePresentSkeletons(instruction[1]);
//                    break;
//                default:
//                    yield return null;
//                    break;
//            }
//        }while(rawData.Length > 0);
//    }

//    void OnDownload(string data){
//        isSimulating = true;
//        rawData = string.Copy(data);
//        StartCoroutine(Simulate());
//    }

//    void UpdateCameraPosition(string data){
//        SetPosRotFromString(data, replayLantern);
//    }

//    void UpdateAmountOfSkeletons(string data){
//        string[] splitData = data.Split("_", System.StringSplitOptions.None);
//        int id = int.Parse(splitData[1]);
//        if(id == 1){
//            BoneController skeleton = Instantiate(replaySkeletonPrefab, Vector3.zero, Quaternion.identity, null).GetComponent<BoneController>();
//            skeleton.InitializeSkeletonJoints();
//            currentSkeletons.Add(splitData[0], skeleton);
//        }
//        else{
//            Destroy(currentSkeletons[splitData[0]].gameObject);
//            currentSkeletons.Remove(splitData[0]);
//        }
//    }

//    void UpdatePresentSkeletons(string data){
//        string[] splitData = data.Split("_", 2);
//        string id = splitData[0];   


//        Vector3[] positions = new Vector3[BoneController.k_NumSkeletonJoints];
//        Vector3[] rotations = new Vector3[BoneController.k_NumSkeletonJoints];

//        splitData = splitData[1].Split("#", 2);
//        SetPosRotFromString(splitData[0], currentSkeletons[id].transform);
//        Debug.Log(currentSkeletons[id].transform.position);
//        for(int i = 0; i < BoneController.k_NumSkeletonJoints; i++){
//            splitData = splitData[1].Split("#", 2);
//            SetPosRotFromString(splitData[0], currentSkeletons[id].m_BoneMapping[i]);
//        }
//    }

//    void SetPosRotFromString(string data, Transform target, bool localSpace = false){
//        string[] splitData = data.Split("_", System.StringSplitOptions.None);
//        Vector3 pos = new Vector3(
//            float.Parse(splitData[0]),
//            float.Parse(splitData[1]),
//            float.Parse(splitData[2])
//        );
//        Vector3 rot = new Vector3(
//            float.Parse(splitData[3]),
//            float.Parse(splitData[4]),
//            float.Parse(splitData[5])
//        );
//        if(localSpace)
//            target.localPosition = pos;
//        else
//            target.position = pos;
//        target.rotation = Quaternion.Euler(rot);
//    }
//}
