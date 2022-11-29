using System;
using Photon.Pun;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private CharacterModel _characterModel;
    
    // private InputManager _inputManager;

    public void Destroy()
    {
        Destroy(this);
    }

    private void Start()
    {
        CommunicationsManager.Instance.inputManager.OnReturnPressed += ReturnPressed;
        CommunicationsManager.Instance.inputManager.OnRightArrowPressed += RightArrowPressed;
        CommunicationsManager.Instance.inputManager.OnLeftArrowPressed += LeftArrowPressed;
    }

    private void LeftArrowPressed()
    {
        _characterModel.Move(false);
    }
    
    private void RightArrowPressed()
    {
        _characterModel.Move(true);
    }
    
    private void ReturnPressed()
    {
        _characterModel.SelectCard();
    }
    
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestMove), PhotonNetwork.LocalPlayer, true);
        
        if(Input.GetKeyDown(KeyCode.LeftArrow)) 
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestMove), PhotonNetwork.LocalPlayer, false);
        
        if (Input.GetKeyDown(KeyCode.Return))
            MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestSelect), PhotonNetwork.LocalPlayer);
    }*/
}