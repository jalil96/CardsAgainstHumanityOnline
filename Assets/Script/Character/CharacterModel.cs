using System;
using UnityEngine;

public class CharacterModel : PUNObject
{
    [SerializeField] private CharacterHand _hand;

    public int HandSelectorIndex => _hand.SelectorIndex;

    public void Move(bool dir)
    {
        if (dir) 
            _hand.MoveSelectorRight();
        else
            _hand.MoveSelectorLeft();
    }
}
