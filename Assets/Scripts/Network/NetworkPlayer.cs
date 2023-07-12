using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

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
        adminUI = transform.Find("AdminUI").gameObject;
    }

    public override void OnStartLocalPlayer()
    {
        if (isServer)
        {
            playerType = PlayerType.LanternPlayer;
        }

        if(manager.isAdmin)
        {
            playerType = PlayerType.AdminPlayer;
        }
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

        if (playerType == PlayerType.LanternPlayer)
        {
            adminUI.SetActive(false);
        }

        geometryParent = GameObject.Find("--GEOMETRY--");
    }

    void FixedUpdate()
    {
        if(playerType == PlayerType.AdminPlayer)
        {
            float xAxisValue = Input.GetAxis("HorizontalArrow");
            float zAxisValue = Input.GetAxis("VerticalArrow");

            CmdMoveObject(xAxisValue, zAxisValue);
        }
    }

    [Command]
    void CmdMoveObject(float xAxisValue, float zAxisValue)
    {
        if (geometryParent != null)
        {
            //Rotation
            geometryParent.transform.Rotate(-zAxisValue * speed, xAxisValue * speed, 0);

            //Force Z rotation to 0
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

            //Translation
            Vector3 movementVector = (geometryParent.transform.forward * Input.GetAxis("Vertical") * 10) + 
                (geometryParent.transform.right * Input.GetAxis("Horizontal") * 10);

            geometryParent.transform.position += new Vector3(movementVector.x, 0, movementVector.z) * Time.deltaTime;
        }
    }

    //private void AssignPlayerType()
    //{
    //    if (isServer && isLocalPlayer)
    //    {
    //        playerType = PlayerType.LanternPlayer;
    //    }
    //    else if (isServer && !isLocalPlayer)
    //    {
    //        playerType = PlayerType.AdminPlayer;
    //    }
    //    else if (!isServer && isLocalPlayer)
    //    {
    //        playerType = PlayerType.AdminPlayer;
    //    }
    //    else if (!isServer && !isLocalPlayer)
    //    {
    //        playerType = PlayerType.LanternPlayer;
    //    }
    //}
}
