using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MasterManager : MonoBehaviourPun
{
    private static MasterManager _instance;
    public static MasterManager Instance => _instance;
    private Dictionary<Player, CharacterModel> _characterModelReferences = new Dictionary<Player, CharacterModel>();
    private Dictionary<CharacterModel, Player> _playerReferences = new Dictionary<CharacterModel, Player>();

    private void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
    }

    public void RPCMaster(string methodName, params object[] p)
    {
        RPC(methodName, PhotonNetwork.MasterClient, p);
    }
    private void RPC(string methodName, Player target, params object[] p)
    {
        photonView.RPC(methodName, target, p);
    }

    public Player GetPlayerFromCharacter(CharacterModel character)
    {
        return _playerReferences.ContainsKey(character) ? _playerReferences[character] : null;
    }
    
    public CharacterModel GetCharacterModelFromPlayer(Player player)
    {
        return _characterModelReferences.ContainsKey(player) ? _characterModelReferences[player] : null;
    }
    
    public void AddCharacterModelReference(CharacterModel characterModel, Player client)
    {
        _characterModelReferences[client] = characterModel;
        _playerReferences[characterModel] = client;
    }

    [PunRPC]
    public void RequestMove(Player client, bool dir)
    {
        if (_characterModelReferences.ContainsKey(client))
        {
            _characterModelReferences[client].Move(dir);
        }
    }

    [PunRPC]
    public void RequestSelect(Player client)
    {
        if (_characterModelReferences.ContainsKey(client))
        {
            _characterModelReferences[client].SelectCard();
        }
    }
}
