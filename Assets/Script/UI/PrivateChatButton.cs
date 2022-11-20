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

    public Color openChatColor = Color.black;
    public Color closeChatColor = Color.grey;

    private bool isChatOpen;
    private int counter;

    public bool HasNotifications => counter > 0;

    #region PUBLIC
    public void AssignUser(string player)
    {
        button.onClick.AddListener(SetInFocus);
        titleText.text = player;
    }

    public void UpdateText(string newText)
    {
        chatText = newText;

        if (!isChatOpen)
            SetNotificationActive();
        else
            CommunicationsManager.Instance.chatManager.UpdateText(chatText);
    }

    public void SetInFocus()
    {
        CommunicationsManager.Instance.chatManager.SetPrivateChatInFocus(this);
    }

    public void ShowChat()
    {
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
