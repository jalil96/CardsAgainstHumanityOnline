using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class JudgeModel : MonoBehaviourPun
{
    public Action OnNicknameUpdated = delegate {};
    
    [SerializeField] private string _nickname;

    public string Nickname => _nickname;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) photonView.RPC(nameof(RequestNickname), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    public void SetNickname(string nickname)
    {
        photonView.RPC(nameof(UpdateNickname), RpcTarget.All, nickname);
    }

    [PunRPC]
    private void UpdateNickname(string nickname)
    {
        var localNickname = PhotonNetwork.LocalPlayer.NickName;

        _nickname = nickname == localNickname ? nickname + "(YOU)" : nickname;

        OnNicknameUpdated.Invoke();
    }

    [PunRPC]
    private void RequestNickname(Player client)
    {
        photonView.RPC(nameof(UpdateNickname), client, _nickname);
    }
}