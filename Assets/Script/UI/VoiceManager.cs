using Newtonsoft.Json.Linq;
using Photon.Voice.PUN;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Photon.Voice;
using Photon.Voice.Unity;
using Recorder = Photon.Voice.Unity.Recorder;
using Photon.Pun;
using Photon.Realtime;

//Create Game Object with Pun Voice Client to connect 
//Use PUN App Settings, Use Pun Auth Values. Auto Connect and Join. Auto Leave and Disconnect, all true;
//it will automatically connect and disconnect from rooms
//Add Recorder too

public class VoiceManager : MonoBehaviour
{
    public GameObject voiceSettingsPanel;
    public TMP_Dropdown micSelectionDropdown;
    public Button voiceSettingsButton;

    [Header("References")]
    public Sprite voiceChatEnabledIMG;
    public Sprite voiceChatDisabledIMG;
    public VoiceUI voiceUserPrefab;
    public GameObject micContainers;

    private Recorder recorder;
    private PunVoiceClient punVoiceClient;
    private bool voiceSettingsActive;
    private List<GameObject> voiceObjects = new List<GameObject>();
    private List<VoiceUI> users = new List<VoiceUI>();

    public void Awake()
    {
        voiceSettingsButton.onClick.AddListener(ToggleVoiceSettingsPanel); //TODO add somewhere a listener for and ESC button, if the settings are open, close them;
        voiceSettingsButton.gameObject.SetActive(false);
        DisableVoiceSettings();
#if !UNITY_EDITOR
        recorder.DebugEchoMode = false; //just in case. never never let recorder work in a build
#endif
    }

    #region Settings
    public void EnableVoiceSettings()
    {
        voiceSettingsButton.gameObject.SetActive(true);
        punVoiceClient = GetComponent<PunVoiceClient>();

        if (punVoiceClient != null)
            recorder = punVoiceClient.PrimaryRecorder;

        InstantiatePhotonVoiceObject();
        CompleteDropdown();
    }

    public void DisableVoiceSettings()
    {
        SetVoiceSettingsVisible(false);
        voiceSettingsButton.gameObject.SetActive(false);

        //we destroy all gameObjects, from players and whatnot
        for (int i = voiceObjects.Count - 1; i >= 0; i--)
        {
            var voice = voiceObjects[i];

            voiceObjects.Remove(voice);
            Destroy(voice);
        }
    }

    private void ToggleVoiceSettingsPanel()
    {
        SetVoiceSettingsVisible(!voiceSettingsActive);
    }

    private void SetVoiceSettingsVisible(bool value)
    {
        voiceSettingsActive = value;
        voiceSettingsPanel.SetActive(value);
    }
    private void CompleteDropdown()
    {
        micSelectionDropdown.options.Clear();
        micSelectionDropdown.onValueChanged.AddListener(SetMic);

        List<string> availableMics = new List<string>(Microphone.devices);

        micSelectionDropdown.AddOptions(availableMics);
    }

    private void SetMic(int i)
    {
        var currMic = Microphone.devices[i];
        recorder.MicrophoneDevice = new Photon.Voice.DeviceInfo(currMic);
    }
#endregion

    public void InstantiatePhotonVoiceObject() //we set ourselves here
    {
        var voice = PhotonNetwork.Instantiate("VoiceObject", Vector3.zero, Quaternion.identity);
        voiceObjects.Add(voice);

        VoiceController voiceController = voice.GetComponent<VoiceController>();
        CreateVisualUI(voiceController, PhotonNetwork.LocalPlayer);
    }

    public void CreateVisualUI(VoiceController voiceController, Player player)
    {
        if (PhotonNetwork.IsMasterClient) return;

        VoiceUI voiceUI = Instantiate(voiceUserPrefab, micContainers.transform);
        voiceController.SetUI(voiceUI, player);

        if (voiceUI != null)
            users.Add(voiceUI);
    }
}
