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
    public GameObject soundIconContainer;
    public GameObject soundIconOn;
    public GameObject soundIconOff;

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
    private List<GameObject> voiceObjects = new List<GameObject>();
    private List<VoiceUI> users = new List<VoiceUI>();

    public void Awake()
    {
        voiceSettingsButton.onClick.AddListener(ToggleVoiceSettingsMenu); //TODO add somewhere a listener for and ESC button, if the settings are open, close them;
        enableVoiceChatToggle.onValueChanged.AddListener(EnableVoiceSound);

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
        soundIconContainer.SetActive(true);

        if (punVoiceClient != null)
            recorder = punVoiceClient.PrimaryRecorder;

        InstantiatePhotonVoiceObject();
        EnableVoiceSound(true);
        CompleteDropdown();
    }

    public void DisableVoiceSettings()
    {
        SetVoiceSettingsVisible(false);
        voiceSettingsButton.gameObject.SetActive(false);
        soundIconContainer.gameObject.SetActive(false);

        ClearData();
    }

    private void ToggleVoiceSettingsMenu()
    {
        SetVoiceSettingsVisible(!voiceSettingsActive);
    }

    private void EnableVoiceSound(bool value)
    {
        voiceController.EnabelSoundSystem(value);
        micContainers.gameObject.SetActive(value);
        soundIconOff.SetActive(!value);
        //soundIconOn.SetActive(value);
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
        AddVoiceObject(voice);

        voiceController = voice.GetComponent<VoiceController>();
        CreateVisualUI(voiceController, PhotonNetwork.LocalPlayer);

        MasterVoiceManager.Instance.AddSoundReference(voiceController, PhotonNetwork.LocalPlayer);
    }

    public void CreateVisualUI(VoiceController voiceController, Player player)
    {
        if (PhotonNetwork.IsMasterClient) return;

        VoiceUI voiceUI = Instantiate(voiceUserPrefab, micContainers.transform);
        voiceController.SetUI(voiceUI, player);

        if (voiceUI != null)
            users.Add(voiceUI);
    }

    public void AddVoiceObject(GameObject voice)
    {
        if (voiceObjects.Contains(voice)) return;
        voiceObjects.Add(voice);
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

        //for (int i = users.Count - 1; i >= 0; i--)
        //{
        //    var user = users[i];
        //    users.Remove(user);
        //    Destroy(user);
        //}
    }
}
