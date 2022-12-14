using Newtonsoft.Json.Linq;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommunicationsManager : MonoBehaviourPunCallbacks
{
    static public CommunicationsManager Instance { get; private set; }
    static public bool HasInstance => Instance != null;

    public ChatManager chatManager;
    public VoiceManager voiceManager;
    public CommandManager commandManager;
    public InputManager inputManager;

    //COLOR MANAGMENT
    private Dictionary<string, int> _playersColors = new Dictionary<string, int>();
    private List<int> _availableColors = new List<int>();
    public string ColorTag { get; private set; } = "ColorsDictionary";
    public Dictionary<string, int> PlayersColors => _playersColors;
    public Action<Dictionary<string, int>> OnColorsUpdate = delegate { };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
			return;
        }
        
		Instance = this;
        DontDestroyOnLoad(gameObject);

        //lets make sure than when the game starts, we have all the managers active
        chatManager.gameObject.SetActive(true);
        voiceManager.gameObject.SetActive(true);
        commandManager.gameObject.SetActive(true);

        SetColorListIndex(6); //Six because it's the maximum of players
    }


    public List<Player> GetCurrentUserList()
    {
        return PhotonNetwork.PlayerList.ToList();
    }

    public Player GetPlayerByNickname(string nickname)
    {
        var currentPlayers = PhotonNetwork.PlayerList.ToList();

        for (int i = 0; i < currentPlayers.Count; i++)
        {
            if (currentPlayers[i].NickName == nickname)
                return currentPlayers[i];
        }
        return null;
    }

    public bool MutePlayer(string nickname)
    {
        var player = GetPlayerByNickname(nickname);
        if(player != null)
        {
            voiceManager.MuteAnotherPlayer(player);
            return true;
        }

        return false;
    }

    public bool SentMessageOpenAChat(string nickname)
    {
        var player = GetPlayerByNickname(nickname);
        
        if(player != null)
        {
            voiceManager.MyVoiceController.OderToOpenPrivateChat(player);
            return true;
        }

        return false;
    }

    #region Colors
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
        //Verify there is a color available and that the player is not already
        if (_playersColors.ContainsKey(nickname)) return;
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

    //#region ColorRPC

    //public void RemoveFromColorList(Player player)
    //{
    //    photonView.RPC(nameof(RemovePlayerFromColorList), RpcTarget.Others, player.NickName);
    //}

    //[PunRPC]
    //public void RemovePlayerFromColorList(string player)
    //{

    //}

    //public void AddToColorList(Player player)
    //{
    //    photonView.RPC(nameof(AddPlayerToColorList), RpcTarget.Others, player.NickName);
    //}

    //[PunRPC]
    //public void AddPlayerToColorList(string player)
    //{

    //}

    //#endregion

    #region Callbacks
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer.IsMasterClient) return;
        if (!PhotonNetwork.IsMasterClient) return;
        RequestAddToColorList(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.IsMasterClient) return;
        if (!PhotonNetwork.IsMasterClient) return;
        RequestRemoveFromColorList(otherPlayer);
        chatManager.CloseChat(otherPlayer.NickName);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (var property in propertiesThatChanged)
        {
            if (property.Key.ToString() == ColorTag)

                if (property.Value is IDictionary)
                {
                    _playersColors = property.Value as Dictionary<string, int>;
                    OnColorsUpdate(_playersColors);
                }
        }
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient) return;

        chatManager.ConnectChat();
        voiceManager.EnableVoiceSettings();
    }

    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsMasterClient) return;

        chatManager.DisableChat();
        voiceManager.DisableVoiceSettings();
    }
    #endregion
}
