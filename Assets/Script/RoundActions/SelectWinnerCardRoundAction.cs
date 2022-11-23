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
        _judge.Hand.SetCards(_selectedCards.Select(c => c.Text).ToList());
        
        _characters.ForEach(character => character.HideWhiteCards());

        _judge.OnSelectedCard += JudgeSelectedWinnerCard;
    }

    private void JudgeSelectedWinnerCard(CharacterModel character)
    {
        //TODO: Get judge card, map it with the character that selected it and give the point to that character DONE

        var selectedCard = _judge.Hand.GetSelectedCard();
        CharacterModel winner;
        
        foreach (var card in _gameManager.SelectedCards.Keys)
        {
            if (card.Text == selectedCard.Text)
            {
                winner = _gameManager.SelectedCards[card];
                winner.Points++;
                Debug.Log($"Giving point to player: {MasterManager.Instance.GetPlayerFromCharacter(winner).NickName}");
                break;
            }
        }
        
        _judge.OnSelectedCard = delegate(CharacterModel model) {  };

        OnEndRoundAction.Invoke();
    }

    public override void EndRoundAction()
    {
        
    }
}
