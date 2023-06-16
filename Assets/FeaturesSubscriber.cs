using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using PointCloud2 = RosMessageTypes.MagicLantern.PointCloud2Msg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class FeaturesSubscriber : MonoBehaviour
{
    public GameObject particlePrefab;  // Prefab for the particle object
    public float particleSize = 0.1f;  // Size of each particle
    public float updateInterval = 0.1f;  // Interval at which to update the particle system

    private ParticleSystem particleSystem;  // Reference to the particle system component
    private float lastUpdateTime;  // Time of the last update
    private bool isUpdating;  // Flag to indicate if an update is in progress

    // ROS variables
    public string topic = "feature_point_cloud";
    private bool isPointCloudInitialized = false;
    private byte[] pointCloudData;
    
    void Start()
    {
        
        ROSConnection.GetOrCreateInstance().Subscribe<PointCloud2>(topic, OnReceivePointCloud);
        particleSystem = GetComponent<ParticleSystem>();

        // Create an initial set of particles from the point cloud
        GenerateParticlesFromPointCloud();
    }



    void GenerateParticlesFromPointCloud()
    {
       

        // Convert the point cloud data to a list of Vector3 positions
        if (!isPointCloudInitialized || pointCloudData == null)
        {
            return;
        }
        
        List<Vector3> positions = ConvertPointCloudToPositions(pointCloudData);

        // Instantiate a particle for each position
        foreach (Vector3 position in positions)
        {
            GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity);
            particle.transform.localScale = new Vector3(particleSize, particleSize, particleSize);
            particle.transform.SetParent(transform);  // Set the particle as a child of this script's GameObject
        }

    }

    void OnReceivePointCloud(PointCloud2 pointCloud)
    {
        
        pointCloudData = pointCloud.data;
        isPointCloudInitialized = true;
        GenerateParticlesFromPointCloud();
    }

    List<Vector3> ConvertPointCloudToPositions(byte[] pointCloudData)
    {
        // Assuming each point consists of x, y, and z coordinates (each float takes 4 bytes)
        int pointSize = 12;  // Size of each point (3 floats)
        int pointCount = pointCloudData.Length / pointSize;
        
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < pointCount; i++)
        {
            int startIndex = i * pointSize;

            // Extract x, y, and z values from the byte array
            float x = BitConverter.ToSingle(pointCloudData, startIndex);
            float y = BitConverter.ToSingle(pointCloudData, startIndex + 4);
            float z = BitConverter.ToSingle(pointCloudData, startIndex + 8);

            // Create a Vector3 position from the extracted values
            Vector3 position = new Vector3(-y, z, x);

            // Add the position to the list
            positions.Add(position);
        }

        return positions;
    }
}