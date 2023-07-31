using UnityEngine;
using UnityEngine.UI;

public class ARCamera : MonoBehaviour
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
        _lastRotationMagnitude = _rotationMagnitude;
        _rotationMagnitude = _lastRotationDelta.magnitude;

        _lastEuler = _currentEuler;

        return _lastRotationDelta;
    }

    // <summary>
    // Restore the rotation if we are below the restore threshold.
    // </summary>
    void RestoreRotation()
    {
        if (_rotationMagnitude < RestoreThreshold)
        {
            transform.localRotation = _gyroInput.transform.localRotation;
        }
    }

    void LerpRestoreRotation()
    {
        if (RestoreFrames == 0)
        {
            return;
        }
        else
        {
            if (_restoreRotationSlerpCount > RestoreFrames)
            {
                _restoreRotationSlerpCount = 0;
                return;
            }
            if (_restoreRotationSlerpCount == 0)
            {
                _restoreRotationSlerpStart = transform.localRotation;
            }
            _restoreSlerp = (float)_restoreRotationSlerpCount / (float)RestoreFrames;
            transform.localRotation = Quaternion.Slerp(
                _restoreRotationSlerpStart,
                _gyroInput.transform.localRotation,
                _restoreSlerp
           );
            _restoreRotationSlerpCount += 1;
        }
    }

    void UpdateTelemetry()
    {
        if (!_telemetry)
        {
            return;
        }
        _rotationMagnitudeMax = _rotationMagnitude > _rotationMagnitudeMax
            ? _rotationMagnitude
            : _rotationMagnitudeMax;

        DebugGUI.Graph("rotMag", _rotationMagnitude);
        DebugGUI.LogPersistent("rotMagMax", "Max Magnitude: " + _rotationMagnitudeMax);
        DebugGUI.LogPersistent("restoreCount", "Restore Slerp: " + _restoreSlerp);
    }

    void Awake()
    {
        _gyroInput.transform.localRotation = transform.localRotation;
        _lastEuler = _gyroInput.transform.localRotation.eulerAngles;

        if (_telemetry)
        {
            DebugGUI.SetGraphProperties("rotMag", "Rot Mag", 0, 0.2f, 0, new Color(1, 0.5f, 1), false);
        }

    }

    void LateUpdate()
    {

        UpdateLastRotationDelta();
        UpdateTelemetry();
        transform.Rotate(_lastRotationDelta * RotationMultiplier);
        RestoreRotation();
    }

    /// <summary>
    /// Coefficient to apply to rotation
    /// </summary>
    [SerializeField] public float RotationMultiplier = 1;

    /// <summary>
    /// Value below which the rotation will be moved back to unmodified value
    /// </summary>
    [SerializeField] public float RestoreThreshold = 1;

    /// <summary>
    /// Rotation will be restored by Slerping over this many frames
    /// </summary>
    [SerializeField] public int RestoreFrames = 0;

    [SerializeField] GyroInput _gyroInput;
    [SerializeField] bool _telemetry;

    Vector3 _lastEuler;
    Vector3 _currentEuler;
    Vector3 _lastRotationDelta;
    float _rotationMagnitude;
    float _lastRotationMagnitude;

    Quaternion _restoreRotationSlerpStart;
    int _restoreRotationSlerpCount = 0;

    #region Telemetry
    float _rotationMagnitudeMax;
    float _restoreSlerp;
    #endregion
}
