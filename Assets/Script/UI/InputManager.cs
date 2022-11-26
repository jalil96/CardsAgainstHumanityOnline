using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public KeyCode muteKey = KeyCode.M;
    public KeyCode escKey = KeyCode.Escape;

    private ChatManager chat;
    private VoiceManager voiceManager;
    private bool isMainMenu;
    private bool isGameplay;

    public Action InputServer = delegate { };
    public Action<string> InputPlayer = delegate { };


    void Start()
    {
        chat = CommunicationsManager.Instance.chatManager;
        voiceManager = CommunicationsManager.Instance.voiceManager;
    }


    public void SetSceneAsMenu()
    {
        isMainMenu = true;
        isGameplay = false;
        CommunicationsManager.Instance.commandManager.IsGameplay(isGameplay);
    }

    public void SetSceneAsGameplay()
    {
        isMainMenu = false;
        isGameplay = true;
        CommunicationsManager.Instance.commandManager.IsGameplay(isGameplay);
    }

    public void SetSceneAsScore()
    {
        isMainMenu = false;
        isGameplay = false;
        CommunicationsManager.Instance.commandManager.IsGameplay(isGameplay);
    }

    void Update()
    {
        if (Input.GetKeyDown(escKey))
        {
            if (voiceManager.VoiceSettingsIsOpen)
                voiceManager.ToggleVoiceSettingsMenu();
            else if (!chat.ChatMinimized)
                chat.MinimizedChat();
        }

        if (Input.GetKeyDown(muteKey))
        {
            if (!chat.inputField.isFocused)
                voiceManager.MyVoiceController.ToggleMic();
        }

        if (isMainMenu)
        {
            if (Input.GetKeyDown(KeyCode.F1))
                InputServer();

            if (Input.GetKeyDown(KeyCode.F2))
                InputPlayer("Jess");

            if (Input.GetKeyDown(KeyCode.F3))
                InputPlayer("Jalil");

            if (Input.GetKeyDown(KeyCode.F4))
                InputPlayer("Sebas");

            if (Input.GetKeyDown(KeyCode.F5))
                InputPlayer("Lilo");

            if (Input.GetKeyDown(KeyCode.F6))
                InputPlayer("Nico");
        }


    }
}
