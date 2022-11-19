using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHandUI : MonoBehaviour
{
    [SerializeField] private CharacterHand _hand;
    [SerializeField] private GameObject _selector;
    [SerializeField] private List<Transform> _selectorPositions;
    [SerializeField] private List<UICard> _cards;

    private void Update()
    {
        if (_hand == null) return;
        var pos = _selectorPositions[_hand.SelectorIndex].position;
        _selector.transform.position = new Vector2(pos.x, pos.y);
    }

    public void SetCharacterHand(CharacterHand characterHand)
    {
        _hand = characterHand;
        _hand.OnShowWhiteCards += ShowWhiteCards;
        _hand.OnHideWhiteCards += HideWhiteCards;
        _hand.OnSetNewCards += SetNewCards;
        
        SetNewCards();
    }

    private void ShowWhiteCards()
    {
        foreach (var card in _cards)
        {
            card.Activate();
        }
    }

    private void HideWhiteCards()
    {
        foreach (var card in _cards)
        {
            card.Deactivate();
        }
    }

    private void SetNewCards()
    {
        for (var i = 0; i < _cards.Count; i++)
        {
            if (_hand.Cards.Count > i)
            {
                _cards[i].text.text = _hand.Cards[i].Text;
            }
            else
            {
                _cards[i].Deactivate();
            }
        }
    }
}