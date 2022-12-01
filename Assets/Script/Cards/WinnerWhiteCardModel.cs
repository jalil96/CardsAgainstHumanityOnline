using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinnerWhiteCardModel : MonoBehaviour
{
    public Action<bool> OnShowCard = delegate(bool b) { };
    public Action<string, string> OnSetText = delegate (string s, string s1) { };
    
    [SerializeField] private string _text;
    
    private bool _showCard = true;
    

    public bool ShowCard => _showCard;

    public void SetShowCard()
    {
        _showCard = true;
        OnShowCard.Invoke(_showCard);
        Invoke(nameof(UnsetShowCard), 2.5f);
    }

    private void UnsetShowCard()
    {
        _showCard = false;
        OnShowCard.Invoke(_showCard);
    }

    public void SetText(string text, string nickname)
    {
        _text = text;
        OnSetText.Invoke(_text, nickname);
    }
}
