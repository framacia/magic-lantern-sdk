using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorMovement : MonoBehaviour
{
    [Header("Deltas")]
    [SerializeField]
    protected Vector3 positionDelta;
    protected Vector3 initialPosition;
    [SerializeField]
    protected Vector3 rotationDelta;
    protected Quaternion initialRotation;
    [SerializeField]
    protected float scaleFactor;
    protected Vector3 initialScale;

    [Header("LerpTime")]
    [SerializeField]
    protected float lerpTime;

    protected void Start(){
        initialPosition = this.transform.position;
        initialRotation = this.transform.rotation;
        initialScale = this.transform.localScale;
    }

    protected void Update(){
        float adjustedTime = (Time.time / lerpTime) * 2.0f * Mathf.PI;
        float value = Mathf.Sin(adjustedTime) * 0.5f + 0.5f;
        this.transform.position = Vector3.Lerp(initialPosition, initialPosition + positionDelta, value);
        this.transform.rotation = Quaternion.Lerp(initialRotation, initialRotation * Quaternion.Euler(rotationDelta.x, rotationDelta.y, rotationDelta.z), value);
        this.transform.localScale = Vector3.Lerp(initialScale, initialScale * scaleFactor, value);
    }
}
