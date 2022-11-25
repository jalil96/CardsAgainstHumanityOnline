using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InRoomPanel : MonoBehaviourPunCallbacks
{
    public string roomPrefix = "Room: ";

    [Header("Reference")]
    public PlayerDisplay playerListPrefab;
    public GameObject playerListContainer;
    public GameObject waitingHostSpecialOptions;
    public GameObject waitingNonHostOptions;

    [Header("Texts")]
    public Text roomNameWaitingLobbyTxt;
    public Text currentNumberPlayersTxt;

    [Header("Buttons")]
    public Button startGameButton;
    public Button leaveRoomButton;

    private MainMenuManager mainMenu;
    private List<PlayerDisplay> playerButtons = new List<PlayerDisplay>();
    private Dictionary<string, int> playerNames = new Dictionary<string, int>();
    private List<Player> currentPlayerList = new List<Player>();
    private bool roomOpen;

    void Awake()
    {
        mainMenu = GetComponent<MainMenuManager>();
        mainMenu.OnClearData += ClearData;
        GenerateWaitingPanel();
    }

    private void OnDestroy()
    {
        mainMenu.OnClearData -= ClearData;
    }

    private void GenerateWaitingPanel()
    {
        mainMenu.roomLobbyPanel.OnOpen += OnOpen;

        leaveRoomButton.onClick.AddListener(LeaveTheRoom);

        waitingHostSpecialOptions.SetActive(false);
        waitingNonHostOptions.SetActive(false);

        //Let´s create the max of buttons needed in the game
        playerListPrefab.gameObject.SetActive(false);
        for (int i = 0; i < mainMenu.RealMaxPlayers; i++)
        {
            PlayerDisplay aux = Instantiate(playerListPrefab, playerListContainer.transform);
            aux.gameObject.SetActive(false);
            playerButtons.Add(aux);
        }

        void OnOpen()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                waitingHostSpecialOptions.SetActive(true);
                waitingNonHostOptions.SetActive(false);
                startGameButton.onClick.AddListener(StartGame);
                startGameButton.interactable = false;
            }
            else
            {
                waitingHostSpecialOptions.SetActive(false);
                waitingNonHostOptions.SetActive(true);
                startGameButton.gameObject.SetActive(false);
            }

            roomNameWaitingLobbyTxt.text = roomPrefix + PhotonNetwork.CurrentRoom.Name;
            RefreshPlayerList();
        }
    }

    public void RefreshPlayerList() //we susbtract everywhere a player because the master client shouldn't be seen
    {
        currentNumberPlayersTxt.text = $"{PhotonNetwork.CurrentRoom.PlayerCount - 1}/{PhotonNetwork.CurrentRoom.MaxPlayers - 1}";

        currentPlayerList = PhotonNetwork.PlayerList.ToList();
        var server = PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.CurrentRoom.MasterClientId);
        currentPlayerList.Remove(server);

        CheckNicknames(currentPlayerList);

        if (PhotonNetwork.IsMasterClient)
            startGameButton.interactable = HasEnoughPlayers();

        for (int i = 0; i < playerButtons.Count; i++)
        {
            bool overflow = (currentPlayerList.Count - 1) < i;

            playerButtons[i].gameObject.SetActive(!overflow);

            if (overflow) continue;

            SetPlayerButton(currentPlayerList[i], i);
        }
    }

    private void SetPlayerButton(Player currentPlayer, int index)
    {
        playerButtons[index].nameTxt.text = currentPlayer.NickName;
        playerButtons[index].numberTxt.text = $"{index + 1} - ";

        playerButtons[index].kickButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        if (PhotonNetwork.IsMasterClient)
            playerButtons[index].kickButton.onClick.AddListener(() => OnKickPlayer(currentPlayer));
    }

    public void CheckNicknames(List<Player> players)
    {
        playerNames.Clear();
        for (int i = 0; i < players.Count; i++)
        {
            if (playerNames.TryGetValue(players[i].NickName, out int number))
            {
                players[i].NickName = $"{players[i].NickName}({number})";
                number++;
                playerNames[players[i].NickName] = number;
            }
            else
                playerNames.Add(players[i].NickName, 1);
        }
    }

    private bool HasEnoughPlayers()
    {
        bool hasEnough = PhotonNetwork.CurrentRoom.PlayerCount >= mainMenu.MinPlayers;

        if (hasEnough)
            mainMenu.SetStatus("Ready to start");
        else
            mainMenu.SetStatus("Waiting for more players");

        return hasEnough;
    }

    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        CloseRoom();
        mainMenu.ClearData();
        PhotonNetwork.LoadLevel(mainMenu.Level);
    }

    private void OnKickPlayer(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        mainMenu.PlayerView.OnKickPlayer(newPlayer);
        mainMenu.SetStatus("Being kicked");
    }

    public void LeaveTheRoom()
    {
        LeaveTheRoom(false);
    }

    public void LeaveTheRoom(bool skipKick = false)
    {
        if (!PhotonNetwork.InRoom) return;

        if (PhotonNetwork.IsMasterClient && !skipKick)
        {
            currentPlayerList = PhotonNetwork.PlayerList.ToList();

            for (int i = currentPlayerList.Count - 1; i >= 0; i--)
            {
                if (!currentPlayerList[i].IsMasterClient)
                    OnKickPlayer(currentPlayerList[i]);
            }

            CloseRoom();
        }

        PhotonNetwork.LeaveRoom(false);

        mainMenu.OnLeftRoom();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        LeaveTheRoom(true);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayerList();
        UpdateRoomFullness();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayerList();
        UpdateRoomFullness();
    }

    public void UpdateRoomFullness()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            CloseRoom();
        else
            OpenRoom();
    }

    public void OpenRoom()
    {
        if (roomOpen) return;
        roomOpen = true;
        PhotonNetwork.CurrentRoom.IsVisible = true;
        PhotonNetwork.CurrentRoom.IsOpen = true;
    }

    public void CloseRoom()
    {
        if (!roomOpen) return;
        roomOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    private void OnApplicationQuit()
    {
        if (PhotonNetwork.InRoom)
            LeaveTheRoom();
    }

    private void ClearData()
    {
        playerNames.Clear();
    }
}
