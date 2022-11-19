using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoiceUserUI : MonoBehaviour
{
    [Header("References")]
    public Button micButton;
    public TextMeshProUGUI nicknameTxt;
    public Image mic_Image;
    public Image voiceAura;

    [Header("Spirtes")]
    public Sprite mic_on;
    public Sprite mic_off;


    private bool micEnabled;

    private void Start()
    {
        micButton.onClick.AddListener(ToggleMic);
    }

    public void ToggleMic()
    {
        SetMic(!micEnabled);
    }

    private void SetMic(bool value)
    {
        micEnabled = value;

        mic_Image.sprite = micEnabled ? mic_on : mic_off;
    }

}
