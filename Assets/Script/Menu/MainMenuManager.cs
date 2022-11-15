using ParrelSync;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    private const string DEFAULT_SERVER_NAME = "Server";
    private const string DEFAULT_ROOM_NAME = "TestRoom";
    private const string DEFAULT_NICK_NAME = "Player";
    private const int DEFAULT_MAX_PLAYERS = 7; //max default is 7 instead of 6 because we have an extra that is the server 
    private const int MINIMUM_PLAYERS_FOR_GAME = 3; //minimum default is 4 instead of 3 for the same reason
    
    [Header("Config")]
    public bool isServer;
    [Tooltip("To skip login if the build is server, this has to be false")]
    public bool serverHasToLog;
    public GameObject cheats;

    [Header("Main Settings")]
    [SerializeField] private Text txtNickname;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button quitButton;
    [SerializeField] private string statusPrefix = "Status: ";
    public ChatManager chatBox;
    public string Level = "Level";

    [Header("All Panels")]
    public Panel loggingPanel;
    public Panel roomSettingPanel;
    public Panel kickedPanel;
    public Panel loadingSymbolPanel;
    public Panel joiningRoomsWaitPanel;
    public Panel chooseRoomPanel;
    public Panel roomLobbyPanel;

    [Header("WaitingToJoinRoom")]
    [SerializeField] private float timeOutSearch = 10f;

    [Header("Logging")]
    [SerializeField] private TMP_InputField nickNameInput;
    [SerializeField] private Button logInButton;

    [Header("Prompts")]
    [SerializeField] private Button kickedOutConfirmButton;

    private List<Panel> allPanels = new List<Panel>();
    private bool skipEverything; //for cheating the login
    private bool forceStart;

    //PROPIERTIES
    public MainMenuView PlayerView { get; private set; }
    public bool Kicked { get; set; }
    public int MaxPlayers { get; private set; }
    public int RealMaxPlayers => MaxPlayers - 1;
    public int MinPlayers => MINIMUM_PLAYERS_FOR_GAME;
    public string DefaultRoom => DEFAULT_ROOM_NAME;
    public string DefaultNickname => DEFAULT_NICK_NAME;
    public Panel ChoosePanel => isServer ? roomSettingPanel : chooseRoomPanel;

    public bool ChatBoxEnabled { get; set; }

    //EVENTS 
    public Action<RoomInfo> OnBannedRoom = delegate { };
    public Action OnClearData = delegate { };
    public Action<string, byte> OnBaseCreateRoom = delegate { };

    public void Awake()
    {
#if UNITY_EDITOR 
        isServer = !ClonesManager.IsClone();
        cheats.SetActive(true);
#else
        cheats.gameObject.SetActive(false);
#endif

        PlayerView = GetComponent<MainMenuView>();
        MaxPlayers = DEFAULT_MAX_PLAYERS;

        PhotonNetwork.AutomaticallySyncScene = true;

        //GeneratePanels
        GenerateWaitJoinningRoomPanel();
        txtNickname.gameObject.SetActive(false);
        quitButton.onClick.AddListener(OnQuitButton);
        logInButton.onClick.AddListener(LogInUser);
        kickedOutConfirmButton.onClick.AddListener(() => { ChangePanel(ChoosePanel); Kicked = false; });

        //Set all panels
        allPanels.Add(loggingPanel);
        allPanels.Add(roomSettingPanel);
        allPanels.Add(roomLobbyPanel);
        allPanels.Add(kickedPanel);
        allPanels.Add(loadingSymbolPanel);
        allPanels.Add(joiningRoomsWaitPanel);
        allPanels.Add(chooseRoomPanel);

        RestartMenu();
    }

    public void RestartMenu()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            ChangePanel(ChoosePanel);
            SetStatus("Connected to Lobby");
        }
        else
        {
            if (isServer && !serverHasToLog)
            {
                ServerLogIn();
                return;
            } 

            ChangePanel(loggingPanel);
            SetStatus("Please Log In");
        }
    }

    #region GeneratingPanels
    private void GenerateWaitJoinningRoomPanel()
    {
        joiningRoomsWaitPanel.OnOpen += OnOpen;
        joiningRoomsWaitPanel.OnClose += OnClose;

        void OnOpen()
        {
            StartCoroutine(JoinRandomRoomTimer(timeOutSearch));
        }

        void OnClose()
        {
        }
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            ForceServerLog();

        if (Input.GetKeyDown(KeyCode.F2))
            ForceQuickStartAsPlayer();
    }

    private IEnumerator JoinRandomRoomTimer(float timer)
    {
        SetStatus("Searching for rooms");

        yield return new WaitForSeconds(timer);

        if (!PhotonNetwork.InRoom)
        {
            ChangePanel(ChoosePanel);
            SetStatus("No rooms found");
        }
    }

    private void ForceQuickStartAsPlayer()
    {
        ChangePanel(loadingSymbolPanel);

        isServer = false;

        PhotonNetwork.NickName = DEFAULT_NICK_NAME;
        txtNickname.gameObject.SetActive(true);
        txtNickname.text = PhotonNetwork.NickName;

        PhotonNetwork.ConnectUsingSettings();

        skipEverything = true;
    }

    private void ForceServerLog()
    {
        isServer = true;
        ServerLogIn();
    }

    private void ServerLogIn()
    {
        ChangePanel(loadingSymbolPanel);
        PhotonNetwork.NickName = DEFAULT_SERVER_NAME; //TODO ADD A RANDOM NUMBER TO THE NAME?
        txtNickname.gameObject.SetActive(true);
        txtNickname.text = PhotonNetwork.NickName;
        PhotonNetwork.ConnectUsingSettings();
        skipEverything = true;

    }

    public void ChangePanel(Panel panelToOpen)
    {
        for (int i = 0; i < allPanels.Count; i++)
        {
            if (allPanels[i] == panelToOpen)
                allPanels[i].OpenPanel();
            else
                allPanels[i].ClosePanel();
        }
    }

    public void SetStatus(string message)
    {
        statusText.text = statusPrefix + message;
    }

    public void LogInUser()
    {
        if (string.IsNullOrEmpty(nickNameInput.text) || string.IsNullOrWhiteSpace(nickNameInput.text)) return;

        PhotonNetwork.NickName = nickNameInput.text;
        txtNickname.text = nickNameInput.text;

        txtNickname.gameObject.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
        ChangePanel(loadingSymbolPanel);
        SetStatus("Trying to Connect");
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        ChangePanel(joiningRoomsWaitPanel);
        SetStatus("Joining random Room");
    }

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        SetStatus("Connecting to Lobby");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (skipEverything)
        {
            if (isServer)
            {
                OnBaseCreateRoom.Invoke("", DEFAULT_MAX_PLAYERS);
                return;
            }

            ChangePanel(loadingSymbolPanel);
            PhotonNetwork.JoinRandomRoom();
            SetStatus("Searching for a random room");
            return;
        }

        if (!Kicked)
            ChangePanel(ChoosePanel);

        SetStatus("Connected to Lobby");
    }

    public override void OnCreatedRoom()
    {
        SetStatus("Created Room");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetStatus("Created Room failed");
    }

    public override void OnJoinedRoom()
    {
        if (forceStart)
        {
            PhotonNetwork.LoadLevel(Level);
            return;
        }

        SetStatus("Joined Room");
        ChangePanel(roomLobbyPanel);

        if (!ChatBoxEnabled && !isServer)
            chatBox.ConnectChat();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatus("Joined Room failed");
        ChangePanel(ChoosePanel);
    }

    public override void OnLeftRoom()
    {
        if (Kicked)
        {
            ChangePanel(kickedPanel);
            SetStatus("Kicked from room");

        }
        else
        {
            ChangePanel(ChoosePanel);
            SetStatus("Left Room");
        }
    }

    public void KickedPlayer()
    {
        Kicked = true;
        OnBannedRoom.Invoke(PhotonNetwork.CurrentRoom);
        PhotonNetwork.LeaveRoom(false);
    }
    #endregion

    private void OnQuitButton()
    {
        SetStatus("Disconnecting");
        PhotonNetwork.Disconnect();
        Application.Quit();
    }

    public void ClearData()
    {
        OnClearData.Invoke();
        skipEverything = false;
        Kicked = false;
    }
}
