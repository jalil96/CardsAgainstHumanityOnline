using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
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

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RequestCards), PhotonNetwork.MasterClient, PhotonNetwork.LocalPlayer);
        }
    }

    [PunRPC]
    private void RequestCards(Player player)
    {
        
        var cards = _cards.Select(c => (object)c.Text).ToArray();
        Debug.Log($"Sending {cards.Length} to player {player.NickName}");
        photonView.RPC(nameof(UpdateCards), player, cards);
    }

    [PunRPC]
    private void UpdateCards(object[] newCards)
    {
        string[] cards = newCards.Select(c => (string)c).ToArray();
        Debug.Log($"New cards to set: {cards.Length}");
        // debug
        string debug = "";
        foreach (var card in cards)
        {
            debug += $"{card}, ";
        }
        Debug.Log(debug);
        // end debug
        for (int i = 0; i < cards.Length; i++)
        {
            _cards[i].Text = cards[i];
        }
        OnSetNewCards.Invoke();
    }

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

    public void SetCards(List<string> newCards)
    {
        for (var i = 0; i < _cards.Count; i++)
        {
            _cards[i].Text = newCards[i];
        }
        _selectorIndex = 0;
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.Others, _selectorIndex);
        OnSetNewCards.Invoke();
    }
}