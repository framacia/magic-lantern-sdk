using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.MagicLantern;

public class AnchorPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "anchor_info";
    public float publishMessageFrequency = 1.0f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    
    void Start()
    {

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<AnchorInformationMsg>(topicName);
       
    }

    private void Update()
    {   
        
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            GameObject[] anchorObjects = GameObject.FindGameObjectsWithTag("anchor");
            foreach (GameObject anchorObject in anchorObjects)
            {
            
                AnchorDefinition anchorDefinition = anchorObject.GetComponent<AnchorDefinition>();

                // Create AnchorInfo object and populate the data
                AnchorInformationMsg anchorInfo = new AnchorInformationMsg();
                anchorInfo.id = int.Parse(anchorDefinition.anchorId);
                anchorInfo.location[0] = anchorObject.transform.position.x;
                anchorInfo.location[1] = anchorObject.transform.position.y;
                anchorInfo.location[2] = anchorObject.transform.position.z;
                anchorInfo.location[3] = anchorObject.transform.rotation.eulerAngles.x;
                anchorInfo.location[4] = anchorObject.transform.rotation.eulerAngles.y;
                anchorInfo.location[5] = anchorObject.transform.rotation.eulerAngles.z;
                anchorInfo.volume_size = anchorDefinition.volumeSize;
                anchorInfo.filter_proximity = anchorDefinition.filterProximity;

                // Finally send the message to server_endpoint.py running in ROS
                ros.Publish(topicName, anchorInfo);

                //PublishAnchorInfo(anchorInfo);
                timeElapsed = 0;
            }
        }
    }

    private void PublishAnchorInfo(AnchorInformationMsg anchorInfo)
    {
        // Publish the anchor information
        Debug.Log("Anchor ID: " + anchorInfo.id);
        Debug.Log("Location:s " + string.Join(", ", anchorInfo.location));
        Debug.Log("Volume Size: " + anchorInfo.volume_size);
        Debug.Log("Filter Proximity: " + anchorInfo.filter_proximity);
    }
}

