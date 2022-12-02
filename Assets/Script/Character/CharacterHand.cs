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
    [SerializeField] private int _activeCards = 5;
    
    private int _selectedCard;
    
    private bool _isMine;
    
    public int SelectorIndex => _selectorIndex;
    public List<CardModel> Cards => _cards;

    public bool IsMine => _isMine;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient && _isMine)
        {
            photonView.RPC(nameof(RequestCards), PhotonNetwork.MasterClient, PhotonNetwork.LocalPlayer);
        }
    }

    [PunRPC]
    private void RequestCards(Player player)
    {
        var cards = _cards.Select(c => c.Text).ToArray();
        Debug.Log($"Sending {cards.Length} to player {player.NickName}");
        photonView.RPC(nameof(UpdateCards), player, (object)cards);
    }

    [PunRPC]
    private void UpdateCards(object newCards)
    {
        string[] cards = (string[])newCards;
        Debug.Log($"New cards to set: {cards.Length}");
        // debug
        string debug = "";
        foreach (var card in cards)
        {
            debug += $"{card}, ";
        }
        Debug.Log(debug);
        // end debug
        _activeCards = cards.Length;
        for (var i = 0; i < _cards.Count; i++)
        {
            if (cards.Length > i)
            {
                _cards[i].Text = cards[i];
                // _cards[i].Activate();
            }
            else
            {
                _cards[i].Deactivate();
            }
        }
        OnSetNewCards.Invoke();
    }

    public void SetIsMine(bool isMine)
    {
        _isMine = isMine;
    }
    
    public void MoveSelectorRight()
    {
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.All, Math.Min(_activeCards-1, _selectorIndex+1));
    }

    public void MoveSelectorLeft()
    {
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.All, Math.Max(0, _selectorIndex-1));
    }

    [PunRPC]
    private void UpdateSelectorIndex(int index)
    {
        _selectorIndex = index;
        if (PhotonNetwork.IsMasterClient)
        {
            _cards.ForEach(c => c.SetHovered(false));
            _cards[_selectorIndex].SetHovered(true);
        }
    }

    public void SelectCard()
    {
        photonView.RPC(nameof(UpdateSelectedCard), RpcTarget.All, _selectorIndex);
    }

    [PunRPC]
    private void UpdateSelectedCard(int selectedCard)
    {
        _selectedCard = selectedCard;
    }

    [PunRPC]
    private void RequestSelectedCard(Player player)
    {
        photonView.RPC(nameof(UpdateSelectedCard), player, _selectedCard);
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
        Debug.Log($"_cards array length: {_cards.Count}, newCards array length: {newCards.Count}");
        /*for (var i = 0; i < _cards.Count; i++)
        {
            if (newCards.Count > i)
            {
                _cards[i].Text = newCards[i];
                _cards[i].Activate();
            }
            else
            {
                _cards[i].Deactivate();
            }
        }*/

        // _activeCards = newCards.Count;
        // _selectorIndex = 0;
        
        photonView.RPC(nameof(UpdateCards), RpcTarget.All, (object)newCards.ToArray());
        photonView.RPC(nameof(UpdateSelectorIndex), RpcTarget.All, 0);
        // OnSetNewCards.Invoke();
    }
}