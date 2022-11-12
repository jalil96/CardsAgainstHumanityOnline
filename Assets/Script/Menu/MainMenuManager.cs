using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    private const string DEFAULT_ROOM_NAME = "TestRoom";
    private const string DEFAULT_NICK_NAME = "TestUser";
    private const int DEFAULT_MAX_PLAYERS = 6;
    private const int MINIMUM_PLAYERS_FOR_GAME = 3;

    [Header("Main Settings")]
    [SerializeField] private Text txtNickname;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button quitButton;
    [SerializeField] private string statusPrefix = "Status: ";
    public ChatManager chatBox;
    public string Level = "Level";

    [Header("All Panels")]
    public Panel loggingPanel;
    public Panel choosePanels;
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

    [Header("Choose")]
    [SerializeField] private Button newRoomButton;
    [SerializeField] private Button joinRoomButton;

    [Header("Prompts")]
    [SerializeField] private Button kickedOutConfirmButton;

    private List<Panel> allPanels = new List<Panel>();
    private bool skipEverything; //for cheating the login
    private bool forceStart;

    //PROPIERTIES
    public MainMenuView PlayerView { get; private set; }
    public bool Kicked { get; set; }
    public int MaxPlayers { get; private set; }
    public int MinPlayers => MINIMUM_PLAYERS_FOR_GAME;
    public string DefaultRoom => DEFAULT_ROOM_NAME;
    public string DefaultNickname => DEFAULT_NICK_NAME;

    //EVENTS 
    public Action<RoomInfo> OnBannedRoom = delegate { };
    public Action OnClearData = delegate { };
    public Action<string, byte> OnBaseCreateRoom = delegate { };

    public void Awake()
    {
        PlayerView = GetComponent<MainMenuView>();
        MaxPlayers = DEFAULT_MAX_PLAYERS;

        PhotonNetwork.AutomaticallySyncScene = true;

        //GeneratePanels
        GenerateChoosingPanel();
        GenerateWaitJoinningRoomPanel();
        txtNickname.gameObject.SetActive(false);
        quitButton.onClick.AddListener(OnQuitButton);
        logInButton.onClick.AddListener(LogInUser);
        kickedOutConfirmButton.onClick.AddListener(() => { ChangePanel(choosePanels); Kicked = false; });

        //Set all panels
        allPanels.Add(loggingPanel);
        allPanels.Add(choosePanels);
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
            ChangePanel(choosePanels);
            SetStatus("Connected to Lobby");
        }
        else
        {
            ChangePanel(loggingPanel);
            SetStatus("Please Log In");
        }
    }

    #region GeneratingPanels
    private void GenerateChoosingPanel()
    {
        joinRoomButton.onClick.AddListener(() => ChangePanel(chooseRoomPanel));
        newRoomButton.onClick.AddListener(() => ChangePanel(roomSettingPanel));
    }

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
            QuickMatchCheat();

        if (Input.GetKeyDown(KeyCode.F2))
            ForceStart();
    }

    private IEnumerator JoinRandomRoomTimer(float timer)
    {
        SetStatus("Searching for rooms");

        yield return new WaitForSeconds(timer);

        if (!PhotonNetwork.InRoom)
        {
            ChangePanel(choosePanels);
            SetStatus("No rooms found");
        }
    }

    private void QuickMatchCheat()
    {
        ChangePanel(loadingSymbolPanel);
        
        PhotonNetwork.NickName = DEFAULT_NICK_NAME;
        txtNickname.gameObject.SetActive(true);
        txtNickname.text = PhotonNetwork.NickName;

        PhotonNetwork.ConnectUsingSettings();

        skipEverything = true;
    }

    private void ForceStart()
    {
        forceStart = true;
        QuickMatchCheat();
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
            OnBaseCreateRoom.Invoke("", DEFAULT_MAX_PLAYERS);
            SetStatus("Getting Room");
            return;
        }

        if (!Kicked)
            ChangePanel(choosePanels);

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
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatus("Joined Room failed");
        ChangePanel(choosePanels);
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
            ChangePanel(choosePanels);
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
