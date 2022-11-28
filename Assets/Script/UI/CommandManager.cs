using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class Command
{
    public string name;
    public string description;
    public bool enabled;
    public bool gameplayOnly;
    [HideInInspector] public Action eventToCall; //this is for when they don't need to pass information
    [HideInInspector] public Action<string> eventToCallWithString; //and some events need to call for and events that pass information
    [HideInInspector] public bool hasValidation; //some might need to validate something
    [HideInInspector] public Func<string, bool> IsValid; //here is the func too call for validation
}

public class CommandManager : MonoBehaviour
{
    [SerializeField] private string commandPrefix = "/";

    [Header("All Time Up Commands")]
    [SerializeField] private Command privateMessage; //changed to a local command, sending and rpc that opens the private tab to the other player
    [SerializeField] private Command help;
    [SerializeField] private Command quitChat;
    [SerializeField] private Command mutePlayer; //local command that mutes the othe rplayer

    [Header("GameOnly Commands")]
    [SerializeField] private Command partypopper; //local command
    [SerializeField] private Command addTime; //master command
    [SerializeField] private Command switchMyHand; //master command
    [SerializeField] private Command switchAllHands; //master command
    [SerializeField] private Command switchBlackCard; //master command
    [SerializeField] private Command soundHorn; //master command

    [Header("Color")]
    [SerializeField] private Color helpDescriptionColor;
    private string helpDesHex;

    private Dictionary<string, Command> commandsList = new Dictionary<string, Command>();
    private bool inGameplayScene;

    //EVENTS FOR COMMANDS
    public Action<string> ErrorCommand = delegate { };
    public Action<string> InfoCommand = delegate { };
    public Action<string> PrivateMessageCommand =  delegate { };
    public Action<string> HelpCommand = delegate { };
    public Action<string> MutePlayer = delegate { };
    public Action<int> AddSecondsToTimer = delegate { };

    public Action SoundHorn =  delegate { };
    public Action PartyPopper = delegate { };
    public Action QuitChat = delegate { };
    public Action SwitchMyHand = delegate { };
    public Action SwitchAllWhites = delegate { };
    public Action SwitchBlack = delegate { };

    private void Awake()
    {
        //Colors
        helpDesHex = ColorUtility.ToHtmlStringRGBA(helpDescriptionColor);

        //Adding Events
        help.eventToCall = HelpList;

        partypopper.eventToCall = StartParty;
        quitChat.eventToCall = () => QuitChat();
        switchAllHands.eventToCall = SwitchTheWhites;
        switchMyHand.eventToCall = SwitchMyCards;
        switchBlackCard.eventToCall = SwitchBlackCard;
        soundHorn.eventToCall = SoundTheHorn;

        //Adding Events with Validation (and that they send something on the invoke)
        privateMessage.hasValidation = true;
        privateMessage.IsValid = ValidateIsUser;
        privateMessage.eventToCallWithString = PrivateMessage;

        mutePlayer.hasValidation = true;
        mutePlayer.IsValid = ValidateIsUser;
        mutePlayer.eventToCallWithString = Mute;

        addTime.hasValidation = true;
        addTime.IsValid = ValidateTime;
        addTime.eventToCallWithString = AddTimer;

        //Adding Commands
        AddACommand(help);
        AddACommand(privateMessage);
        AddACommand(mutePlayer);
        AddACommand(quitChat);
        AddACommand(partypopper);
        AddACommand(addTime);
        AddACommand(switchMyHand);
        AddACommand(switchAllHands);
        AddACommand(switchBlackCard);
        AddACommand(soundHorn);
    }

    //if it's a command will return true and invoke the command
    public bool IsCommand(string message)
    {
        string[] words = message.Split(' ');
        if (words[0].StartsWith(commandPrefix))
        {
            var target = words[0].Remove(0, commandPrefix.Length);

            if(commandsList.TryGetValue(target, out Command command))
            {
                if (!command.enabled)
                {
                    NotACommand(words[0]);
                    return true;
                };

                if (command.gameplayOnly && !inGameplayScene)
                {
                    ErrorCommand($"'{commandPrefix}{words[0]}' command can only be used during gameplay");
                    return true;
                }
				
                if (command.hasValidation)
                    return CommandWithValidation(command, words);

                command.eventToCall.Invoke();
                return true;
            }

            NotACommand(words[0]);
            return true;
        }

        return false;
    }


