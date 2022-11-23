using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum CardType
{
    White,
    Black
}

public class CardModel : MonoBehaviourPun
{

    public Action<string> OnTextUpdated = delegate(string s) {  };

    [SerializeField] private string _text;
    [SerializeField] private CardType _type;

    private bool _isActive = true;
    public bool IsActive => _isActive;

    public string Text
    {
        get
        {
            return _text;
        }
        set
        {
            photonView.RPC(nameof(UpdateText), RpcTarget.All, value);
        }
    }

    private void Awake()
    {
        _type = CardType.White;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RequestText), PhotonNetwork.MasterClient, PhotonNetwork.LocalPlayer);
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        _isActive = false;
        photonView.RPC(nameof(UpdateIsActive), RpcTarget.Others, _isActive);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        _isActive = true;
        photonView.RPC(nameof(UpdateIsActive), RpcTarget.Others, _isActive);
    }

    [PunRPC]
    private void RequestText(Player player)
    {
        photonView.RPC(nameof(UpdateText), player, _text);
    }

    [PunRPC]
    private void UpdateText(string text)
    {
        _text = text;
        OnTextUpdated.Invoke(_text);
    }

    [PunRPC]
    private void UpdateIsActive(bool isActive)
    {
        _isActive = isActive;
    }
}