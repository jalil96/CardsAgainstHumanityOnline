
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhiteCardRoundAction : RoundAction
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private ChronometerController _chronometer;
    [Range(1, 1000)] [SerializeField] private float _chronometerTime = 30f;
    [SerializeField] private TempCardsController _tempCardsController;
    
    private List<CharacterModel> _characters;
    private CharacterModel _judge;

    private List<CharacterModel> _selectedCardCharacters = new List<CharacterModel>();

    private bool _finishedSelecting;
    private Dictionary<CardModel, CharacterModel> _selectedCards = new Dictionary<CardModel, CharacterModel>();

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
            character.SetSelectedCard(false);
            character.ShowWhiteCards();
        });
        
        _chronometer.StartChronometer(_chronometerTime);
        _chronometer.OnChronometerTimeElapsed += ChronometerTimeEnded;
    }

    private void CharacterSelectedCard(CharacterModel character)
    {
        if (_selectedCardCharacters.Contains(character)) return;
        _selectedCardCharacters.Add(character);

        Debug.Log($"Player: {MasterManager.Instance.GetPlayerFromCharacter(character).NickName} selected a card");
        
        character.HideWhiteCards();
        _tempCardsController.SpawnTempCard(character.transform.position, character.transform.rotation, true);
        if (_selectedCardCharacters.Count != _characters.Count) return;

        Debug.Log($"All players players selected their cards");
        
        _characters.ForEach(c =>
        {
            c.OnSelectedCard = delegate(CharacterModel model) {  };
            c.OnUnselectedCard = delegate(CharacterModel model) {  };
        });

        _selectedCards = new Dictionary<CardModel, CharacterModel>();
        
        _selectedCardCharacters.ForEach(c =>
        {
            _selectedCards[c.Hand.GetSelectedCard()] = c;
        });

        _gameManager.SelectedCards = _selectedCards;
        _selectedCardCharacters.Clear();
        
        _chronometer.OnChronometerTimeElapsed = delegate {};
        _chronometer.StopChronometer();
        
        OnEndRoundAction.Invoke();
    }
    
    private void CharacterUnselectedCard(CharacterModel character)
    {
        if (!_selectedCardCharacters.Contains(character)) return;
        _selectedCardCharacters.Remove(character);
    }

    private void ChronometerTimeEnded()
    {
        _characters.ForEach(c =>
        {
            if (!c.SelectedCard)
            {
                c.SelectCard();    
            }
        });
    }
    
}