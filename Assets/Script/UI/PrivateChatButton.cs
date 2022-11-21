using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateChatButton : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public Button button;
    public GameObject notificationActiveIcon;
    public string chatText;
    public string nickname;
    public Image bgColor;
    public TextMeshProUGUI messageContainer;

    public Color openChatColor = Color.black;
    public Color closeChatColor = Color.grey;

    private bool isChatOpen;
    private int counter;

    public bool IsMainChat { get; private set; }
    public bool HasNotifications => counter > 0;

    #region PUBLIC
    public void AssignUser(string player, TextMeshProUGUI textContainer)
    {
        button.onClick.AddListener(SetInFocus);
        titleText.text = player;
        nickname = player;
        messageContainer = textContainer;
        IsMainChat = false;
        HideChat();
    }

    public void AssingAsMain(TextMeshProUGUI textContainer)
    {
        button.onClick.AddListener(SetInFocus);
        titleText.text = "MAIN";
        nickname = "MAIN";
        IsMainChat = true;
        messageContainer = textContainer;
        ShowChat();
    }

    public void UpdateText(string newText)
    {
        chatText += newText;

        if (isChatOpen)
            messageContainer.text = chatText;
        else
            SetNotificationActive();
    }

    public void SetInFocus()
    {
        if(IsMainChat)
            CommunicationsManager.Instance.chatManager.SetMainChatInFocus();
        else
            CommunicationsManager.Instance.chatManager.SetPrivateChatInFocus(this);
    }

    public void ShowChat()
    {
        messageContainer.text = chatText;
        isChatOpen = true;
        bgColor.color = openChatColor;
        CleanNotifications();
    }

    public void HideChat()
    {
        bgColor.color = closeChatColor;
        isChatOpen = false;
    }
    #endregion

    #region PRIVATE 
    private void SetNotificationActive()
    {
        counter++;
        notificationActiveIcon.SetActive(true);
    }

    private void CleanNotifications()
    {
        counter = 0;
        notificationActiveIcon.SetActive(false);
    }
    #endregion
}
