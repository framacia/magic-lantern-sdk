using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualHandIndicator : MonoBehaviour
{
    [SerializeField]
    public RectTransform indicator;
    private Camera cam;

    void Start(){
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        indicator.position = cam.WorldToScreenPoint(this.transform.position);
    }
}
