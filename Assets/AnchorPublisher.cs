using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.MagicLantern;

public class AnchorPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "anchor_info";
    private float publishMessageFrequency = 1f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    
    void Start()
    {

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<AnchorInformationMsg>(topicName);
       
    }

    private void Update()
    {   
        float Frequency = publishMessageFrequency;
        timeElapsed += Time.deltaTime;

        if (timeElapsed > Frequency)
        {
            GameObject[] anchorObjects = GameObject.FindGameObjectsWithTag("anchor");
            foreach (GameObject anchorObject in anchorObjects)
            {
                Vector3 rosPos = new Vector3(anchorObject.transform.position.z, - anchorObject.transform.position.x, anchorObject.transform.position.y);
                AnchorDefinition anchorDefinition = anchorObject.GetComponent<AnchorDefinition>();

                // Create AnchorInfo object and populate the data
                AnchorInformationMsg anchorInfo = new AnchorInformationMsg();
                anchorInfo.id = int.Parse(anchorDefinition.anchorId);
                anchorInfo.location[0] = anchorObject.transform.position.z;
                anchorInfo.location[1] = - anchorObject.transform.position.x;
                anchorInfo.location[2] = anchorObject.transform.position.y;
                anchorInfo.location[3] = 0;  //  the orientation is not correct, it is necessary to change it to ROS coordinate frame (right hand)
                anchorInfo.location[4] = 0;
                anchorInfo.location[5] = 0;
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

