using System;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private CardModel _cardModel;
    [SerializeField] private Animator _animator;
    
    private static readonly int Hovered = Animator.StringToHash("Hovered");


    private void Start()
    {
        _cardModel.OnHovered += SetHovered;
    }

    private void SetHovered(bool hovered)
    {
        _animator.SetBool(Hovered, hovered);
    }
}