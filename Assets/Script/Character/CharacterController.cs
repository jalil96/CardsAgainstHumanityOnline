using System;
using Photon.Pun;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Destroy(Camera.main.gameObject);
            Destroy(this);
        }
    }

    private void Start()
    {
        MasterManager.Instance.RPCMaster("RequestConnectPlayer", PhotonNetwork.LocalPlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MasterManager.Instance.RPCMaster("RequestMove", PhotonNetwork.LocalPlayer, true);
        
        if(Input.GetKeyDown(KeyCode.LeftArrow)) 
            MasterManager.Instance.RPCMaster("RequestMove", PhotonNetwork.LocalPlayer, false);
    }
}