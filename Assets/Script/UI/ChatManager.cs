using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Chat;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using UnityEditor.VersionControl;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    [Header("References")]
    public TextMeshProUGUI content;
    public TMP_InputField inputField;
    public Button sendButton;

    [Header("Colors")]
    public List<Color> colorNameList = new List<Color>();
    public Color privateMessageColor = Color.yellow;

    [Header("Style Status Font")]
    public Color statusColor = Color.green;
    public float statusFontSize = 10.5f;
    public Font statusFontStyle;

    //private variables
    private string userPrivateCommand = "@";
    private ChatClient _chatClient;
    private string _channel;

    public bool ChatEnabled { get; set; }

    //EVENTS
    public Action OnChatConnected;
    public Action OnChatDisconected;

    public Action OnChatSuscribed;
    public Action OnChatUnsuscribed;

    private void Start()
    {
        sendButton.onClick.AddListener(SendChatMessage);
        inputField.onEndEdit.AddListener(SendChatMessage);

        ConnectChat();
    }

    public void ConnectChat()
    {
        _chatClient = new ChatClient(this);

        AuthenticationValues auth = new AuthenticationValues(PhotonNetwork.NickName);
        _chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, auth);
    }

    private void Update()
    {
        if (!ChatEnabled) return;

        _chatClient.Service();
    }

    public void SendChatMessage()
    {
        var message = inputField.text;
        SendChatMessage(message);
    }

    public void SendChatMessage(string chat)
    {
        if (string.IsNullOrEmpty(chat) || string.IsNullOrWhiteSpace(chat)) return;

        print("text is: " + chat);

        _chatClient.PublishMessage(_channel, chat);
        
        inputField.text = "";

    }

    public void PrivateMessage(string message)
    {
        string[] words = message.Split(' ');
        if (words[0].StartsWith(userPrivateCommand))
        {
            var target = words[0].Remove(0, userPrivateCommand.Length);

            var players = PhotonNetwork.CurrentRoom.Players;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].NickName == target)
                {

                }
            }
        }
    }

    public void UnsuscribeFromRoom(string roomName)
    {
        //TODO ADD ROOM UNSUSCRIPTION
    }

    //if the player doesn't use a command, send it like a private message.
    //if it starts a command, then check if the player is right, else let te player know somewhere that the nickname is wrong but don't send it.
    //if everything is right, chat and player, then send it alright. 

    #region Use for me
    private void AddChatMessage(string playerNickname, string message, int playerIndex)
    {
        content.text += $"{ColorfyNickname(playerNickname, playerIndex)} {message}";
    }

    private void AddPrivateChatMessage(string playerNickname, string message)
    {
        content.text += $"<color = {privateMessageColor}>{playerNickname}:</color> {message} \n";
    }

    private int GetUserIndexColor(string nickname)
    {
        return 0; // todo get which user is it and put their assigned index color maybe? dictionary with current users? asign one to each new player?
    }

    private bool IsMessageValid(string message)
    {
        return (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message));
    }


    private string ColorfyNickname(string playerNickname, int playerIndex)
    {
        return $"<color = {colorNameList[playerIndex]}>{playerNickname}:</color>";
    }

    private bool IsCommand(string message)
    {
        //TODO for future use, have a dictionary of commands, if it's a command then call command action or something;
        return false;
    }
    #endregion


    public void DebugReturn(DebugLevel level, string message)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnected()
    {
        OnChatDisconected?.Invoke(); //TODO make main manager listen to this and change status info

        ChatEnabled = false;
    }

    public void OnConnected()
    {
        OnChatConnected?.Invoke(); //TODO make main manager listen to this and change status info

        _channel = PhotonNetwork.CurrentRoom.Name; //TODO MAKE UNSUSCRIBE CHANNEL ON LEAVE ROOM. 

        _chatClient.Subscribe(_channel);

        ChatEnabled = true;

        string[] friends = new string[] { };
        _chatClient.AddFriends(friends);
        _chatClient.SetOnlineStatus(ChatUserStatus.Online);
    }

    public void OnChatStateChange(ChatState state)
    {
        throw new NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            AddChatMessage(senders[i], messages[i].ToString(), GetUserIndexColor(senders[i]));
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        AddPrivateChatMessage(sender, message.ToString());
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        OnChatSuscribed?.Invoke(); //TODO make main manager listen to this and change status info
    }

    public void OnUnsubscribed(string[] channels)
    {
        OnChatUnsuscribed?.Invoke(); //TODO make main manager listen to this and change status info
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        string userStatus = status == ChatUserStatus.Online ? "connected" : "disconected";
        content.text += $"<color={statusColor}>{user} is {message} </color> \n";
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }
}
