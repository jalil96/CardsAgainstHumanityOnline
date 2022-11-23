using System;
using Photon.Pun;
using UnityEngine;

public class CharacterModel : MonoBehaviourPun
{
    public Action<CharacterModel> OnSelectedCard = delegate(CharacterModel model) {  };
    public Action<CharacterModel> OnUnselectedCard = delegate(CharacterModel model) {  };
    
    [SerializeField] private CharacterHand _hand;
    [SerializeField] private int _points;

    private bool _isMine = false;

    public bool IsMine => _isMine;

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
        if (dir) 
            _hand.MoveSelectorRight();
        else
            _hand.MoveSelectorLeft();
    }

    public void SelectCard()
    {
        _hand.SelectCard();
        if (_hand.GetSelectedCard() != null) OnSelectedCard.Invoke(this);
        else OnUnselectedCard.Invoke(this);
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
    }

    [PunRPC]
    private void UpdateHideWhiteCards()
    {
        _hand.HideWhiteCards();
    }

    [PunRPC]
    private void UpdatePoints(int points)
    {
        _points = points;
    }
}
