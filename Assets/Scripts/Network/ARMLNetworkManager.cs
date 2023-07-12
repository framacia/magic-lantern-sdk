using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARMLNetworkManager : NetworkManager
{
    [Header("ARML")]
    public bool isAdmin = false;
    [SerializeField] GameObject lanternPlayer;
    [SerializeField] GameObject adminPlayer;

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
        }
    }

    //public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    //{
    //    if (!isAdmin)
    //    {
    //        GameObject _lanternPlayer = Instantiate(lanternPlayer);
    //        NetworkServer.AddPlayerForConnection(conn, _lanternPlayer);
    //    }
    //    else
    //    {
    //        GameObject _adminPlayer = Instantiate(adminPlayer);
    //        NetworkServer.AddPlayerForConnection(conn, _adminPlayer);
    //    }
    //}
}


