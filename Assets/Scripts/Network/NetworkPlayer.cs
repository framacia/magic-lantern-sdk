using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkPlayer : NetworkBehaviour
{
    GameObject geometryParent;
    GameObject adminUI;
    ARMLNetworkManager manager;
    float speed = 1f;

    enum PlayerType
    {
        LanternPlayer,
        AdminPlayer
    }

    PlayerType playerType;

    private void Awake()
    {
        manager = FindObjectOfType<ARMLNetworkManager>();

        //TODO Find better way of handling this reference
        adminUI = GameObject.Find("AdminUI").gameObject;
    }

    public override void OnStartLocalPlayer()
    {
        if (isServer)
        {
            playerType = PlayerType.LanternPlayer;
            adminUI.SetActive(false);
        }

        if (manager.isAdmin)
        {
            playerType = PlayerType.AdminPlayer;
            adminUI.SetActive(true);
        }

        //Subscribe to event only if local player
        PostProcessingController.OnPostProcessingChanged += UpdatePostProcessing;
    }

    private void Start()
    {
        //TODO Improve this
        if (!isLocalPlayer)
        {
            if (isServer)
                playerType = PlayerType.AdminPlayer;
            else
                playerType = PlayerType.LanternPlayer;
        }

        name = playerType.ToString();

        geometryParent = GameObject.Find("--GEOMETRY--");

    }

    void Update()
    {
        if (!isLocalPlayer) return;

        float xAxisValue = Input.GetAxis("Horizontal");
        float zAxisValue = Input.GetAxis("Vertical");

        if (xAxisValue > 0.1f || zAxisValue > 0.1f || xAxisValue < -0.1f || zAxisValue < -0.1f)
            MoveGeometry(xAxisValue, zAxisValue);
    }

    void MoveGeometry(float xAxisValue, float zAxisValue)
    {
        if (geometryParent != null)
        {
            //Rotation
            //geometryParent.transform.Rotate(-zAxisValue * speed, xAxisValue * speed, 0);

            //Force Z rotation to 0
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

            //Translation
            Vector3 movementVector = (geometryParent.transform.forward * zAxisValue * 10) +
                (geometryParent.transform.right * xAxisValue * 10);

            geometryParent.transform.position += new Vector3(movementVector.x, 0, movementVector.z) * Time.deltaTime;

            UpdateObjectPosition(geometryParent, geometryParent.transform.position);
        }
    }

    [Command]
    void UpdateObjectPosition(GameObject go, Vector3 newPosition)
    {
        go.transform.position = newPosition;
    }

    //TODO Test if this can be called direcly from the action subscription
    [Command]
    void UpdatePostProcessing(PostProcessingConfig config, GameObject go)
    {
        go.GetComponent<PostProcessingController>().SetPostProcessingConfig(config);
    }
}
