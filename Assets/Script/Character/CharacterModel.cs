using System;
using Photon.Pun;
using UnityEngine;

public class CharacterModel : MonoBehaviourPun
{
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
        _hand.GetSelectedCard();
    }
}
