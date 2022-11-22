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
    public Toggle enableVoiceChatToggle;

    [Header("Sound Info")]
    public GameObject soundIconOff;
    public bool allPlayersStartMute = true;

    [Header("References")]
    public Color openSettingsColor = Color.black;
    public Color closeSettingsColor = Color.white;
    public Image voiceSettingsImage;
    public Sprite voiceChatEnabledIMG;
    public Sprite voiceChatDisabledIMG;
    public VoiceUI voiceUserPrefab;
    public GameObject micContainers;

    private VoiceController voiceController;
    private Recorder recorder;
    private PunVoiceClient punVoiceClient;
    private bool voiceSettingsActive;
    private List<VoiceController> voiceObjects = new List<VoiceController>();
    private List<VoiceUI> users = new List<VoiceUI>();

    public void Awake()
    {
        voiceSettingsButton.onClick.AddListener(ToggleVoiceSettingsMenu); //TODO add somewhere a listener for and ESC button, if the settings are open, close them;
        enableVoiceChatToggle.onValueChanged.AddListener(EnableAudioSourceSystem);
        soundIconOff.SetActive(false);

        DisableVoiceSettings();
#if !UNITY_EDITOR
        recorder.DebugEchoMode = false; //just in case. never never let recorder work in a build
#endif
    }

    #region Settings
    public void EnableVoiceSettings()
    {
        punVoiceClient = GetComponent<PunVoiceClient>();
        if (PhotonNetwork.IsMasterClient) return;

        voiceSettingsButton.gameObject.SetActive(true);
        if (punVoiceClient != null)
            recorder = punVoiceClient.PrimaryRecorder;

        InstantiatePhotonVoiceObject();
        CompleteDropdown();
    }

    public void DisableVoiceSettings()
    {
        SetVoiceSettingsVisible(false);
        voiceSettingsButton.gameObject.SetActive(false);

        ClearData();
    }

    private void ToggleVoiceSettingsMenu()
    {
        SetVoiceSettingsVisible(!voiceSettingsActive);
    }

    private void EnableAudioSourceSystem(bool value)
    {
        print("AudioSourceSystem was: " + value);
        MasterVoiceManager.Instance.RPCMaster(nameof(MasterVoiceManager.Instance.RequestUpdateSoundStatus), PhotonNetwork.LocalPlayer, value);
        micContainers.gameObject.SetActive(value);
        soundIconOff.SetActive(!value);

        SetAudioInVoiceObjects(value);

        if (!value) //if we are disabling the system, we want to mute ourselves
            voiceController.SetMic(value);
    }

    private void SetVoiceSettingsVisible(bool value)
    {
        voiceSettingsActive = value;
        voiceSettingsPanel.SetActive(voiceSettingsActive);
        voiceSettingsImage.color = voiceSettingsActive ? openSettingsColor : closeSettingsColor;
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
        voiceController = voice.GetComponent<VoiceController>();
        CreateVisualUI(voiceController, PhotonNetwork.LocalPlayer);
        voiceController.SetMic(!allPlayersStartMute);
        AddVoiceObject(voiceController);
    }

    public void CreateVisualUI(VoiceController voiceController, Player player)
    {
        if (PhotonNetwork.IsMasterClient) return;

        VoiceUI voiceUI = Instantiate(voiceUserPrefab, micContainers.transform);
        voiceController.SetUI(voiceUI, player);

        if (voiceUI != null)
            users.Add(voiceUI);
    }

    public void AddVoiceObject(VoiceController voice)
    {
        if (voiceObjects.Contains(voice)) return;
        voiceObjects.Add(voice);
    }

    public void SetAudioInVoiceObjects(bool canHear)
    {
        for (int i = voiceObjects.Count - 1; i >= 0; i--)
        {
            voiceObjects[i].audioSource.enabled = canHear;
        }
    }

    public void ClearData()
    {
        //we destroy all gameObjects, from players and whatnot
        for (int i = voiceObjects.Count - 1; i >= 0; i--)
        {
            var voice = voiceObjects[i];

            voiceObjects.Remove(voice);
            Destroy(voice);
        }
    }
}
