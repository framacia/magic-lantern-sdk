using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.XR.ARFoundation;

public class ArrowIndicator : MonoBehaviour
{
    [Header("AR required")]
    [SerializeField]
    public Camera arCam;
    public float distanceFromCamera = 0.65f;

    [Header("Target and angle threshold")]
    public Transform target;
    public float dotThreshold = 0.75f; //Around 45 deg for normalized vectors

    [Header("Color and Fade")]
    public Color DisplayColor = Color.red;
    public float fadeTime = 1f;
    private float alpha;
    public Material mat {get; private set;}


    private void Start()
    {
        this.transform.parent = arCam.transform;
        this.transform.localPosition = Vector3.forward * distanceFromCamera;

        mat = this.transform.GetChild(0).GetComponent<Renderer>().material;
    }
    // Update is called once per frame
    void Update()
    {
        RotateTowardsObject();
        UpdateOpacity();
    }
    void RotateTowardsObject()
    {
        if (target == null)
            return;

        Quaternion desiredRotation = Quaternion.LookRotation(target.position - this.transform.position);
        this.transform.rotation = desiredRotation;
    }
    void UpdateOpacity()
    {
        if (target == null)
        {
            mat.color = new Color(1f, 0f, 0f, 0f); //Transparent red
            return;
        }

        float dot = Vector3.Dot(arCam.transform.forward, this.transform.forward);

        //Greater than threshold means alignment with the object, the arrow will fade out
        float alphaAddition = (dot > dotThreshold ? -Time.deltaTime : Time.deltaTime) / fadeTime;
        alpha = Mathf.Clamp(alpha + alphaAddition, 0f, 1f);

        mat.color = new Color(DisplayColor.r, DisplayColor.g, DisplayColor.b, alpha);
    }
}
