using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Chat;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Voice;
using JetBrains.Annotations;
using System.Linq;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    [Header("Chat References")]
    public GameObject chatBox;
    public GameObject chatMinimizedNotification;
    public TextMeshProUGUI chatMinimizedCounter;
    public TextMeshProUGUI mainChatContent;
    public TMP_InputField inputField;

    [Header("Buttons")]
    public Button minimizedChat;
    public Button sendButton;

    [Header("Colors")]
    public List<Color> colorNameList = new List<Color>();

    [Header("SpecialColors")]
    public Color privateMessageColor = Color.yellow;
    private string privateHexColor;
    public Color serverInfoColor = Color.grey;
    private string serverHexColor;
    public Color statusColor = Color.green;
    private string statusHexColor;

    //private variables
    private ChatClient _chatClient;
    private string _channel;
    private Dictionary<string, int> playersColorDictionary = new Dictionary<string, int>();
    private string[] colorHex;

    public bool ChatEnabled { get; set; }
    public bool ChatMinimized { get; private set; }

    private int currentNumberOfNewMessages = 0;

    //EVENTS
    public Action OnChatConnected;
    public Action OnChatDisconected;

    public Action OnChatSuscribed;
    public Action OnChatUnsuscribed;
    public Action<string> OnChatStateChanged;

    private void Awake()
    {
        DisableChat();

        mainChatContent.text = "";

        minimizedChat.onClick.AddListener(ToggleChat);

        sendButton.onClick.AddListener(SendChatMessage);
        inputField.onEndEdit.AddListener(SendChatMessage);

        colorHex = new string[colorNameList.Count];
        for (int i = 0; i < colorNameList.Count; i++) //in the beginning all colors all available
        {
            colorHex[i] = ColorUtility.ToHtmlStringRGBA(colorNameList[i]);
        }

        statusHexColor = ColorUtility.ToHtmlStringRGBA(statusColor);
        serverHexColor = ColorUtility.ToHtmlStringRGBA(serverInfoColor);
        privateHexColor = ColorUtility.ToHtmlStringRGBA(privateMessageColor);
    }

    private void SuscribeEvents()
    {
		CommunicationsManager.Instance.commandManager.PrivateMessageCommand += SendPrivateChatMessage;
        CommunicationsManager.Instance.commandManager.ErrorCommand += ErrorCommandMessage;
        CommunicationsManager.Instance.OnColorsUpdate += UpdateColorDictionary;
    }

    private void OnDestroy()
    {
        if (!CommunicationsManager.HasInstance) return;

        CommunicationsManager.Instance.commandManager.PrivateMessageCommand -= SendPrivateChatMessage;
        CommunicationsManager.Instance.commandManager.ErrorCommand -= ErrorCommandMessage;
        CommunicationsManager.Instance.OnColorsUpdate -= UpdateColorDictionary;
    }

    public void ConnectChat()
    {
		SuscribeEvents();
		
        _chatClient = new ChatClient(this);

        AuthenticationValues auth = new AuthenticationValues(PhotonNetwork.NickName);
        _chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, auth);

        EnableChat();
    }

    public void DisconnectChat()
    {

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
        if (CommunicationsManager.Instance.commandManager.IsCommand(message)) return;

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

    public void DisableChat()
    {
        ChatEnabled = false;

        if(_channel != null)
        {
            UnsuscribeFromRoom(_channel);
            _channel = null;
        }

        chatBox.gameObject.SetActive(false);
        minimizedChat.gameObject.SetActive(false);
        currentNumberOfNewMessages = 0;
    }

    public void EnableChat()
    {
        ChatEnabled = true;
        minimizedChat.gameObject.SetActive(true);
        MinimizedChat();
    }

    #region Private
    private void UpdateColorDictionary(Dictionary<string, int> newDictionary)
    {
        playersColorDictionary = newDictionary;
    }

    private void OpenChat()
    {
        ChatMinimized = false;
        currentNumberOfNewMessages = 0;
        RefreshCurrentView();
    }

    private void MinimizedChat()
    {
        ChatMinimized = true;
        RefreshCurrentView();
    }
	
	private void ToggleChat()
	{
		if(ChatMinimized)
			OpenChat();
		else
			MinimizedChat();
	}
	
    private void RefreshCurrentView()
    {
        chatBox.SetActive(!ChatMinimized);

        bool hasNewMessages = currentNumberOfNewMessages > 0;

        chatMinimizedCounter.text = hasNewMessages ? currentNumberOfNewMessages.ToString() : "...";
        chatMinimizedNotification.gameObject.SetActive(hasNewMessages);
    }

    private void ErrorCommandMessage(string error)
    {
        //TODO show the error message somewhere for the player
        Debug.Log("ERROR: " + error);
    }

    private int GetUserIndexColor(string nickname)
    {
        if (playersColorDictionary.TryGetValue(nickname, out int index))
            return index;

        return 0;
    }


    private bool IsMessageValid(string message)
    {
        return !(string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message));
    }

    private string ColorfyWords(string wordsToColor, string hex)
    {
        return $"<color=#{hex}>{wordsToColor}</color>";
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

        //      var currentPlayerList = PhotonNetwork.PlayerList.ToList();
        //      string[] friends = new string[currentPlayerList.Count];

        //      for (int i = 0; i < currentPlayerList.Count; i++)
        //      {
        //          if (currentPlayerList[i].IsMasterClient) continue;

        //          AssingNewColorFromList(currentPlayerList[i].NickName);
        //          friends[i] = currentPlayerList[i].NickName;
        //      }

        //if(friends.Length > 0)
        //	_chatClient.AddFriends(friends);

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
            mainChatContent.text += $"{ColorfyWords($"{senders[i]}:", colorHex[playerIndex])} {messages[i]} \n";
        }

        if (ChatMinimized)
        {
            currentNumberOfNewMessages++;
            RefreshCurrentView();
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        mainChatContent.text += $"{ColorfyWords($"{sender}:", privateHexColor)} {message} \n";

        if (ChatMinimized)
        {
            currentNumberOfNewMessages++;
            RefreshCurrentView();
        }
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        OnChatSuscribed?.Invoke(); //TODO make main manager listen to this and change status info
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
        var text = $"{user} is {userStatus}";
        mainChatContent.text += $"{ColorfyWords(text, statusHexColor)} \n";
    }

    public void OnUserSubscribed(string channel, string user)
    {
        string[] friends = new  string[] {user};
        _chatClient.AddFriends(friends);

        var text = ColorfyWords($"{user} has entered the chat", serverHexColor);
        _chatClient.PublishMessage(channel, $"{text}\n");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        var text = ColorfyWords($"{user} has left the chat", serverHexColor);
        _chatClient.PublishMessage(channel, $"{text}\n");
    }
    #endregion
}
