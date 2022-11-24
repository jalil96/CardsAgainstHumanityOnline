using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private CharacterModel _model;
    [SerializeField] private GameObject _selector;

    [SerializeField] private List<Transform> _selectorPositions;
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _points;
    
    private CharacterHandUI _handUI;

    private void Awake()
    {
        _model.OnShowCards += ShowSelector;
        _model.OnHideCards += HideSelector;
        _model.OnNickNameUpdated += UpdateNickName;
        _model.OnPointsUpdated += UpdatePoints;
    }

    private void Start()
    {
        _playerName.text = _model.NickName;
        _playerName.transform.rotation = Quaternion.identity;
        _points.transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        _selector.transform.position = _selectorPositions[_model.HandSelectorIndex].position;
    }

    private void ShowSelector()
    {
        Debug.Log("Showing selector");
        _selector.SetActive(true);
    }

    private void HideSelector()
    {
        Debug.Log("Hiding selector");
        _selector.SetActive(false);
    }

    private void UpdateNickName()
    {
        _playerName.text = _model.NickName;
    }

    private void UpdatePoints()
    {
        _points.text = _model.Points.ToString();
    }
}