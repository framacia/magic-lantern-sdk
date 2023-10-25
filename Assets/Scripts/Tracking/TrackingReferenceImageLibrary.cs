using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingReferenceImageLibrary : MonoBehaviour
{
    [SerializeField] private List<TrackingReferenceImage> trackingReferenceImageList = new List<TrackingReferenceImage>();

    public void ConvertImagesToByteArrays()
    {
        foreach (TrackingReferenceImage referenceImage in trackingReferenceImageList)
        {
            byte[] bytes = referenceImage.Image.EncodeToPNG();
        }
    }
}

[Serializable]
struct TrackingReferenceImage
{
    public Texture2D Image;
    public string ID;
}