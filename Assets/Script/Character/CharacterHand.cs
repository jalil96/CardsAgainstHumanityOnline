using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CharacterHand : MonoBehaviourPun
{
    public Action OnShowWhiteCards = delegate { };
    public Action OnHideWhiteCards = delegate { };
    public Action OnSetNewCards = delegate { };
    
    [SerializeField] private List<CardModel> _cards;
    [SerializeField] private List<Transform> _cardsPositions;
    [SerializeField] private int _selectorIndex;

    private int _selectedCard;
    
    public int SelectorIndex => _selectorIndex;
    public List<CardModel> Cards => _cards;
    public void MoveSelectorRight()
    {
        _selectorIndex = Math.Min(_cards.Count-1, _selectorIndex+1);
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.Others, _selectorIndex);
    }

    public void MoveSelectorLeft()
    {
        _selectorIndex = Math.Max(0, _selectorIndex-1);
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.Others, _selectorIndex);
    }

    [PunRPC]
    private void UpdateSelectorIndex(int index)
    {
        _selectorIndex = index;
    }

    public void SelectCard()
    {
        if (_selectedCard > 0 && _selectedCard == _selectorIndex) _selectedCard = -1; // Unselects card if selects the one already selected
        else _selectedCard = _selectorIndex;
    }

    public CardModel GetSelectedCard()
    {
        if (_selectedCard < 0) return null;
        return _cards[_selectorIndex];
    }
    
    public void ShowWhiteCards()
    {
        OnShowWhiteCards.Invoke();
        _cards.ForEach(card => card.Activate());
    }
    
    public void HideWhiteCards()
    {
        OnHideWhiteCards.Invoke();
        _cards.ForEach(card => card.Deactivate());
    }

    public void SetCards(List<CardModel> cards)
    {
        _cards = cards;
        _selectorIndex = 0;
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.Others, _selectorIndex);
        OnSetNewCards.Invoke();
    }
}