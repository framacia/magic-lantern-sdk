using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroInput : MonoBehaviour
{
    void EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            _remoteGyro = Input.gyro;
            _remoteGyro.enabled = true;
        }
        else
        {
            _remoteGyroEnabled = false;
        }
    }

    Vector3 RemoteGyroToEuler(Quaternion imuRot)
    {
        Vector3 v = imuRot.eulerAngles;
        return new Vector3(v.y, -v.x, v.z);
    }

    Vector3 GyroToEuler(Quaternion imuRot)
    {
        return imuRot.eulerAngles;
    }

    void Start()
    {
        if (_remoteGyroEnabled)
        {
            EnableGyro();
        }
    }

    void HandleArrowKeyInputs()
    {
        float xAxisValue = Input.GetAxis("HorizontalArrow");
        float zAxisValue = Input.GetAxis("VerticalArrow");
        transform.Rotate(-zAxisValue * _arrowKeyMoveSpeed, xAxisValue * _arrowKeyMoveSpeed, 0);
    }

    void HandleRemoteGyro()
    {
        if (!_remoteGyroEnabled)
        {
            return;
        }
        if (_imuStartEuler == Vector3.zero && ++_gyroWaitCount > 500)
        {
            _camStartEuler = transform.localRotation.eulerAngles;
            _imuStartEuler = RemoteGyroToEuler(_remoteGyro.attitude);
            Debug.Log(_imuStartEuler);
            return;
        }
        if (_imuStartEuler != Vector3.zero)
        {
            Vector3 imuRelativeEuler = RemoteGyroToEuler(_remoteGyro.attitude) - _imuStartEuler;
            transform.localRotation = Quaternion.Euler(imuRelativeEuler + _camStartEuler);
        }
    }

    void HandleGyro()
    {
        if (!_gyroEnabled)
        {
            return;
        }
        if (_imuStartEuler == Vector3.zero)
        {
            _camStartEuler = transform.localRotation.eulerAngles;
            _imuStartEuler = GyroToEuler(GyroReader.GetQuaternion());
            Debug.Log(_imuStartEuler);
            return;
        }
        Vector3 imuRelativeEuler = GyroToEuler(GyroReader.GetQuaternion()) - _imuStartEuler;
        transform.localRotation = Quaternion.Euler(imuRelativeEuler + _camStartEuler);
    }

    void Update()
    {
        HandleArrowKeyInputs();
        HandleRemoteGyro();
        HandleGyro();
    }

    [SerializeField] float _arrowKeyMoveSpeed = 1f;
    [SerializeField] bool _remoteGyroEnabled;
    [SerializeField] bool _gyroEnabled;

    Gyroscope _remoteGyro;

    Vector3 _camStartEuler;
    Vector3 _imuStartEuler;

    int _gyroWaitCount = 0;
}