    private void AddACommand(Command command)
    {
        commandsList.Add(command.name, command);
    }


    #region Message Events
    private void NotACommand(string word)
    {
        ErrorCommand($"'{word}' is not a command. Get full list in {commandPrefix}{help.name}");
    }

    private void ShowCommandMessageInChat(string message)
    {
        InfoCommand(message);
    }
    #endregion

    #region Command Methods
    private void SwitchTheWhites()
    {
        ShowCommandMessageInChat($"{PhotonNetwork.LocalPlayer.NickName} has changed all the white cards");
        SwitchAllWhites.Invoke();
    }

    private void SwitchMyCards()
    {
        ShowCommandMessageInChat($"{PhotonNetwork.LocalPlayer.NickName} has changed their hand");
        SwitchMyHand.Invoke();
    }

    private void SwitchBlackCard()
    {
        ShowCommandMessageInChat($"{PhotonNetwork.LocalPlayer.NickName} has changed the black card");
        SwitchBlack.Invoke();
    }

    private void SoundTheHorn()
    {
        ShowCommandMessageInChat($"{PhotonNetwork.LocalPlayer.NickName} has blared a horn");
        MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestSoundHorn), PhotonNetwork.LocalPlayer);
    }

    private void PrivateMessage(string nickname)
    {
        PrivateMessageCommand.Invoke(nickname);
    }

    private void AddTimer(string time)
    {
        ShowCommandMessageInChat($"{PhotonNetwork.LocalPlayer.NickName} added {time} seconds to the timer");
        MasterManager.Instance.RPCMaster(nameof(MasterManager.Instance.RequestAddTime), PhotonNetwork.LocalPlayer, time);
    }

    private void StartParty()
    {
        ShowCommandMessageInChat($"{PhotonNetwork.LocalPlayer.NickName} is partying");
        PartyPopper.Invoke();
    }

    private void Mute(string nickname)
    {
        if (CommunicationsManager.Instance.MutePlayer(nickname))
        {
            ShowCommandMessageInChat($"{PhotonNetwork.LocalPlayer.NickName} has muted {nickname}");
            return;
        }

        NotACommand("Something went wrong somehow");
    }

    private void HelpList()
    {
        string allCommands = "";
        
        foreach (var command in commandsList)
        {
            if (!command.Value.enabled) continue; //if the command is disabled, we skip it.
            if (command.Value.gameplayOnly && !inGameplayScene) continue; //if the command is only for gameplay and we are not there, we do not list it. 

            allCommands += $"<b>{commandPrefix}{command.Key}</b>: <color=#{helpDesHex}>{command.Value.description} </color> \n";
        }

        HelpCommand.Invoke(allCommands);
    }
    #endregion

    #region Validation
    public void IsGameplay(bool value)
    {
        inGameplayScene = value;
    }

    private bool CommandWithValidation(Command command, string[] words)
    {
        if (words.Length <= 1)
        {
            ErrorCommand($"Forgot to add nickname");
            return true;
        }

        string nickname = words[1].Trim();
        if (!command.IsValid(nickname))
        {
            if (command == addTime)
                ErrorCommand($"'{commandPrefix}{words[0]}' needs to follow with a number. Check {commandPrefix}{help.name} for more information");
            else
                ErrorCommand($"User '{nickname}' was not found");

            return false;
        }
        else
        {

            if (nickname == PhotonNetwork.LocalPlayer.NickName)
            {
                ErrorCommand($"Can't use this command on yourself \n");
                return true;
            }

            command.eventToCallWithString.Invoke(nickname);
            return true;
        }
    }

    private bool ValidateIsUser(string nickname)
    {
        var playerList = CommunicationsManager.Instance.GetCurrentUserList();

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].IsMasterClient) continue;

            if (nickname == playerList[i].NickName)
                return true;
        }
        return false;
    }

    private bool ValidateTime(string time)
    {
        return float.TryParse(time, out float result);
    }
    #endregion
}
