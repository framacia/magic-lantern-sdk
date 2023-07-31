using UnityEngine;
using System.Collections.Generic;

public class ARCamera3 : MonoBehaviour
{
    /// <summary>
    /// Calculates the difference between current rotation and last rotation and returns it
    /// </summary>
    Vector3 UpdateLastRotationDelta()
    {
        _currentEuler = _gyroInput.transform.localRotation.eulerAngles;

        _lastRotationDelta = new Vector3(
            Mathf.DeltaAngle(_lastEuler.x, _currentEuler.x),
            Mathf.DeltaAngle(_lastEuler.y, _currentEuler.y),
            Mathf.DeltaAngle(_lastEuler.z, _currentEuler.z)
        );
        _lastEuler = _currentEuler;

        return _lastRotationDelta;
    }

    void RestoreRotation()
    {
        transform.localRotation = _gyroInput.transform.localRotation;
    }

    void UpdateTelemetry()
    {
        if (!_telemetry)
        {
            return;
        }

        DebugGUI.Graph("rotDeltaX", _averageRotationDelta.x);
        DebugGUI.Graph("rotDeltaY", _averageRotationDelta.y);
        DebugGUI.Graph("rotDeltaZ", _averageRotationDelta.z);
        DebugGUI.LogPersistent("rotMagMax", "Max Magnitude: " + _rotationMagnitudeMax);
    }

    void UpdateAverageRotationDelta()
    {
        _rotationVectorQ.Enqueue(_lastRotationDelta);
        if (_rotationVectorQ.Count > RotationBufferLength)
        {
            _rotationVectorQ.Dequeue();
        }
        float sumX = 0, sumY = 0, sumZ = 0;
        foreach (Vector3 rotVec in _rotationVectorQ)
        {
            sumX += rotVec.x;
            sumY += rotVec.y;
            sumZ += rotVec.z;
        }
        _lastAverageRotationDelta = _averageRotationDelta;
        _averageRotationDelta = new Vector3(sumX, sumY, sumZ) / _rotationVectorQ.Count;
    }

    void Awake()
    {
        _rotationVectorQ = new Queue<Vector3>();

        _gyroInput.transform.localRotation = transform.localRotation;
        _lastEuler = _gyroInput.transform.localRotation.eulerAngles;

        if (_telemetry)
        {
            DebugGUI.SetGraphProperties("rotDeltaX", "RotX", -0.4f, 0.4f, 0, new Color(1, 0.5f, 1), false);
            DebugGUI.SetGraphProperties("rotDeltaY", "RotY", -0.4f, 0.4f, 0, new Color(0.5f, 1, 1), false);
            DebugGUI.SetGraphProperties("rotDeltaZ", "RotY", -0.4f, 0.4f, 0, new Color(1, 1, 0.5f), false);
        }
    }

    void LateUpdate()
    {

        UpdateLastRotationDelta();
        UpdateAverageRotationDelta();
        UpdateTelemetry();
        RestoreRotation();
        transform.Rotate(_averageRotationDelta * RotationMultiplier);
    }

    /// <summary>
    /// Coefficient to apply to rotation
    /// </summary>
    [SerializeField] public float RotationMultiplier = 1;
    [SerializeField] public int RotationBufferLength = 5;

    [SerializeField] GyroInput _gyroInput;
    [SerializeField] bool _telemetry;

    Vector3 _lastEuler;
    Vector3 _currentEuler;
    Vector3 _lastRotationDelta;
    Vector3 _averageRotationDelta;
    Vector3 _lastAverageRotationDelta;

    Queue<Vector3> _rotationVectorQ;

    #region Telemetry
    float _rotationMagnitudeMax;
    #endregion
}
