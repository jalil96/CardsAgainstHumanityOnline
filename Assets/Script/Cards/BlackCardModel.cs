using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackCardModel : MonoBehaviour
{
    public Action<bool> OnShowCard = delegate(bool b) { };
    public Action<string> OnSetText = delegate (string s) { };
    
    [SerializeField] private string _text;
    
    private bool _showCard = true;
    

    public bool ShowCard => _showCard;

    public void SetShowCard()
    {
        _showCard = false;
        OnShowCard.Invoke(_showCard);
        Invoke(nameof(UnsetShowCard), 1f);
    }

    private void UnsetShowCard()
    {
        _showCard = true;
        OnShowCard.Invoke(_showCard);
    }

    public void SetText(string text)
    {
        _text = text;
        OnSetText.Invoke(_text);
    }

}
