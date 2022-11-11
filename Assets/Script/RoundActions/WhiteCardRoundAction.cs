
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhiteCardRoundAction : RoundAction
{
    [SerializeField] private GameManager _gameManager;

    private List<CharacterModel> _characters;
    private CharacterModel _judge;

    private List<CharacterModel> _selectedCardCharacters;

    private bool _finishedSelecting;
    private List<CardModel> _selectedCards;

    public override void StartRoundAction()
    {
        _characters = new List<CharacterModel>(_gameManager.Characters);
        _judge = _gameManager.GetCurrentJudge();
        _characters.Remove(_judge);
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

        if (_selectedCardCharacters.Count != _characters.Count) return;
        
        _selectedCards = _selectedCardCharacters.Select(c => c.Hand.GetSelectedCard()).ToList();
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