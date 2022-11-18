using System;
using Photon.Pun;
using UnityEngine;

public class CharacterModel : MonoBehaviourPun
{
    public Action<CharacterModel> OnSelectedCard = delegate(CharacterModel model) {  };
    public Action<CharacterModel> OnUnselectedCard = delegate(CharacterModel model) {  };
    
    [SerializeField] private CharacterHand _hand;

    public CharacterHand Hand => _hand;
    public int HandSelectorIndex => _hand.SelectorIndex;

    private void Start()
    {
        // if (!Equals(Client, PhotonNetwork.LocalPlayer)) return;
        // var handUI = FindObjectOfType<CharacterHandUI>();
        // handUI.SetCharacterHand(_hand);
    }

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
}
