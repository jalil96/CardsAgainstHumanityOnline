using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    private bool isOpen;

    public bool IsOpen => isOpen;

    public Action OnOpen = delegate {};
    public Action OnClose = delegate { };

    public void OpenPanel()
    {
        isOpen = true;
        OnOpen.Invoke();
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        isOpen = false;
        OnClose.Invoke();
        gameObject.SetActive(false);
    }

}
