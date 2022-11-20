using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatColorsDictionary : MonoBehaviourPunCallbacks
{
    private Dictionary<string, int> _playersColors = new Dictionary<string, int>();

    private List<int> _availableColors = new List<int>();
    public string ColorTag { get; private set; } = "ColorsDictionary";

    public Dictionary<string, int> PlayersColors => _playersColors;
    public Action<Dictionary<string, int>> OnColorsUpdate = delegate { };

    private void Awake()
    {
        SetColorListIndex(6); //Six because it's the maximum of players
    }

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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (newPlayer.IsMasterClient) return;
        RequestAddToColorList(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (otherPlayer.IsMasterClient) return;
        RequestRemoveFromColorList(otherPlayer);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (var property in propertiesThatChanged)
        {
            if (property.Key.ToString() == ColorTag)
            
                if(property.Value is IDictionary)
                {
                    _playersColors = property.Value as Dictionary<string, int>;
                    OnColorsUpdate(_playersColors);
                }
        }
    }
}
