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
    private bool isSoundOpen = true;

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
        if (!isSoundOpen) return;

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
        voiceUI = voiceUser;
        hasVoiceUser = true;
        isSoundOpen = true;
        voiceUI.SetUser(speaker, player);
        voiceUI.micButton.onClick.AddListener(ToggleVoice);
        voiceUI.SetSoundEnabled(true);
        SetVoice(false);
        //The system by default is open
    }

    public void EnabelSoundSystem(bool value)
    {
        isSoundOpen = value;
        audioSource.volume = value ? 1.0f : 0.0f;
        SetVoice(value);

        voiceUI.SetSoundEnabled(value);

        photonView.RPC(nameof(UpdateSoundSystem), RpcTarget.Others, isSoundOpen);
    }

    [PunRPC]
    private void UpdateSoundSystem(bool isOpen)
    {
        if (PhotonNetwork.IsMasterClient) return;
        EnabelSoundSystem(isOpen);
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
