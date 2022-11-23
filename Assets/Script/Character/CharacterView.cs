using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private CharacterModel _model;
    [SerializeField] private GameObject _selector;

    [SerializeField] private List<Transform> _selectorPositions;
    
    private CharacterHandUI _handUI;

    private void Awake()
    {
        _model.OnShowCards += ShowSelector;
        _model.OnHideCards += HideSelector;
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
}