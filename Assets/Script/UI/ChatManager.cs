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
using UnityEngine.SocialPlatforms;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    [Header("Chat References")]
    public GameObject chatBox;
    public GameObject chatMinimizedNotification;
    public TextMeshProUGUI chatMinimizedCounter;
    public TextMeshProUGUI mainChatContent;
    public TextMeshProUGUI privateChatContent;
    public TMP_InputField inputField;
    public PrivateChatButton mainChatButton;
    public GameObject chatPrivateButtonsContainer;

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
    public Color errorColor = Color.red;
    private string errorHexColor;

    //private variables
    private ChatClient _chatClient;
    private string _channel;
    private Dictionary<string, int> playersColorDictionary = new Dictionary<string, int>();
    private Dictionary<string, PrivateChatButton> privateChatsButtons = new Dictionary<string, PrivateChatButton>();
    private Dictionary<PrivateChatButton, string> privateUserButtons = new Dictionary<PrivateChatButton, string>();
    private string[] colorHex;
    private List<string> currentUsers = new List<string>();
    private int currentNumberOfNewMessages = 0;
    private PrivateChatButton currentChat;
    private TextMeshProUGUI currentTextChat;

    //PROPIEDADES
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

        minimizedChat.onClick.AddListener(ToggleChat);
        sendButton.onClick.AddListener(SendChatMessage);
        inputField.onEndEdit.AddListener(SendChatMessage);

        mainChatButton.AssingAsMain(mainChatContent);
        
        //let's set up everything
        privateChatContent.gameObject.SetActive(false);
        mainChatContent.gameObject.SetActive(true);
        privateChatContent.text = "";
        mainChatContent.text = "";
        currentTextChat = mainChatContent;
        currentChat = mainChatButton;

        SetColors();
        DisableChat();
    }

    private void SuscribeEvents()
    {
		CommunicationsManager.Instance.commandManager.PrivateMessageCommand += OpenAPrivateChat;
        CommunicationsManager.Instance.commandManager.ErrorCommand += ErrorCommandMessage;
        CommunicationsManager.Instance.commandManager.HelpCommand += HelpCommandMessage;
        CommunicationsManager.Instance.OnColorsUpdate += UpdateColorDictionary;
    }

    private void OnDestroy()
    {
        if (!CommunicationsManager.HasInstance) return;

        CommunicationsManager.Instance.commandManager.PrivateMessageCommand -= OpenAPrivateChat;
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
        SetMainChatInFocus();
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

        if (!CommunicationsManager.Instance.commandManager.IsCommand(message))
        {
            if (currentChat == mainChatButton) //we check which chat is open, depending if it's the main chat or one of the privat chats is HOW we send the message
                _chatClient.PublishMessage(_channel, message);
            else
                SendPrivateChatMessage(message);
        }

        inputField.text = "";
        inputField.ActivateInputField();
        inputField.Select();
    }

    private void OpenAPrivateChat(string nickname)
    {
        string text = "";
        if (!IsInUserList(nickname))
        {
            text = $"{ColorfyWords($"ERROR: User '{nickname}' was not found", errorHexColor)} \n";
            UpdateText(text);
            return;
        }

        text = $"{ColorfyWords($"A private chat with {nickname}' was open, say 'Hi'", serverHexColor)} \n";
        UpdateChats(nickname, text, true);
    }

    public void SendPrivateChatMessage(string message)
    {
        //UpdateChats(currentChat.nickname, message);
        _chatClient.SendPrivateMessage(currentChat.nickname, message);
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

    public void SetMainChatInFocus()
    {
        if (currentTextChat == mainChatContent) return;

        SwitchCurrentChat(mainChatButton);
        SwitchTextBox(mainChatContent);
    }

    public void SetPrivateChatInFocus(PrivateChatButton chat)
    {
        SwitchTextBox(privateChatContent);
        SwitchCurrentChat(chat);
    }

    public void UpdateText(string message)
    {
        currentTextChat.text += message;
    }

    #region Private

    private void SwitchTextBox(TextMeshProUGUI newText)
    {
        if(currentTextChat == newText) return;

        currentTextChat.gameObject.SetActive(false);
        currentTextChat = newText;
        currentTextChat.gameObject.SetActive(true);
    }

    private void SwitchCurrentChat(PrivateChatButton chat)
    {
        if (currentChat == chat) return;

        currentChat.HideChat();
        currentChat = chat;
        currentChat.ShowChat();
    }

    private void UpdateChats(string nickname, string newText, bool forceShow = false)
    {
        PrivateChatButton chatButton = GetPrivateChat(nickname, forceShow);

        chatButton.UpdateText(newText);
    }

    private PrivateChatButton GetPrivateChat(string nickname, bool forceShow = false)
    {
        PrivateChatButton chatButton = null;

        if (!privateChatsButtons.TryGetValue(nickname, out chatButton))
        {
            print("Opening a new chat");
            chatButton = CreateNewPrivateChat(nickname);
        }


        if (forceShow)
            SetPrivateChatInFocus(chatButton);

        return chatButton;
    }

    private PrivateChatButton CreateNewPrivateChat(string nickname)
    {
        PrivateChatButton newButton = Instantiate(mainChatButton, chatPrivateButtonsContainer.transform);
        newButton.chatText = "";
        newButton.AssignUser(nickname, privateChatContent);
        privateChatsButtons[nickname] = newButton;
        privateUserButtons[newButton] = nickname;
        return newButton;
    }

    private void CloseChat(PrivateChatButton chatbutton)
    {
        if (privateUserButtons.TryGetValue(chatbutton, out string nickname))
        {
            SwitchCurrentChat(mainChatButton);
            privateUserButtons.Remove(chatbutton);
            privateChatsButtons.Remove(nickname);
        }
    }

    public void CloseChat(string nickname)
    {
        if (privateChatsButtons.TryGetValue(nickname, out PrivateChatButton chatbutton))
        {
            SwitchCurrentChat(mainChatButton);
            privateUserButtons.Remove(chatbutton);
            privateChatsButtons.Remove(nickname);
        }
    }

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
        var text = $"{ColorfyWords($"ERROR: {error}", errorHexColor)} \n";
        UpdateText(text);
    }

    private void HelpCommandMessage(string help)
    {
        //var text = $"{ColorfyWords($"{help}", serverHexColor)} \n";
        UpdateText(help);
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

    private void SetColors()
    {
        colorHex = new string[colorNameList.Count];
        for (int i = 0; i < colorNameList.Count; i++) //in the beginning all colors all available
        {
            colorHex[i] = ColorUtility.ToHtmlStringRGBA(colorNameList[i]);
        }

        statusHexColor = ColorUtility.ToHtmlStringRGBA(statusColor);
        serverHexColor = ColorUtility.ToHtmlStringRGBA(serverInfoColor);
        privateHexColor = ColorUtility.ToHtmlStringRGBA(privateMessageColor);
        errorHexColor = ColorUtility.ToHtmlStringRGB(errorColor);
    }

    private string ColorfyWords(string wordsToColor, string hex)
    {
        return $"<color=#{hex}>{wordsToColor}</color>";
    }

    private bool IsInUserList(string nickname)
    {
        var playerList = PhotonNetwork.PlayerList.ToList();

        for (int i = 0; i < playerList.Count; i++)
        {
            if (nickname == playerList[i].NickName)
                return true;
        }
        return false;
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
        //MasterVoiceManager.Instance.RequestRemoveFromColorList(PhotonNetwork.LocalPlayer);
        ChatEnabled = false;
    }

    public void OnConnected()
    {
        OnChatConnected?.Invoke(); //TODO make main manager listen to this and change status info

        _channel = PhotonNetwork.CurrentRoom.Name;

        _chatClient.Subscribe(_channel);

        //MasterVoiceManager.Instance.RequestAddToColorList(PhotonNetwork.LocalPlayer);

        var currentPlayerList = PhotonNetwork.PlayerList.ToList();
        List<string> newFriends = new List<string>();

        for (int i = 0; i < currentPlayerList.Count; i++)
        {
            if (currentPlayerList[i].IsMasterClient) continue;
            newFriends.Add(currentPlayerList[i].NickName);
        }

        if (newFriends.Count > 0)
        {
            string[] friends = newFriends.ToArray();
            _chatClient.AddFriends(friends);
        }

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
            var message = $"{ColorfyWords($"{senders[i]}:", colorHex[playerIndex])} {messages[i]} \n";
            mainChatButton.UpdateText(message);

            if (ChatMinimized)
            {
                currentNumberOfNewMessages++;
                RefreshCurrentView();
            }
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        var newMessage = $"{ColorfyWords($"{sender}:", privateHexColor)} {message} \n";
        string channel = sender;

        if(sender == PhotonNetwork.LocalPlayer.NickName)
             channel = channelName.Remove(0, sender.Length + 1);

        UpdateChats(channel, newMessage);

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
        //string userStatus = status == ChatUserStatus.Online ? "connected" : "disconected";
        //var text = $"{ColorfyWords($"{user} is {userStatus}", statusHexColor)} \n";
        //mainChatButton.UpdateText(text);
    }

    public void OnUserSubscribed(string channel, string user)
    {
        if (!IsInUserList(user))
        {

        }
        string[] friends = new string[] { user };
        _chatClient.AddFriends(friends);

        var text = ColorfyWords($"{user} has entered the chat", serverHexColor);
        _chatClient.PublishMessage(channel, $"{text}\n");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        var text = ColorfyWords($"{user} has left the chat", serverHexColor);
        _chatClient.PublishMessage(channel, $"{text}\n");
        
        CloseChat(user);
    }
    #endregion
}
