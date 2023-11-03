using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class RealSenseController : MonoBehaviour
{
    private const string PLUGIN_NAME = "camera_motion";

    #region Native Plugin Methods
    [DllImport(PLUGIN_NAME)]
    private static extern void cleanupCamera();

    [DllImport(PLUGIN_NAME)]
    private static extern void colorStreamConfig(int width, int height, int fps);
    [DllImport(PLUGIN_NAME)]
    private static extern void depthStreamConfig(int width, int height, int fps);

    [DllImport(PLUGIN_NAME)]
    private static extern void bagFileStreamConfig(string bagFileAddress);

    [DllImport(PLUGIN_NAME)]
    private static extern void initCamera();

    [DllImport(PLUGIN_NAME)]
    private static extern void initImu();

    [DllImport(PLUGIN_NAME)]
    private static extern void setParams(systemConfig config);

    [DllImport(PLUGIN_NAME)]
    private static extern void cropRectangleFeatures(int initX, int initY, int endX, int endY);

    [DllImport(PLUGIN_NAME)]
    private static extern void createORB(int nfeatures,
                                        float scaleFactor,
                                        int nlevels,
                                        int edgeThreshold,
                                        int firstLevel,
                                        int WTA_K,
                                        int scoreType,
                                        int patchSize,
                                        int fastThreshold
                                    );

    [DllImport(PLUGIN_NAME)]
    private static extern void createSIFT(int nfeatures = 0,
                                        int nOctaveLayers = 3,
                                        double contrastThreshold = 0.04,
                                        double edgeThreshold = 10,
                                        double sigma = 1.6,
                                        bool enable_precise_upscale = false
                                    );

    [DllImport(PLUGIN_NAME)]
    private static extern void firstIteration();


    [DllImport(PLUGIN_NAME)]
    private static extern void findFeatures();

    [DllImport(PLUGIN_NAME)]
    private static extern void getTranslationVector(float[] t_f_data);

    public static float[] RetrieveTranslationVector()
    {
        float[] t_f_data = new float[3];
        getTranslationVector(t_f_data);
        return t_f_data;
    }

    [DllImport(PLUGIN_NAME)]
    private static extern void getCameraRotation(float[] R_f_data);

    public static Quaternion RetrieveCameraQuaternions()
    {
        float[] R_f_data = new float[4];
        getCameraRotation(R_f_data);
        return new Quaternion(R_f_data[0], R_f_data[1], R_f_data[2], R_f_data[3]);
    }

    [DllImport(PLUGIN_NAME)]
    private static extern void getCameraOrientation(float[] cameraAngle);

    public static float[] RetrieveCameraOrientation()
    {
        float[] cameraAngle = new float[3];
        getCameraOrientation(cameraAngle);
        return cameraAngle;
    }

    [DllImport(PLUGIN_NAME)]
    private static extern void resetOdom();

    [DllImport(PLUGIN_NAME)]
    private static extern void addKeyframe();

    [DllImport(PLUGIN_NAME)]
    private static extern bool isLoop();

    [DllImport(PLUGIN_NAME)]
    private static extern IntPtr getJpegBuffer(out int bufferSize);

    public static byte[] GetJpegBuffer(out int bufferSize)
    {
        IntPtr bufferPtr = getJpegBuffer(out bufferSize);

        byte[] jpegBuffer = new byte[bufferSize];
        Marshal.Copy(bufferPtr, jpegBuffer, 0, bufferSize);

        Marshal.FreeCoTaskMem(bufferPtr);

        return jpegBuffer;
    }

    [DllImport(PLUGIN_NAME)]
    private static extern float GetDepthAtCenter();

    [DllImport(PLUGIN_NAME)]
    private static extern void setProjectorZone(int sectionX, int sectionY, int sectionWidth, int sectionHeight);

    [DllImport(PLUGIN_NAME)]
    private static extern void serializeKeyframeData(string fileName);

    [DllImport(PLUGIN_NAME)]
    private static extern void deserializeKeyframeData(string fileName);
    #endregion

    public struct systemConfig
    {
        public float ratioTresh;
        public float minDepth;
        public float maxDepth;
        public int min3DPoints;
        public float maxDistanceF2F;
        public int minFeaturesLoopClosure;
        public int framesUntilLoopClosure;
        public float noMovementThresh;
        public int framesNoMovement;
        public int maxGoodFeatures;
        public int minFeaturesFindObject;
    }

    #region Odometry Parameters
    [SerializeField] private bool localizationMode = false;
    [SerializeField] private int colorWidth = 640;
    [SerializeField] private int colorHeight = 480;
    [SerializeField] private int colorFPS = 30;
    [SerializeField] private int depthWidth = 640;
    [SerializeField] private int depthHeight = 480;
    [SerializeField] private int depthFPS = 30;
    [SerializeField] private float ratioTresh = 0.7f;
    [SerializeField] private float minDepth = 0.0f;
    [SerializeField] private float maxDepth = 6.0f;
    [SerializeField] private int min3DPoints = 15;
    [SerializeField] private float maxDistanceF2F = 0.05f;
    [SerializeField] private int minFeaturesLoopClosure = 200;
    [SerializeField] private int framesUntilLoopClosure = 200;
    [SerializeField] private float noMovementThresh = 0.0001f;
    [SerializeField] private int framesNoMovement = 50;
    [SerializeField] private int maxGoodFeatures = 500;
    [SerializeField] private int minFeaturesFindObject = 30;
    [SerializeField] private int xRectangle = 180;
    [SerializeField] private int yRectangle = 65;
    [SerializeField] private int widthRectangle = 325;
    [SerializeField] private int heightRectangle = 200;
    [SerializeField] private string fileName = "keyframeDatabase.yml";
    [SerializeField] private bool useRecord = false;
    [SerializeField] private string bagFileName = "bag1.bag";

    //ORB Parameters
    [Header("ORB Parameters")]
    [SerializeField] private int orbNFeatures = 500;
    [SerializeField] private float orbScaleFactor = 1.2f;
    [SerializeField] private int orbNLevels = 8;
    [SerializeField] private int orbEdgeThreshold = 31;
    [SerializeField] private int orbFirstLevel = 0;
    [SerializeField] private int orbWTA_K = 2;
    [SerializeField] private int orbScoreType = 0;
    [SerializeField] private int orbPatchSize = 31;
    [SerializeField] private int orbFastThreshold = 20;
    #endregion

    private Vector3 rotattedTranslationVector, initialCamPosition;
    private bool isStopped = false;
    private Thread trackingThread;
    private AutoResetEvent resetEvent;
    private bool reset_odom = false;
    private bool add_keyframe_by_hand = false;
    private string filePath;

    private float[] quaternionsCamera;
    private bool loopClosure;
    private Quaternion remappedRealSenseRotation;

    private IMUCameraRotation imuCameraRotation;

    private void Start()
    {
        //Get Camera initial position to apply as offset
        initialCamPosition = transform.localPosition;

        Debug.Log("---------------------------------- INICIO PROGRAMA --------------------------------");
        // Initialize the RealSense camera when the script starts
        string systemPath = Application.persistentDataPath;

        //Records video with camera
        if (useRecord)
        {
            string bagFilePath = systemPath + bagFileName;
            if (File.Exists(bagFilePath))
            {
                // The file exists, you can proceed with your operations on the file.
                Debug.Log("The file exists: " + bagFilePath);
                bagFileStreamConfig(bagFilePath);
            }
            else
            {
                // The file does not exist, handle the case where the file is missing.
                Debug.LogError("The file does not exist: " + bagFilePath);
            }
        }
        else
        {
            colorStreamConfig(colorWidth, colorHeight, colorFPS);
            depthStreamConfig(depthWidth, depthHeight, depthFPS);
        }

        initCamera();
        initImu();

        createORB(orbNFeatures, orbScaleFactor, orbNLevels, orbEdgeThreshold, orbFirstLevel, orbWTA_K, orbScoreType, orbPatchSize, orbFastThreshold);

        systemConfig config = new systemConfig();
        config.ratioTresh = ratioTresh;
        config.minDepth = minDepth;
        config.maxDepth = maxDepth;
        config.min3DPoints = min3DPoints;
        config.maxDistanceF2F = maxDistanceF2F;
        config.minFeaturesLoopClosure = minFeaturesLoopClosure;
        config.minFeaturesLoopClosure = minFeaturesLoopClosure;
        config.noMovementThresh = noMovementThresh;
        config.framesNoMovement = framesNoMovement;
        config.maxGoodFeatures = maxGoodFeatures;
        config.minFeaturesFindObject = minFeaturesFindObject;

        setParams(config);

        setProjectorZone(xRectangle, yRectangle, widthRectangle, heightRectangle);

        filePath = systemPath + "/" + fileName;
        if (!localizationMode)
        {
            FindObjectOfType<TrackingReferenceImageLibrary>().ConvertImagesToByteArrays();
            firstIteration();
        }
        else
        {
            deserializeKeyframeData(filePath);
        }

        //Thread handling
        trackingThread = new Thread(ThreadUpdate);
        trackingThread.Start();
        resetEvent = new AutoResetEvent(false);

        imuCameraRotation = GetComponent<IMUCameraRotation?>();
    }

    private void Update()
    {
        //Thread
        resetEvent.Set();

        //Apply RealSense position to camera, + initialPosition
        transform.localPosition = initialCamPosition + rotattedTranslationVector;

        remappedRealSenseRotation = new Quaternion(quaternionsCamera[0], -quaternionsCamera[1], quaternionsCamera[2], quaternionsCamera[3]);

        //Reset Odometry
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Resetting Odometry...");
            reset_odom = true;
        }

        //Add keyframe
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("Adding Keyframe...");
            add_keyframe_by_hand = true;
        }

        //Close application - TODO Don't have this here
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        //Loop closure trigger
        if (loopClosure)
        {
            OnLoopClosure();
        }
    }

    private void ThreadUpdate()
    {
        while (!isStopped)
        {
            resetEvent.WaitOne();

            findFeatures();
            //float depth = GetDepthAtCenter();
            float[] translationVector = RetrieveTranslationVector();
            Vector3 remappedTranslationVector = new Vector3(-translationVector[0], translationVector[1], -translationVector[2]);
            rotattedTranslationVector = Quaternion.AngleAxis(0, Vector3.right) * remappedTranslationVector;

            //Get raw RealSense rotation and remap it to camera
            remappedRealSenseRotation = new Quaternion(RetrieveCameraQuaternions().x,
                -RetrieveCameraQuaternions().y,
                RetrieveCameraQuaternions().z,
                RetrieveCameraQuaternions().w);

            loopClosure = isLoop();

            if (reset_odom == true)
            {
                resetOdom();
                reset_odom = false;
            }

            if (add_keyframe_by_hand == true)
            {
                addKeyframe();
                add_keyframe_by_hand = false;
            }
        }
    }

    private void OnLoopClosure()
    {
        //Send RealSense rotation to IMU script
        if (imuCameraRotation)
            imuCameraRotation.ReceiveRealSenseLoopClosure(remappedRealSenseRotation);
        
        //Set false so it only runs one frame
        loopClosure = false;
    }

    private void OnDestroy()
    {
        if (!localizationMode)
        {
            serializeKeyframeData(filePath);
        }
        isStopped = true;
        resetEvent.Set();
        trackingThread.Join();
        cleanupCamera();
    }
}
