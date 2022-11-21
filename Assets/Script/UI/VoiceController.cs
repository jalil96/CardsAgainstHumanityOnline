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

        if (PhotonNetwork.IsMasterClient)
        {
            MasterVoiceManager.Instance.AddSoundReference(this, photonView.Owner);
            audioSource.volume = 0f;
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
    }

    public void EnabelSoundSystem(bool value)
    {
        SetSoundSystem(value);
        photonView.RPC(nameof(UpdateSoundSystem), RpcTarget.Others, isSoundOpen);
    }

    [PunRPC]
    private void UpdateSoundSystem(bool isOpen)
    {
        if (PhotonNetwork.IsMasterClient) return;
        SetSoundSystem(isOpen);
    }

    private void SetSoundSystem(bool value)
    {
        if (!hasVoiceUser) return;
        isSoundOpen = value;
        audioSource.volume = value ? 1.0f : 0.0f;
        SetVoice(value);

        voiceUI.SetSoundEnabled(value);
    }


    private void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient && MasterVoiceManager.Instance != null)
            MasterVoiceManager.Instance.RemoveSoundReference(this, photonView.Owner);

        if (hasVoiceUser)
        {
            if (voiceUI == null) return;
            voiceUI.micButton.onClick.RemoveListener(ToggleVoice);
            Destroy(voiceUI.gameObject);
        }
    }
}
