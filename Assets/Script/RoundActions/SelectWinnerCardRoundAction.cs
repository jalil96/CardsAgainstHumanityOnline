using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectWinnerCardRoundAction : RoundAction
{
    [SerializeField] private GameManager _gameManager;

    private List<CharacterModel> _characters;
    private CharacterModel _judge;
    private List<CardModel> _selectedCards;
    
    public override void StartRoundAction()
    {
        _characters = new List<CharacterModel>(_gameManager.Characters);
        
        _judge = _gameManager.GetCurrentJudge();
        _characters.Remove(_judge);
        
        
        Debug.Log($"Showing judge {MasterManager.Instance.GetPlayerFromCharacter(_judge).NickName} cards");

        _selectedCards = _gameManager.SelectedCards.Keys.ToList();
        _judge.Hand.SetCards(_selectedCards);
        
        _characters.ForEach(character => character.HideWhiteCards());
        
        _judge.ShowWhiteCards();
        
        _judge.OnSelectedCard += JudgeSelectedWinnerCard;
    }

    private void JudgeSelectedWinnerCard(CharacterModel character)
    {
        OnEndRoundAction.Invoke();
    }

    public override void EndRoundAction()
    {
        
    }
}
