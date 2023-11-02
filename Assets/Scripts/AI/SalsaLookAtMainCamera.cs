using CrazyMinnow.SALSA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Eyes))]
public class SalsaLookAtMainCamera : MonoBehaviour
{
    [SerializeField] bool lookAtMainCamera;
    private Eyes eyes;

    // Start is called before the first frame update
    void Start()
    {
        eyes = GetComponent<Eyes>();

        if (lookAtMainCamera)
        {
            //This will cause problems with several cameras, when building should change to get main camera (as it should never be inactive)
            if(Camera.main != null)
            {
                eyes.lookTarget = Camera.main.transform;
            }
            else
            {
                StartCoroutine(TryToGetCamera());
            }
        }
    }

    IEnumerator TryToGetCamera()
    {
        Debug.Log("trying to get camera");
        if(Camera.main != null)
        {
            eyes.lookTarget = Camera.main.transform;
            Debug.Log("Got camera!");
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(TryToGetCamera());
        }
    }
}
