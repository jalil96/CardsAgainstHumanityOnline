using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CharacterHand : MonoBehaviourPun
{
    [SerializeField] private List<CardModel> _cards;

    [SerializeField] private int _selectorIndex;

    public int SelectorIndex => _selectorIndex;
    public void MoveSelectorRight()
    {
        _selectorIndex = Math.Min(_cards.Count, _selectorIndex+1);
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

    public CardModel GetSelectedCard()
    {
        return _cards[_selectorIndex];
    }
    
}