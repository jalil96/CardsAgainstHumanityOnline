using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

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

    [Header("ScrollView Chat")]
    public ScrollRect chatScrollRect;

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
    private PrivateChatButton currentChatButton;
    private TextMeshProUGUI currentTextChat;

    //Special Message in chat 
    private string secretCommandCode = "654a21dasd654";
    private string secretCommandPrefix = "#";
    private string FullSecretCommandCode => secretCommandPrefix+secretCommandCode;

    //PROPIEDADES
    public bool ChatEnabled { get; set; }
    public bool ChatMinimized { get; private set; }
    public bool IsBeingUsed => !ChatMinimized && inputField.text.Length > 0;

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
        //inputField.onEndEdit.AddListener(SendChatMessage);

        mainChatButton.AssingAsMain(mainChatContent);
        
        //let's set up everything
        privateChatContent.gameObject.SetActive(false);
        mainChatContent.gameObject.SetActive(true);
        privateChatContent.text = "";
        mainChatContent.text = "";
        currentTextChat = mainChatContent;
        currentChatButton = mainChatButton;

        SetColors();
        DisableChat();
    }

    private void SuscribeEvents()
    {
		CommunicationsManager.Instance.commandManager.PrivateMessageCommand += OpenPrivateChatCommand;
        CommunicationsManager.Instance.commandManager.ErrorCommand += ErrorCommandMessage;
        CommunicationsManager.Instance.commandManager.HelpCommand += HelpCommandMessage;
        CommunicationsManager.Instance.commandManager.InfoCommand += SendCommandMessage;
        CommunicationsManager.Instance.OnColorsUpdate += UpdateColorDictionary;
        CommunicationsManager.Instance.commandManager.QuitChat += OnQuitCommand;
    }


    private void OnDestroy()
    {
        if (!CommunicationsManager.HasInstance) return;

        CommunicationsManager.Instance.commandManager.PrivateMessageCommand -= OpenPrivateChatCommand;
        CommunicationsManager.Instance.commandManager.ErrorCommand -= ErrorCommandMessage;
        CommunicationsManager.Instance.commandManager.HelpCommand -= HelpCommandMessage;
        CommunicationsManager.Instance.commandManager.InfoCommand -= SendCommandMessage;
        CommunicationsManager.Instance.OnColorsUpdate -= UpdateColorDictionary;
        CommunicationsManager.Instance.commandManager.QuitChat -= OnQuitCommand;
    }

    private void Update()
    {
        if (!ChatEnabled) return;

        _chatClient.Service();
    }

    #region Send Messages
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
            if (currentChatButton == mainChatButton) //we check which chat is open, depending if it's the main chat or one of the privat chats is HOW we send the message
                _chatClient.PublishMessage(_channel, message);
            else
                SendPrivateChatMessage(message);
        }

        inputField.text = "";
        SelectInputField();
    }


    public void SendPrivateChatMessage(string message)
    {
        _chatClient.SendPrivateMessage(currentChatButton.nickname, message);
    }

    #endregion

    #region Commands
    private void HelpCommandMessage(string help)
    {
        UpdateText(help);
    }

    private void ErrorCommandMessage(string error)
    {
        var text = $"{ColorfyWords($"ERROR: {error}", errorHexColor)} \n";
        UpdateText(text);
    }

    public void OpenPrivateChatCommand(string nickname)
    {
        if (CommunicationsManager.Instance.SentMessageOpenAChat(nickname))
            OpenAPrivateChat(nickname);
        else
            ErrorCommandMessage("Something went wrong, coulnd't open a private chat");
    }

    public void OpenAPrivateChat(string nickname)
    {
        string text = $"{ColorfyWords($"A private chat with {nickname}' was open, say 'Hi'", serverHexColor)} \n";
        UpdateChats(nickname, text, true);
        SelectInputField();
    }

    public void SendCommandMessage(string message)
    {
        string newMessage = $"{FullSecretCommandCode} {message}";
        _chatClient.PublishMessage(_channel, newMessage);
    }

    private bool ValidateIsACommandMessage(string message)
    {
        if (!message.StartsWith(secretCommandPrefix)) return false;
        string[] words = message.Split(' ');
        return words[0] == FullSecretCommandCode;
    }

    private void OnQuitCommand()
    {
        if(currentChatButton == mainChatButton) //if it's the main chat, we just minimize it. Else we close the chat
            MinimizedChat();
        else
            CloseChat(currentChatButton);
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

    #endregion

    #region Public

    public void ConnectChat()
    {
        SuscribeEvents();

        _chatClient = new ChatClient(this);

        AuthenticationValues auth = new AuthenticationValues(PhotonNetwork.NickName);
        _chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, auth);

        EnableChat();
        SetMainChatInFocus();
    }

    public void EnableChat()
    {
        ChatEnabled = true;
        minimizedChat.gameObject.SetActive(true);
        MinimizedChat();
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

    #endregion

    #region Minimize / Open, Input Field and Refresh View
    private void SelectInputField()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        chatScrollRect.verticalScrollbar.value = 0f;
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    private void OpenChat()
    {
        ChatMinimized = false;
        currentNumberOfNewMessages = 0;
        RefreshCurrentView();
        SelectInputField();
    }

    public void MinimizedChat()
    {
        ChatMinimized = true;
        RefreshCurrentView();
    }

    private void ToggleChat()
    {
        if (ChatMinimized)
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




    #endregion

    #region Private
    private void UpdateText(string message)
    {
        currentTextChat.text += message;

        StartCoroutine(ScrollToBottom());
    }

    private void UpdateChats(string nickname, string newText, bool forceShow = false)
    {
        PrivateChatButton chatButton = GetPrivateChat(nickname, forceShow);

        chatButton.UpdateText(newText);
    }

    private void SwitchTextBox(TextMeshProUGUI newText)
    {
        if(currentTextChat == newText) return;

        currentTextChat.gameObject.SetActive(false);
        currentTextChat = newText;
        currentTextChat.gameObject.SetActive(true);
    }

    private void SwitchCurrentChat(PrivateChatButton chat)
    {
        if (currentChatButton == chat) return;

        currentChatButton.HideChat();
        currentChatButton = chat;
        currentChatButton.ShowChat();
    }

    private PrivateChatButton GetPrivateChat(string nickname, bool forceShow = false) //Will return a private chat for the user, if there is no existing chat, will create a new one
    {
        PrivateChatButton chatButton = null;

        if (!privateChatsButtons.TryGetValue(nickname, out chatButton))
            chatButton = CreateNewPrivateChat(nickname);

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


    private void UnsuscribeFromRoom(string roomName)
    {
        string[] roomsList = new string[] { roomName };

        _chatClient.Unsubscribe(roomsList);

    }


    private void UpdateColorDictionary(Dictionary<string, int> newDictionary)
    {
        playersColorDictionary = newDictionary;
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
            string currentMessage = messages[i].ToString();
            string message = "";
            if (ValidateIsACommandMessage(currentMessage))
            {
                currentMessage = currentMessage.Remove(0, (secretCommandPrefix.Length + secretCommandCode.Length)); //we remove the secret code we use to identify the mute info
                message = $"<align=\"right\">{currentMessage}</align> \n";
            }
            else
            {
                int playerIndex = GetUserIndexColor(senders[i]);
                message = $"{ColorfyWords($"{senders[i]}:", colorHex[playerIndex])} {messages[i]} \n";
            }

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
