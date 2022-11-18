using System;
using Photon.Pun;
using UnityEngine;

public enum CardType
{
    White,
    Black
}

public class CardModel : MonoBehaviourPun
{

    [SerializeField] private string _text;
    [SerializeField] private CardType _type;

    public string Text => _text;

    private void Awake()
    {
        if (_type == null) _type = CardType.White;
        if (_text == "") _text = Guid.NewGuid().ToString();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
}