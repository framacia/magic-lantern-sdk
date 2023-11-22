using CrazyMinnow.SALSA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Eyes))]
public class SalsaLookAtMainCamera : MonoBehaviour
{
    [SerializeField] bool lookAtMainCamera;
    private Eyes eyes;

    void OnEnable()
    {
        NetworkPlayer.OnPlayerLoaded += GetCamera;
    }

    void OnDisable()
    {
        NetworkPlayer.OnPlayerLoaded -= GetCamera;
    }

    void GetCamera()
    {
        eyes.lookTarget = Camera.main.transform;
        Debug.Log("Got camera!");
    }

    // Start is called before the first frame update
    void Start()
    {
        eyes = GetComponent<Eyes>();
    }
}
