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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestMove), PhotonNetwork.LocalPlayer, true);
        
        if(Input.GetKeyDown(KeyCode.LeftArrow)) 
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestMove), PhotonNetwork.LocalPlayer, false);
        
        if (Input.GetKeyDown(KeyCode.Return))
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestSelect), PhotonNetwork.LocalPlayer);
    }
}