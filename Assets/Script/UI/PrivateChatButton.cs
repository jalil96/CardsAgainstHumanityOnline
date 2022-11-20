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
        chatText += newText;

        if (!isChatOpen)
            SetNotificationActive();
    }

    public void SetInFocus()
    {
        CommunicationsManager.Instance.chatManager.SetPrivateChatInFocus(this);
    }

    public void ShowChat()
    {
        isChatOpen = true;
        CleanNotifications();
    }

    public void HideChat()
    {
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
        if (!HasNotifications) return;
        counter = 0;
        notificationActiveIcon.SetActive(false);
    }
    #endregion
}
