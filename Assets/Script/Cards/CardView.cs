using System;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private UICard _uiCard;
    [SerializeField] private CardModel _cardModel;

    private void Start()
    {
        _uiCard.text.text = _cardModel.Text;
    }

    public void SetText(string text)
    {
        _uiCard.text.text = text;
    }
}