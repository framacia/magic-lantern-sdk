using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using UnityEngine;
using System.Linq;

public class ARMLNetworkManager : NetworkManager
{
    [Header("ARML")]
    public bool isAdmin = false;
    public bool autoConnectoToHotspot = false;

    private void Awake()
    {
#if UNITY_EDITOR
        isAdmin = true;
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isAdmin)
        {
            StartHost();
            GetComponent<NetworkManagerHUD>().enabled = false;
            //TODO Handle the on-screen notifications for Admin connected/disconnected etc.
        }
        else
        {
            //TODO Implement list of hotspots that fit criteria (number of lanters) to choose from
            if(autoConnectoToHotspot)
                networkAddress = GetDefaultGateway()?.ToString();
        }
    }

    public override void OnStartHost()
    {
        base.OnStartHost();

    }

    public void SetNetworkAddress(string networkAddress)
    {
        networkAddress = networkAddress ?? string.Empty;
    }

    public static IPAddress GetDefaultGateway()
    {
        IPAddress iPAddress = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Where(n => n.Name.StartsWith("ARML"))
            .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
            .Select(g => g?.Address)
            .Where(a => a != null)
            .FirstOrDefault();

        if (iPAddress != null)
            return iPAddress;
        else
        {
            Debug.LogError("ARML Hotspot not found! Check your connection settings");
            return null;
        }
    }

    public override void OnStartServer()
    {
        Debug.Log("Host has started server");
    }

    public override void OnServerConnect()
    {
        base.OnServerConnect();
        Debug.Log("Client has connected to server");
    }
}
