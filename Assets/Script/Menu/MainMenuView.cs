using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MainMenuView : MonoBehaviourPun
{
    [SerializeField] private MainMenuManager mainMenu;

    void Awake()
    {
        if (!this.photonView.IsMine) Destroy(this);

        mainMenu = GetComponent<MainMenuManager>();
    }

    public void OnKickPlayer(Player newPlayer)
    {
        photonView.RPC("KickPlayer", newPlayer);
    }

    //public void OnLoadScene(Player newPlayer)
    //{
    //    photonView.RPC("LoadSceneMaster", newPlayer);
    //}

    [PunRPC]
    private void KickPlayer()
    {
        print("I was kicked");
        mainMenu.KickedPlayer();
    }

    //[PunRPC]
    //private void LoadSceneMaster()
    //{
    //    PhotonNetwork.LoadLevel(mainMenu.Level);
    //}
}
