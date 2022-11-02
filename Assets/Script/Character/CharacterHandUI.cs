using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHandUI : MonoBehaviour
{
    [SerializeField] private CharacterHand _hand;
    [SerializeField] private GameObject _selector;
    [SerializeField] private List<Transform> _selectorPositions;

    private void Update()
    {
        if (_hand == null) return;
        var pos = _selectorPositions[_hand.SelectorIndex].position;
        _selector.transform.position = new Vector2(pos.x, pos.y);
    }

    public void SetCharacterHand(CharacterHand characterHand)
    {
        _hand = characterHand;
    }
}