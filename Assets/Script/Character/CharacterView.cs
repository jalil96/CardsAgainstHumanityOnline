using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private CharacterModel _model;
    [SerializeField] private GameObject _selector;

    [SerializeField] private List<Transform> _selectorPositions;

    private void Update()
    {
        _selector.transform.position = _selectorPositions[_model.HandSelectorIndex].position;
    }
}