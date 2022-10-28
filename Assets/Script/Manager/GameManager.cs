using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<RoundAction> _roundActions;

    private Queue<RoundAction> _roundActionsQueue = new Queue<RoundAction>();
    private RoundAction _currentRoundAction;
    
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
    }
}