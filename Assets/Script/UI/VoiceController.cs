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
    public AudioSource audioSource;

    private bool hasVoiceUser = false;
    private bool isUsingMic = false;

    void Awake()
    {
        if (!photonView.IsMine)
        {
            CommunicationsManager.Instance.voiceManager.CreateVisualUI(this, photonView.Owner);
            CommunicationsManager.Instance.voiceManager.AddVoiceObject(this.gameObject);
        }
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

    public void EnableMicSystem(bool value)
    {
        audioSource.volume = value ? 1.0f : 0.0f;
        SetVoice(value);

        voiceUI.SetSoundEnabled(value);
        //TODO send a request to show the others I'm not hearing anything;
    }

    private void OnDestroy()
    {
        if (hasVoiceUser)
        {
            if (voiceUI == null) return;
            voiceUI.micButton.onClick.RemoveListener(ToggleVoice);
            Destroy(voiceUI.gameObject);
        }
    }
}
