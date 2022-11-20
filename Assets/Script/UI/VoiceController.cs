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

    private bool hasVoiceUser = false;
    private bool isUsingMic = false;

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
        SetVoice(!isUsingMic);
    }

    public void SetVoice(bool value)
    {
        isUsingMic = value;
        PunVoiceClient.Instance.PrimaryRecorder.TransmitEnabled = isUsingMic;
        voiceUI.SetMicStatus(isUsingMic);
    }

    public void SetUI(VoiceUI voiceUser, Player player)
    {
        this.voiceUI = voiceUser;
        voiceUI.SetUser(speaker, player);
        hasVoiceUser = true;
        voiceUI.micButton.onClick.AddListener(ToggleVoice);
        SetVoice(false);
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
