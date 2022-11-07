using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    [SerializeField] private List<RoundAction> _roundActions;

    private Queue<RoundAction> _roundActionsQueue = new Queue<RoundAction>();
    private RoundAction _currentRoundAction;

    private List<CharacterModel> _characters;
    private int _currentJudgeIndex;

    public int CurrentJudgeIndex => _currentJudgeIndex;
    
    public List<CharacterModel> GetCharacters => _characters;

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) Destroy(gameObject);
    }

    private void Start()
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
        _currentRoundAction.OnEndRoundAction = delegate {};
        
        _currentRoundAction = roundAction;

        _currentRoundAction.OnEndRoundAction += RoundActionEnded;
        _currentRoundAction.StartRoundAction();
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            _characters.ForEach(character => character.HideWhiteCards());
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            _characters.ForEach(character => character.ShowWhiteCards());
        }
    }

    public CharacterModel GetCurrentJudge()
    {
        return _characters[_currentJudgeIndex];
    }
}