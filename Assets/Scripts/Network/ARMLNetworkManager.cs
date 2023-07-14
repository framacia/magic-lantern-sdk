using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARMLNetworkManager : NetworkManager
{
    [Header("ARML")]
    public bool isAdmin = false;

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
}
