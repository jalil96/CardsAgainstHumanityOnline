using System;
using System.Runtime.InteropServices;
using Photon.Pun;
using UnityEngine;

public class CharacterModel : MonoBehaviourPun
{
    public Action<CharacterModel> OnSelectedCard = delegate(CharacterModel model) {  };
    public Action<CharacterModel> OnUnselectedCard = delegate(CharacterModel model) {  };
    public Action OnHideCards = delegate {};
    public Action OnShowCards = delegate {};


    [SerializeField] private CharacterHand _hand;
    [SerializeField] private int _points;

    private bool _selectedCard;
    private bool _isMine = false;
    private bool _hidenCards = false;
    
    public bool IsMine => _isMine;
    public bool HidenCards => _hidenCards;
    public bool SelectedCard => _selectedCard;
    
    public int Points
    {
        get => _points;
        set
        {
            _points = value;
            photonView.RPC(nameof(UpdatePoints), RpcTarget.Others, _points);
        }
    }

    public CharacterHand Hand => _hand;
    public int HandSelectorIndex => _hand.SelectorIndex;
    

    public void Move(bool dir)
    {
        if (_selectedCard) return;
        if (dir) 
            _hand.MoveSelectorRight();
        else
            _hand.MoveSelectorLeft();
    }

    public void SelectCard()
    {
        if (_hidenCards) return;
        _hand.SelectCard();
        _selectedCard = true;
        if (_hand.GetSelectedCard() != null) OnSelectedCard.Invoke(this);
        else OnUnselectedCard.Invoke(this);
    }

    public void SetSelectedCard(bool selected)
    {
        _selectedCard = selected;
    }

    public void SetIsMine(bool isMine)
    {
        _isMine = isMine;
    }
        
    public void ShowWhiteCards()
    {
        photonView.RPC(nameof(UpdateShowWhiteCards), RpcTarget.All);
    }

    public void HideWhiteCards()
    {
        photonView.RPC(nameof(UpdateHideWhiteCards), RpcTarget.All);
    }

    [PunRPC]
    private void UpdateShowWhiteCards()
    {
        _hand.ShowWhiteCards();
        _hidenCards = false;
        OnShowCards.Invoke();
    }

    [PunRPC]
    private void UpdateHideWhiteCards()
    {
        _hand.HideWhiteCards();
        _hidenCards = true;
        OnHideCards.Invoke();
    }

    [PunRPC]
    private void UpdatePoints(int points)
    {
        _points = points;
    }
}
