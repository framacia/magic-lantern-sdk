using UnityEngine;

public class RotationTracker : MonoBehaviour
{

    //Holds the previous frames rotation
    Quaternion lastRotation;

    PostProcessingController postProcessingController;

    [SerializeField] float accelerationThreshold = 15f;
    [SerializeField] float accelerationAmount = 0.01f;

    //Averaging stuff
    int averageFrames = 5; //made public in case you want to change it in the Inspector, if not, could be declared Constant
    private int count;
    private Vector3 averagedAngularVelocity;

    void Start()
    {
        lastRotation = transform.rotation;
        postProcessingController = FindObjectOfType<PostProcessingController?>(true);
    }

    void LateUpdate()
    {
        count++;

        if (count > averageFrames)
        {
            averagedAngularVelocity = averagedAngularVelocity + (GetAngularVelocityVector() - averagedAngularVelocity) / (averageFrames + 1);

        }
        else
        {
            //NOTE: The MovingAverage will not have a value until at least "MovingAverageLength" values are known (10 values per your requirement)
            averagedAngularVelocity += GetAngularVelocityVector();

            //This will calculate ONLY the very first value of the MovingAverage,
            if (count == averageFrames)
            {
                averagedAngularVelocity = averagedAngularVelocity / count;

                //Debug.Log("Moving Average: " + movingAverage); //for testing purposes
            }
        }

        if (averagedAngularVelocity.magnitude > 0.01f)
        {
            postProcessingController?.ChangeDoFFocalLength(averagedAngularVelocity.magnitude * 20);
        }

        //TODO This needs to check if it's bigger than the ORIGINAL IMU rotation
        if (averagedAngularVelocity.magnitude > accelerationThreshold)
        {

            transform.Rotate(averagedAngularVelocity.x * accelerationAmount,
    averagedAngularVelocity.y * accelerationAmount,
    averagedAngularVelocity.z * accelerationAmount);

            
        }


    }

    Vector3 GetAngularVelocityVector()
    {
        var deltaRot = transform.rotation * Quaternion.Inverse(lastRotation);
        var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));

        lastRotation = transform.rotation;

        return eulerRot / Time.deltaTime;
    }
}