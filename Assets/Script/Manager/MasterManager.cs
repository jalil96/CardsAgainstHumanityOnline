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

    [SerializeField] private GameObject _winScreen;
    [SerializeField] private GameObject _loseScreen;

    private SoundEffectManager _soundEffectManager;

    public Action<float> AddTimeEvent = delegate { };
    public Action<Player> OnChangeMyWhiteCards = delegate(Player player) { };
    public Action OnChangeAllWhiteCards = delegate { };
    public Action OnChangeBlackCard = delegate { };

    private void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
        _winScreen.SetActive(false);
        _loseScreen.SetActive(false);

    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.Instantiate("PartyEffectsManager", Vector3.zero, Quaternion.identity);

        _soundEffectManager = PhotonNetwork.Instantiate("SoundEffectsManager", Vector3.zero, Quaternion.identity).GetComponent<SoundEffectManager>();
    }

    public void RPCMaster(string methodName, params object[] p)
    {
        RPC(methodName, PhotonNetwork.MasterClient, p);
    }
    public void RPC(string methodName, Player target, params object[] p)
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

    /*[PunRPC]
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
    }*/

    [PunRPC]
    public void WinConditionMet(bool winner)
    {
        if (winner) _winScreen.SetActive(true);
        else _loseScreen.SetActive(true);
    }

    [PunRPC]
    public void RequestSoundHorn(Player player)
    {
        _soundEffectManager.SendPlaySoundHorn();
    }

    [PunRPC]
    public void RequestAddTime(Player client, string time)
    {
        float.TryParse(time, out float result);
        AddTimeEvent.Invoke(result);
    }

    [PunRPC]
    public void RequestChangeMyWhiteCards(Player client)
    {
        OnChangeMyWhiteCards.Invoke(client);
    }

    [PunRPC]
    public void RequestChangeAllWhiteCards(Player client)
    {
        OnChangeAllWhiteCards.Invoke();
    }

    [PunRPC]
    public void RequestChangeBlackCard(Player client)
    {
        OnChangeBlackCard.Invoke();
    }
}
