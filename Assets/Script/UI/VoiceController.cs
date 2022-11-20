using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using System.Data.SqlTypes;
using Photon.Voice.Unity;
using Photon.Realtime;

public class VoiceController : MonoBehaviourPun
{
    public VoiceUI voiceUI;
    public Speaker speaker;
    public Recorder recorder;

    private bool hasVoiceUser = false;
    private bool isUsingMic = false;
    private bool isRecording = false;
    private bool hasRecorder = false;

    void Awake()
    {
        if (!photonView.IsMine)
            CommunicationsManager.Instance.voiceManager.CreateVisualUI(this, photonView.Owner);
    }

    void Update()
    {
        if (!hasVoiceUser) return;

        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.M))
                ToggleVoice();
        }
        else
        {
            voiceUI.SetMicStatus(speaker.IsPlaying);
        }

    }

    public void ToggleVoice()
    {
        isUsingMic = !isUsingMic;
        PunVoiceClient.Instance.PrimaryRecorder.TransmitEnabled = isUsingMic;
        voiceUI.SetMicStatus(isUsingMic);
    }

    public void SetUI(VoiceUI voiceUser, Player player, Recorder recorder = null)
    {
        this.voiceUI = voiceUser;
        voiceUI.SetUser(speaker, player);
        hasVoiceUser = true;
        voiceUI.micButton.onClick.AddListener(ToggleVoice);
        voiceUI.SetMicStatus(PunVoiceClient.Instance.PrimaryRecorder.TransmitEnabled);

        if (recorder != null)
        {
            hasRecorder = true;
            this.recorder = recorder;
        }
    }

    private void OnDestroy()
    {
        if (hasVoiceUser)
        {
            voiceUI.micButton.onClick.RemoveListener(ToggleVoice);
            Destroy(voiceUI.gameObject);
        }
    }
}
