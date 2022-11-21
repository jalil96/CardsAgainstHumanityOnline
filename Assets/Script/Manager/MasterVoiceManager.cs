using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MasterVoiceManager : MonoBehaviourPun
{
    private static MasterVoiceManager _instance;
    public static MasterVoiceManager Instance => _instance;

    private Dictionary<Player, VoiceController> _soundReference = new Dictionary<Player, VoiceController>();

    private Dictionary<string, int> _playersColors = new Dictionary<string, int>();
    private List<int> _availableColors = new List<int>();
    public string ColorTag { get; private set; } = "ColorsDictionary";
    public Dictionary<string, int> PlayersColors => _playersColors;
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

    #region SoundManagement
    public void AddSoundReference(VoiceController soundRef, Player client)
    {
        _soundReference[client] = soundRef;
    }

    public void RemoveSoundReference(VoiceController soundRef, Player client)
    {
        if (_soundReference.ContainsKey(client))
            _soundReference.Remove(client);
    }

    [PunRPC]
    public void RequestUpdateSoundStatus(Player client, bool isSoundOpen)
    {
        if (_soundReference.ContainsKey(client))
        {
            _soundReference[client].EnabelSoundSystem(isSoundOpen);
        }
    }
    #endregion

    #region ChatManagement
    private void SetColorListIndex(int maxColorAvailable)
    {
        for (int i = 0; i < maxColorAvailable; i++)
        {
            _availableColors.Add(i);
        }
    }

    public void RequestAddToColorList(Player newPlayer)
    {
        RequestAddToColorList(newPlayer.NickName);
    }

    public void RequestAddToColorList(string nickname)
    {
        //Verify there is a color available and that the player is not already there
        Debug.Assert(!_playersColors.ContainsKey(nickname), "Player already is on the lsit");
        Debug.Assert(_availableColors.Count > 0, "No colors are available");

        //Get the color index to the new player
        var newColor = _availableColors[0];
        _availableColors.Remove(newColor);
        _playersColors.Add(nickname, newColor);

        //send all current players in room the updated list
        RefreshColorList();
    }

    public void RequestRemoveFromColorList(Player leavingPlayer)
    {
        RequestRemoveFromColorList(leavingPlayer.NickName);
    }

    public void RequestRemoveFromColorList(string nickname)
    {
        if (_playersColors.TryGetValue(nickname, out int colorIndex))
        {
            _availableColors.Add(colorIndex);
            _playersColors.Remove(nickname);
        }
        else
        {
            Debug.LogError("Player is not on the list");
        }

        //send all current players in room the updated list
        RefreshColorList();
    }

    private void RefreshColorList()
    {
        if (!PhotonNetwork.InRoom) return;
        ExitGames.Client.Photon.Hashtable colorDictionary = new ExitGames.Client.Photon.Hashtable();

        colorDictionary.Add(ColorTag, _playersColors);

        PhotonNetwork.CurrentRoom.SetCustomProperties(colorDictionary);
    }
    #endregion
}
