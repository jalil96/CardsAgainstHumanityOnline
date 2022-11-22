using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    [Header("Commands")]
    public string privateMessage = "whisper";
    public string help = "help";

    [Header("Color")]
    public Color helpDescriptionColor;
    private string helpDesHex;

    private string commandPrefix = "/";
    private Dictionary<string, Action> commandDictionary = new Dictionary<string, Action>();
    private Dictionary<string, string> commandDescriptions = new Dictionary<string, string>();
    private string currentMessage;

    //EVENTS FOR COMMANDS
    public Action<string> ErrorCommand = delegate { };
    public Action<string> PrivateMessageCommand =  delegate { };
    public Action<string> HelpCommand = delegate { };

    private void Awake()
    {
        helpDesHex = ColorUtility.ToHtmlStringRGBA(helpDescriptionColor);

        AddACommand(privateMessage, PrivateMessage, $"Write '/{privateMessage} UserName' to open a private chat");
        AddACommand(help, HelpList, "Prints a list of all available commands with a description");
    }

    //if it's a command will return true and invoke the command
    public bool IsCommand(string message)
    {
        string[] words = message.Split(' ');
        if (words[0].StartsWith(commandPrefix))
        {
            var target = words[0].Remove(0, commandPrefix.Length);
            if(commandDictionary.TryGetValue(target, out Action value))
            {
                if(target == privateMessage)
                {
                    currentMessage = words[1];
                }

                value();
                return true;
            }

            ErrorCommand($"'{words[0]}' is not a command. Get full list in {commandPrefix}help");
            return true;
        }

        return false;
    }

    //set command name, event to call and a short description of what it does;
    private void AddACommand(string command, Action action, string description)
    {
        commandDictionary.Add(command, action);
        commandDescriptions.Add(command, description);
    }

    private void PrivateMessage()
    {
        print("Private Message is:" + currentMessage);
        PrivateMessageCommand.Invoke(currentMessage);
    }

    private void HelpList()
    {
        string allCommands = "";

        foreach (var command in commandDictionary)
        {
            allCommands += $"<b>{commandPrefix}{command.Key}</b>: <color=#{helpDesHex}>{commandDescriptions[command.Key]} </color> \n";
        }

        HelpCommand.Invoke(allCommands);
    }
}
