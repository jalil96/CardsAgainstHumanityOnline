using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomSelectionPanel : MonoBehaviourPunCallbacks
{
    public GameObject roomListContainer;
    public Button goBackButton;
    public RoomDisplay roomPrefab;
    public Text availableRoomsNumber;
    public GameObject loadingAnimation;
    public GameObject roomListDisplay;

    private MainMenuManager mainMenu;
    private List<RoomInfo> bannedRooms = new List<RoomInfo>();
    private List<RoomInfo> currentRoomList = new List<RoomInfo>();
    private List<RoomDisplay> currentRoomButtons = new List<RoomDisplay>();

    private void Awake()
    {
        mainMenu = GetComponent<MainMenuManager>();
        mainMenu.OnBannedRoom += AddToBannedRooms;
        GenerateRoomListPanel();
    }

    private void OnDestroy()
    {
        mainMenu.OnBannedRoom -= AddToBannedRooms;
    }

    private void GenerateRoomListPanel()
    {
        mainMenu.chooseRoomPanel.OnOpen += OnOpen;
        mainMenu.chooseRoomPanel.OnClose += OnClose;

        goBackButton.onClick.AddListener(GoBack);

        roomPrefab.gameObject.SetActive(false);
        loadingAnimation.SetActive(false);

        void OnOpen()
        {
            mainMenu.SetStatus("Searching for rooms");
            RefreshRoomListDisplayed();
            ShowRoomList(currentRoomList.Count > 0);
        }

        void OnClose()
        {

        }
    }

    private void GoBack()
    {
        if (currentRoomList.Count > 0)
            mainMenu.SetStatus("Choose new option");
        else
            mainMenu.SetStatus("No rooms found");

        mainMenu.ChangePanel(mainMenu.choosePanels);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            RoomReceived(room);
        }

        if (mainMenu.chooseRoomPanel.IsOpen)
        {
            ShowRoomList(currentRoomList.Count > 0);
            RefreshRoomListDisplayed();
        }
    }

    private void ShowRoomList(bool hasRoomsAvailable)
    {
        string message = hasRoomsAvailable ? "Rooms available" : "Searching for rooms";
        mainMenu.SetStatus(message);

        availableRoomsNumber.text = currentRoomList.Count.ToString();
        roomListDisplay.SetActive(hasRoomsAvailable);
        loadingAnimation.SetActive(!hasRoomsAvailable);
    }

    private bool CheckIfRoomIsBanned(RoomInfo room)
    {
        return bannedRooms.Contains(room);
    }

    private void AddToBannedRooms(RoomInfo room)
    {
        if (CheckIfRoomIsBanned(room)) return;

        bannedRooms.Add(room);

        if (CheckRoomIsInCurrentList(room))
            currentRoomList.Remove(room);
    }

    private void AddRoomToList(RoomInfo room)
    {
        if (CheckIfRoomIsBanned(room)) return;
        if (CheckRoomIsInCurrentList(room))
        {
            for (int i = 0; i < currentRoomList.Count; i++)
            {
                if (room.Equals(currentRoomList[i]))
                    currentRoomList[i] = room;
            }
        }
        else
        {
            currentRoomList.Add(room);
        }

    }

    private void RemoveUnavailableRooms()
    {
        for (int i = currentRoomList.Count - 1; i >= 0; i--)
        {
            var room = currentRoomList[i];
            RemoveUnavailableRoom(room);
        }
    }

    private void RemoveUnavailableRoom(RoomInfo room)
    {
        if (!room.IsOpen || !room.IsVisible || room.PlayerCount >= room.MaxPlayers)
            currentRoomList.Remove(room);
    }


    private bool CheckRoomIsInCurrentList(RoomInfo room)
    {
        return currentRoomList.Contains(room);
    }

    private void RoomReceived(RoomInfo room)
    {
        if (room.IsVisible && room.PlayerCount < room.MaxPlayers)
        {
            AddRoomToList(room);
        }
        else
        {
            if (CheckRoomIsInCurrentList(room))
                RemoveUnavailableRoom(room);
        }

    }

    private void RefreshRoomListDisplayed()
    {
        int difference = currentRoomList.Count - currentRoomButtons.Count;
        if (difference > 0)
            AddNewRoom(difference);

        for (int i = 0; i < currentRoomButtons.Count; i++)
        {
            bool overflow = (currentRoomList.Count - 1) < i;

            currentRoomButtons[i].gameObject.SetActive(!overflow);

            if (overflow) continue;

            SetRoomNameInfo(currentRoomList[i], i);
        }
    }

    private void SetRoomNameInfo(RoomInfo room, int index)
    {
        RoomDisplay currentDisplay = currentRoomButtons[index];
        currentDisplay.nameTxt.text = room.Name;
        currentDisplay.numberTxt.text = $"{room.PlayerCount} / {room.MaxPlayers} ";
        currentDisplay.RoomInfo = room;
        currentDisplay.joinButton.onClick.AddListener(() => OnClickRoom(currentDisplay));
    }

    private void AddNewRoom(int roomsToAdd)
    {
        for (int i = 0; i < roomsToAdd; i++)
        {
            RoomDisplay aux = Instantiate(roomPrefab, roomListContainer.transform);
            aux.gameObject.SetActive(false);
            currentRoomButtons.Add(aux);
        }
    }
    private void OnClickRoom(RoomDisplay roomDisplay)
    {
        if (!roomDisplay.RoomInfo.IsOpen)
        {
            mainMenu.SetStatus($"Room {roomDisplay.RoomInfo.Name} is full");
            return;
        }
        if (PhotonNetwork.JoinRoom(roomDisplay.RoomInfo.Name))
        {
            mainMenu.SetStatus("Player Joined in the Room");
            return;
        }

        Debug.Log("Failed to join in the room, please fix the error!");
    }
}
