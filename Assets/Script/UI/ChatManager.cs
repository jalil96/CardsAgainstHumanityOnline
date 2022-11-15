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
using Photon.Voice;
using JetBrains.Annotations;

[System.Serializable]

public class ChatManager : MonoBehaviour, IChatClientListener
{
    [Header("Chat References")]
    public GameObject chatBox;
    public GameObject chatMinimizedNotification;
    public TextMeshProUGUI mainChatContent;
    public TMP_InputField inputField;

    [Header("Buttons")]
    public Button openChatButton;
    public Button closeChatButton;
    public Button sendButton;

    [Header("Colors")]
    public List<Color> colorNameList = new List<Color>();
    public Color privateMessageColor = Color.yellow;

    [Header("Info Font")]
    public Color serverInfoColor = Color.grey;

    [Header("Status Font")]
    public Color statusColor = Color.green;

    //private variables
    private ChatClient _chatClient;
    private string _channel;
    private Dictionary<string, int> playersColorDictionary = new Dictionary<string, int>();
    private bool chatNotificationActive = false;

    public bool ChatEnabled { get; set; }
    public bool ChatMinimized { get; private set; }

    //EVENTS
    public Action OnChatConnected;
    public Action OnChatDisconected;

    public Action OnChatSuscribed;
    public Action OnChatUnsuscribed;
    public Action<string> OnChatStateChanged;

    private void Awake()
    {
        DisableAllChat();

        openChatButton.onClick.AddListener(OpenChat);
        closeChatButton.onClick.AddListener(MinimizedChat);

        sendButton.onClick.AddListener(SendChatMessage);
        inputField.onEndEdit.AddListener(SendChatMessage);
    }

    private void Start()
    {
        CommandManager.Instance.PrivateMessageCommand += SendPrivateChatMessage;
        CommandManager.Instance.ErrorCommand += ErrorCommandMessage;
    }

    private void OnDestroy()
    {
        CommandManager.Instance.PrivateMessageCommand -= SendPrivateChatMessage;
        CommandManager.Instance.ErrorCommand -= ErrorCommandMessage;
    }

    public void ConnectChat()
    {
        _chatClient = new ChatClient(this);

        AuthenticationValues auth = new AuthenticationValues(PhotonNetwork.NickName);
        _chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, auth);
        ChatEnabled = true;
        OpenChat();
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

    public void SendChatMessage(string message)
    {
        if (!IsMessageValid(message)) return;
        if (CommandManager.Instance.IsCommand(message)) return;

        print("text is: " + message);

        _chatClient.PublishMessage(_channel, message);
        
        inputField.text = "";
    }

    public void SendPrivateChatMessage(string message)
    {
        //TODO: Verify that there is an user withing the message. if not, then throw a warning in the chatbox. 
    }

    public void UnsuscribeFromRoom(string roomName)
    {
        string[] roomsList = new string[] { roomName };

        _chatClient.Unsubscribe(roomsList);
    }

    public void DisableAllChat()
    {
        ChatEnabled = false;
        chatBox.gameObject.SetActive(false);
        openChatButton.gameObject.SetActive(false);
        chatNotificationActive = false;
    }

    public void EnableChat()
    {
        chatBox.gameObject.SetActive(true);
    }

    #region Private

    private void OpenChat()
    {
        ChatMinimized = false;
        chatNotificationActive = false;
        RefreshCurrentView();
    }

    private void MinimizedChat()
    {
        ChatMinimized = true;
        RefreshCurrentView();
    }

    private void RefreshCurrentView()
    {
        chatBox.SetActive(!ChatMinimized);
        openChatButton.gameObject.SetActive(ChatMinimized);
        chatMinimizedNotification.gameObject.SetActive(chatNotificationActive && ChatMinimized);
    }

    private void ErrorCommandMessage(string error)
    {
        //TODO show the error message somewhere for the player
        Debug.Log("ERROR: " + error);
    }

    private int GetUserIndexColor(string nickname)
    {
        return 0; //TODO get which user is it and put their assigned index color maybe? dictionary with current users? asign one to each new player?
    }

    private bool IsMessageValid(string message)
    {
        return (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message));
    }

    private string ColorfyNickname(string playerNickname, int playerIndex)
    {
        return $"<color = {colorNameList[playerIndex]}>{playerNickname}:</color>";
    }
    #endregion

    #region Callbacks

    public void DebugReturn(DebugLevel level, string message)
    {
        print($"Chat Debug {level}: {message}");
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

        EnableChat();

        string[] friends = new string[] { };
        _chatClient.AddFriends(friends);
        _chatClient.SetOnlineStatus(ChatUserStatus.Online);
    }

    public void OnChatStateChange(ChatState state)
    {
        OnChatStateChanged?.Invoke("Chat State: " + state.ToString());
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            int playerIndex = GetUserIndexColor(senders[i]);
            mainChatContent.text += $"{ColorfyNickname(senders[i], playerIndex)} {messages[i]} \n";
        }

        if (ChatMinimized)
        {
            chatNotificationActive = true;
            RefreshCurrentView();
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        mainChatContent.text += $"<color = {privateMessageColor}>{sender}:</color> {message} \n";

        if (ChatMinimized)
        {
            chatNotificationActive = true;
            RefreshCurrentView();
        }
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        OnChatSuscribed?.Invoke(); //TODO make main manager listen to this and change status info
        OpenChat();
    }

    public void OnUnsubscribed(string[] channels)
    {
        OnChatUnsuscribed?.Invoke(); //TODO make main manager listen to this and change status info

        for (int i = 0; i < channels.Length; i++)
        {
            if (channels[i] == _channel)
            {
                _channel = null;
                mainChatContent.text = "";
            }
        }
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        string userStatus = status == ChatUserStatus.Online ? "connected" : "disconected";
        mainChatContent.text += $"<color={statusColor}>{user} is {message} </color> \n";
    }

    public void OnUserSubscribed(string channel, string user)
    {
        _chatClient.PublishMessage(channel, $"<color={serverInfoColor}><i> {user} has entered the chat </i></color> \n");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        _chatClient.PublishMessage(channel, $"<color={serverInfoColor}><i> {user} has left the chat </i></color> \n");
    }
    #endregion
}
