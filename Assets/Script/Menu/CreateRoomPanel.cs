using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomNameInput;
    public Slider maxPlayersSlider;
    public Text txtMaxPlayersValue;
    public Button createRoomButton;

    private MainMenuManager mainMenu;

    void Start()
    {
        mainMenu = GetComponent<MainMenuManager>();
        mainMenu.OnBaseCreateRoom += BaseCreateRoom;
        GenerateCreateRoomPanel();
    }

    private void OnDestroy()
    {
        mainMenu.OnBaseCreateRoom -= BaseCreateRoom;
    }

    private void GenerateCreateRoomPanel()
    {
        maxPlayersSlider.maxValue = mainMenu.RealMaxPlayers;
        maxPlayersSlider.minValue = mainMenu.MinPlayers;

        maxPlayersSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        createRoomButton.onClick.AddListener(CreateRoom);

        ValueChangeCheck();

        mainMenu.roomSettingPanel.OnOpen += OnOpenPanel;
    }

    private void OnOpenPanel()
    {
        ClearData();
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text) || string.IsNullOrWhiteSpace(roomNameInput.text)) return;

        BaseCreateRoom(roomNameInput.text, (byte)(maxPlayersSlider.value + 1)); //we add one extra for the server space
        mainMenu.ChangePanel(mainMenu.loadingSymbolPanel);
    }

    private void BaseCreateRoom(string roomName = "", byte maxPlayers = 7)
    {
        if (string.IsNullOrEmpty(roomName) || string.IsNullOrWhiteSpace(roomName))
            roomName = mainMenu.DefaultRoom;

        RoomOptions options = new RoomOptions();

        options.MaxPlayers = maxPlayers;
        options.IsOpen = true;
        options.IsVisible = true;

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public void ValueChangeCheck()
    {
        txtMaxPlayersValue.text = maxPlayersSlider.value.ToString();
    }

    public void ClearData()
    {
        maxPlayersSlider.value = mainMenu.MinPlayers;
        roomNameInput.text = "";
        ValueChangeCheck();
    }
}
