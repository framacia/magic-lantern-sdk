using UnityEngine;

public class GyroBoundCamera : MonoBehaviour
{
    void Awake()
    {
        _gyroInput.transform.localRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        transform.localRotation = _gyroInput.transform.localRotation;
    }

    [SerializeField] GyroInput _gyroInput;
}
