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
}