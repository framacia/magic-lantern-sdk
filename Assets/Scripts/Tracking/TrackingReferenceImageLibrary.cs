using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackingReferenceImageLibrary : MonoBehaviour
{
    [Serializable]
    struct TrackingReferenceImage
    {
        public Texture2D Image;
        public string ID;
    }

    [SerializeField] private List<TrackingReferenceImage> trackingReferenceImageList = new List<TrackingReferenceImage>();
    private Dictionary<string, byte[]> imageDictionary = new Dictionary<string, byte[]>();
    //[SerializeField] private Renderer renderer;

    private void Awake()
    {
        ConvertImagesToByteArrays();
    }

    public void ConvertImagesToByteArrays()
    {
        foreach (TrackingReferenceImage referenceImage in trackingReferenceImageList)
        {
            byte[] bytes = referenceImage.Image.GetRawTextureData();
            imageDictionary.Add(referenceImage.ID, bytes);
        }

        //Load back into texture

        //Texture2D convertedTexture = new Texture2D(256, 128, trackingReferenceImageList[0].Image.format, false);
        //convertedTexture.LoadRawTextureData(imageDictionary["bat"]);
        //convertedTexture.Apply();
        //renderer.material.mainTexture = convertedTexture;
    }
}