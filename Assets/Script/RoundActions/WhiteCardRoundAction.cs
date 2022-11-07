
using System;
using System.Collections.Generic;
using UnityEngine;

public class WhiteCardRoundAction : RoundAction
{
    [SerializeField] private GameManager _gameManager;

    private List<CharacterModel> _characters;
    private CharacterModel _judge;

    public override void StartRoundAction()
    {
        _characters = _gameManager.GetCharacters;
        _judge = _characters[_gameManager.CurrentJudgeIndex];
        
    }
    
    private void Update()
    {
        
    }
}