using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    [SerializeField] private List<RoundAction> _roundActions;

    private Queue<RoundAction> _roundActionsQueue = new Queue<RoundAction>();
    private RoundAction _currentRoundAction;

    private List<CharacterModel> _characters;
    private int _currentJudgeIndex;

    private Stack<string> _blackCards = new Stack<string>();
    private Stack<string> _whiteCards = new Stack<string>();
    
    public int CurrentJudgeIndex => _currentJudgeIndex;
    public List<CharacterModel> Characters => _characters;

    public Dictionary<CardModel, CharacterModel> SelectedCards { get; set; }

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) Destroy(gameObject);
    }

    private void Start()
    {
        EnqueueRoundActions();
        RoundActionEnded();
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
        // TODO: Check win condition by points, Call SetCharacters To give new cards
        // TODO: If win condition met, show Win/Lose screen on respective players and move to the scoreboard scene

        foreach (var character in _characters)
        {
            if (character.Points >= 3) // Parametrize win condition value
            {
                Debug.Log($"Win condition met, winner is: {MasterManager.Instance.GetPlayerFromCharacter(character).NickName}");
                WinConditionMet(character);
                break;
            }
        }
        
        SetCharacters(_characters);
        
        EnqueueRoundActions();
    }

    private void WinConditionMet(CharacterModel winner)
    {
        foreach (var characterModel in _characters)
        {
            var player = MasterManager.Instance.GetPlayerFromCharacter(characterModel);
            player.SetScore(characterModel.Points);
            MasterManager.Instance.RPC(nameof(MasterManager.Instance.WinConditionMet) ,player,characterModel == winner);
        }
    }

    private void LoadCards()
    {
        for (int i = 0; i < 300; i++)
        {
            _whiteCards.Push(Guid.NewGuid().ToString());
        }
        
        for (int i = 0; i < 100; i++)
        {
            _blackCards.Push(Guid.NewGuid().ToString());
        }
    }

    private void SetCurrentRoundAction(RoundAction roundAction)
    {
        if (_currentRoundAction != null) _currentRoundAction.OnEndRoundAction = delegate {};
        
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

        if (_currentJudgeIndex >= _characters.Count - 1) _currentJudgeIndex = 0;
        else _currentJudgeIndex++;
    }

    public void SetCharacters(List<CharacterModel> characters)
    {
        _characters = characters;
        if (_whiteCards.Count <= 0)
        {
            LoadCards();
        }
        _characters.ForEach(c =>
        {
            List<string> newCards = new List<string>();
            for (int i = 0; i < 5; i++) newCards.Add(_whiteCards.Pop());
            c.Hand.SetCards(newCards);
        });
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
        
        Debug.Log($"Current characters: {_characters.Count} & current judge index: {_currentJudgeIndex}");
        return _characters[_currentJudgeIndex];
    }
}