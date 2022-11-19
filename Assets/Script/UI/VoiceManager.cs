using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
//using Photon.Voice.Unity

//Create Game Object with Pun Voice Client to connect 
//Use PUN App Settings, Use Pun Auth Values. Auto Connect and Join. Auto Leave and Disconnect, all true;
//it will automatically connect and disconnect from rooms
//Add Recorder too


public class VoiceManager : MonoBehaviour
{
    [SerializeField] private Sprite voiceChatEnabledIMG;
    [SerializeField] private Sprite voiceChatDisabledIMG;
    [SerializeField] private TMP_Dropdown micSelectionDropdown; //TODO move all the mic selection to it's on script. but lets leave it here for now

    private Recorder recorder;
    //private PunVoiceClient punVoiceClient;

    public void Awake()
    {
        CompleteDropdown();
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }

    private void CompleteDropdown()
    {
        micSelectionDropdown.options.Clear();

        List<string> availableMics = new List<string>(Microphone.devices);

        micSelectionDropdown.AddOptions(availableMics);
    }

    private void SetMic(int i)
    {
        var currMic = Microphone.devices[i];
        //recorder.MicrophoneDevice = new Photon.DeviceInfo(currMic);
    }

    //TODO SOMEWHERE ELSE: instanciar desde un controlador algo un VoiceObject que tenga PhotonVire, Photon Voice View, Speaker (y AudioSource, que se deja en Audio 2D). 
    //PhotonNetwork. Instantiate("VoiceObject", Vector3.zero, Quaternion.identity);
}
