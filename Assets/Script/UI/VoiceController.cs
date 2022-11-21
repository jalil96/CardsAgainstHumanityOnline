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
    private bool lastMicStatusWasOpen = false;

    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            MasterVoiceManager.Instance.AddSoundReference(this, photonView.Owner);
            audioSource.enabled = false;
            return;
        }

        if (photonView.IsMine)
        {
            gameObject.name += "OWNER";
        }
        else
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
                ToggleMic();
        }
        else
        {
            voiceUI.SetMicStatus(speaker.IsPlaying);
        }

    }

    public void ToggleMic()
    {
        SetMic(!isUsingMic);
    }

    public void SetMic(bool value)
    {
        isUsingMic = value;
        PunVoiceClient.Instance.PrimaryRecorder.TransmitEnabled = value;
        voiceUI.SetMicStatus(value);
    }

    public void SetUI(VoiceUI voiceUser, Player player)
    {
        hasVoiceUser = true;
        isSoundOpen = true;
        voiceUI = voiceUser;
        voiceUI.micButton.onClick.AddListener(ToggleMic);
        voiceUI.SetUser(player);

        if (!photonView.IsMine)
            voiceUI.SetMicStatus(speaker.IsPlaying);
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
        voiceUI.SetSoundEnabled(value);

        if (photonView.IsMine) 
        {
            if (value)
                voiceUI.SetMicStatus(lastMicStatusWasOpen);
            else
                lastMicStatusWasOpen = isUsingMic;
        }
        else
        {
            voiceUI.SetMicStatus(speaker.IsPlaying);
        }
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient && MasterVoiceManager.Instance != null)
            MasterVoiceManager.Instance.RemoveSoundReference(this, photonView.Owner);

        if (hasVoiceUser)
        {
            if (voiceUI == null) return;
            voiceUI.micButton.onClick.RemoveListener(ToggleMic);
            Destroy(voiceUI.gameObject);
        }
    }
}
