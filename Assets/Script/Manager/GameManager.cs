using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<RoundAction> _roundActions;

    private Queue<RoundAction> _roundActionsQueue = new Queue<RoundAction>();
    private RoundAction _currentRoundAction;

    private List<CharacterModel> _characters;
    private int _currentJudgeIndex;
    
    private void Awake()
    {
        EnqueueRoundActions();
    }

    private void RoundActionEnded()
    {
        if (_roundActionsQueue.Count == 0)
        {
            RoundEnded();
        }
        
        SetCurrentRoundAction(_roundActionsQueue.Dequeue());
    }

    private void RoundEnded()
    {
        EnqueueRoundActions();
    }

    private void SetCurrentRoundAction(RoundAction roundAction)
    {
        _currentRoundAction.EndRoundAction = delegate {};
        
        _currentRoundAction = roundAction;

        _currentRoundAction.EndRoundAction += RoundActionEnded;
    }

    private void EnqueueRoundActions()
    {
        foreach (var roundAction in _roundActions)
        {
            _roundActionsQueue.Enqueue(roundAction);
        }

        if (_currentJudgeIndex > _characters.Count - 1) _currentJudgeIndex = 0;
        else _currentJudgeIndex++;
    }

    public void SetCharacters(List<CharacterModel> characters)
    {
        _characters = characters;
    }

    public CharacterModel GetCurrentJudge()
    {
        return _characters[_currentJudgeIndex];
    }
}