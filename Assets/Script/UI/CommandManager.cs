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
    public string errorMessage;
    public bool isEnabled;
    public bool isGameplayOnly;
    [HideInInspector] public Action eventToCall; //this is for when they don't need to pass information
    [HideInInspector] public Action<string> eventToCallWithString; //and some events need to call for and events that pass information
    [HideInInspector] public bool hasValidation; //some might need to validate something
    [HideInInspector] public Func<string, bool> IsValid; //here is the func too call for validation
}

public class CommandManager : MonoBehaviour
{
    public string commandPrefix = "/";

    [Header("Commands")]
    public Command privateMessage;
    public Command help;

    //local commands
    public Command partypopper;
    public Command mutePlayer;
    public Command quitChat;

    //master commands
    public Command addTime;
    public Command switchMyHand;
    public Command switchAllHands;
    public Command switchBlackCard;

    [Header("Color")]
    public Color helpDescriptionColor;
    private string helpDesHex;

    private Dictionary<string, Command> commandsList = new Dictionary<string, Command>();
    private bool inGameplayScene;

    //EVENTS FOR COMMANDS
    public Action<string> ErrorCommand = delegate { };
    public Action<string> PrivateMessageCommand =  delegate { };
    public Action<string> HelpCommand = delegate { };
    public Action<string> MuteChat = delegate { };
    public Action<string> AddSecondsToTimer = delegate { };

    public Action PartyPopper = delegate { };
    public Action QuitChat = delegate { };
    public Action SwitchMyHand = delegate { };
    public Action SwitchAllWhites = delegate { };
    public Action SwitchBlacks = delegate { };

    private void Awake()
    {
        //Colors
        helpDesHex = ColorUtility.ToHtmlStringRGBA(helpDescriptionColor);

        //Adding Events
        help.eventToCall = HelpList;

        partypopper.eventToCall = () => PartyPopper();
        quitChat.eventToCall = () => QuitChat();
        switchAllHands.eventToCall = () => SwitchAllWhites();
        switchMyHand.eventToCall = () => SwitchMyHand();
        switchBlackCard.eventToCall = () => SwitchBlacks();

        //Adding Events with Validation
        privateMessage.hasValidation = true;
        privateMessage.IsValid = ValidateIsUser;
        privateMessage.eventToCallWithString = PrivateMessageCommand;

        mutePlayer.hasValidation = true;
        mutePlayer.IsValid = ValidateIsUser;
        mutePlayer.eventToCallWithString = MuteChat;

        addTime.hasValidation = true;
        addTime.IsValid = ValidateTime;
        addTime.eventToCallWithString = AddSecondsToTimer;

        //Adding Commands
        AddACommand(privateMessage);
        AddACommand(help);
        AddACommand(partypopper);
        AddACommand(mutePlayer);
        AddACommand(quitChat);
        AddACommand(addTime);
        AddACommand(switchMyHand);
        AddACommand(switchAllHands);
        AddACommand(switchBlackCard);
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
                if (!command.isEnabled) return false;

                if (command.isGameplayOnly && !inGameplayScene)
                {
                    ErrorCommand($"'{commandPrefix}{words[0]}' command can only be used during gameplay");
                    return true;
                }
				
                if (command.hasValidation)
                    return CommandWithValidation(command, words);

                command.eventToCall();
                return true;
            }

            ErrorCommand($"'{words[0]}' is not a command. Get full list in {commandPrefix}{help.name}");
            return true;
        }

        return false;
    }

    public void IsGameplay(bool value)
    {
        inGameplayScene = value;
    }

    private void AddACommand(Command command)
    {
        commandsList.Add(command.name, command);
    }

    private void HelpList()
    {
        string allCommands = "";
        
        foreach (var command in commandsList)
        {
            if (!command.Value.isEnabled) continue; //if the command is disabled, we skip it.
            if (command.Value.isGameplayOnly && !inGameplayScene) continue; //if the command is only for gameplay and we are not there, we do not list it. 

            allCommands += $"<b>{commandPrefix}{command.Key}</b>: <color=#{helpDesHex}>{command.Value.description} </color> \n";
        }

        HelpCommand.Invoke(allCommands);
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
        }
        else
        {

            if (nickname == PhotonNetwork.LocalPlayer.NickName)
            {
                ErrorCommand($"Can't use this command on yourself \n");
                return true;
            }

            command.eventToCallWithString(nickname);
        }
        return true;
    }


    #region Validation
    private bool ValidateIsUser(string nickname)
    {
        var playerList = CommunicationsManager.Instance.GetCurrentUserList();

        for (int i = 0; i < playerList.Count; i++)
        {
            if (nickname == playerList[i].NickName)
                return true;
        }
        return false;
    }

    private bool ValidateTime(string time)
    {
        return Int32.TryParse(time, out int result);
    }

    #endregion
}
