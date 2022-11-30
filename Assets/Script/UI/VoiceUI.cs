using Photon.Voice.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class VoiceUI : MonoBehaviour
{
    public Color muteColor;
    public Color notSpeaking;
    public Color speakingColor;

    [Header("References")]
    public Button micButton;
    public Image micIcon;
    public TextMeshProUGUI nicknameTxt;
    public Image voiceAura;

    [Header("Spirtes")]
    public Sprite mic_on;
    public Sprite mic_off;
    public GameObject sound_off;

    private bool micEnabled;

    private void Awake()
    {
        SetSoundEnabled(true);
        micButton.interactable = false;
    }

    public void ToggleMic()
    {
        SetMicStatus(!micEnabled);
    }

    public void SetMicStatus(bool value)
    {
        micEnabled = value;

        micIcon.sprite = micEnabled ? mic_on : mic_off;
        micIcon.color = micEnabled ? speakingColor : muteColor;
    }

    public void SetSoundEnabled(bool value)
    {
        sound_off.SetActive(!value);
    }

    public void ShowTalking(bool isCurrentlySpeaking)
    {
        micIcon.color = isCurrentlySpeaking ? speakingColor : notSpeaking;
    }

    public void SetUser(Player player)
    {
        bool isOwner = player.IsLocal;

        micButton.interactable = isOwner;
        voiceAura.gameObject.SetActive(isOwner);
        nicknameTxt.text = player.NickName;
    }
}
