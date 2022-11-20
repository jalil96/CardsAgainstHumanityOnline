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

    private bool micEnabled;
    private Image currentImage;
    private bool isSpeaking = false;

    public void ToggleMic()
    {
        SetMicStatus(!micEnabled);
    }

    public void SetMicStatus(bool value)
    {
        if (micEnabled == value) return;

        micEnabled = value;

        currentImage.sprite = micEnabled ? mic_on : mic_off;
        currentImage.color = micEnabled ? speakingColor : muteColor;
    }

    public void ShowTalking(bool isCurrentlySpeaking)
    {
        if (isSpeaking == isCurrentlySpeaking) return;
        isSpeaking = isCurrentlySpeaking;

        currentImage.color = isCurrentlySpeaking ? speakingColor : notSpeaking;
    }

    public void SetUser(Speaker speaker, Player player)
    {
        bool isOwner = player.IsLocal;

        if (isOwner)
        {
            micButton.onClick.AddListener(ToggleMic);
            currentImage = micButton.GetComponent<Image>();
        }
        {
            currentImage = micIcon;
        }

        voiceAura.gameObject.SetActive(isOwner);
        micButton.gameObject.SetActive(isOwner);
        micIcon.gameObject.SetActive(!isOwner);

        nicknameTxt.text = player.NickName;
    }
}
