using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationsManager : MonoBehaviour
{
    static public CommunicationsManager Instance { get; private set; }
    static public bool HasInstance => Instance != null;

    public ChatManager chatManager;
    public VoiceManager voiceManager;
    public CommandManager commandManager;
    public ChatColorsDictionary colorsDictionary;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
			return;
        }
        
		Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
