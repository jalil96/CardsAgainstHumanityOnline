using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
using System.Data.SqlTypes;
using Photon.Voice.Unity;
using Photon.Realtime;
using Newtonsoft.Json.Linq;

public class VoiceController : MonoBehaviourPun
{
    public VoiceUI voiceUI;
    public Speaker speaker;
    public AudioSource audioSource;

    private bool hasVoiceUser = false;
    private bool isUsingMic = false;
    private bool isSoundOpen = true;
    private bool lastMicStatusWasOpen = false;
    public Player Owner { get; private set; }

    void Awake()
    {
        transform.SetParent(CommunicationsManager.Instance.transform);

        Owner = photonView.Owner;

        if (PhotonNetwork.IsMasterClient)
        {
            MasterVoiceManager.Instance.AddSoundReference(this, Owner);
            audioSource.enabled = false;
            return;
        }

        if (photonView.IsMine)
        {
            gameObject.name += "OWNER";
        }
        else
        {
            CommunicationsManager.Instance.voiceManager.CreateVisualUI(this, Owner);
            CommunicationsManager.Instance.voiceManager.AddVoiceObject(Owner, this);
        }

    }

    void Update()
    {
        if (!hasVoiceUser) return;
        if (!isSoundOpen) return;

        if (!photonView.IsMine)
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
        photonView.RPC(nameof(UpdateSoundSystem), RpcTarget.Others, value);
    }

    public void MuteAnotherPlayer(Player player)
    {
        photonView.RPC(nameof(MuteYourself), player);
    }

    [PunRPC]
    private void MuteYourself()
    {
        print("I was muted " + Owner.NickName);
        SetMic(false);
    }

    [PunRPC]
    private void UpdateSoundSystem(bool isOpen)
    {
        if (PhotonNetwork.IsMasterClient) return;
        SetSoundSystem(isOpen);
    }

    public void OderToOpenPrivateChat(Player player)
    {
        photonView.RPC(nameof(ToldToOpenAChat), player, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    public void ToldToOpenAChat(Player sender) //this is here... because i need someone to have photon view
    {
        if (sender == PhotonNetwork.LocalPlayer) return;
        CommunicationsManager.Instance.chatManager.OpenAPrivateChat(sender.NickName);
    }


    private void SetSoundSystem(bool value)
    {
        if (!hasVoiceUser) return;
        isSoundOpen = value;
        audioSource.enabled = value;
        voiceUI.SetSoundEnabled(value);

        if (photonView.IsMine) 
        {
            if (value)
                SetMic(lastMicStatusWasOpen);
            else
                lastMicStatusWasOpen = isUsingMic;
        }
        else
        {
            voiceUI.SetMicStatus(speaker.IsPlaying);
        }

        print($"{photonView.Owner.NickName} was called, set sound system to set sound to: " + value);
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
