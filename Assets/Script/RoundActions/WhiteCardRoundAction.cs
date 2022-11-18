
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhiteCardRoundAction : RoundAction
{
    [SerializeField] private GameManager _gameManager;

    private List<CharacterModel> _characters;
    private CharacterModel _judge;

    private List<CharacterModel> _selectedCardCharacters = new List<CharacterModel>();

    private bool _finishedSelecting;
    private Dictionary<CardModel, CharacterModel> _selectedCards;

    public override void StartRoundAction()
    {
        _characters = new List<CharacterModel>(_gameManager.Characters);
        _judge = _gameManager.GetCurrentJudge();
        _characters.Remove(_judge);
        Debug.Log($"Hiding judge {MasterManager.Instance.GetPlayerFromCharacter(_judge).NickName} cards");
        _judge.HideWhiteCards();
        
        _characters.ForEach(character =>
        {
            character.OnSelectedCard += CharacterSelectedCard;
            character.OnUnselectedCard += CharacterUnselectedCard;
        });
    }

    private void CharacterSelectedCard(CharacterModel character)
    {
        if (_selectedCardCharacters.Contains(character)) return;
        _selectedCardCharacters.Add(character);

        Debug.Log($"Player: {MasterManager.Instance.GetPlayerFromCharacter(character).NickName} selected a card");
        
        if (_selectedCardCharacters.Count != _characters.Count) return;

        Debug.Log($"All players players selected their cards");
        
        _selectedCardCharacters.ForEach(c =>
        {
            _selectedCards[c.Hand.GetSelectedCard()] = c;
        });
        
        _gameManager.SelectedCards = _selectedCards;
        OnEndRoundAction.Invoke();
    }
    
    private void CharacterUnselectedCard(CharacterModel character)
    {
        if (!_selectedCardCharacters.Contains(character)) return;
        _selectedCardCharacters.Remove(character);
    }
    
    private void Update()
    {
        
    }
}