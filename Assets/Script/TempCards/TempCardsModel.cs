using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TempCardsModel : MonoBehaviour
{
    private List<TempCard> _tempCardsModel = new List<TempCard>();
    
    public void AddTempCard(TempCard tempCard)
    {
        tempCard.OnDestroy += TempCardDestroy;
        _tempCardsModel.Add(tempCard);
    }

    public void SendCardsToCharacter(Transform character)
    {
        _tempCardsModel.ForEach(tc => tc.TravelToCharacter(character, true));
    }

    private void TempCardDestroy(TempCard tempCard)
    {
        if (_tempCardsModel.Contains(tempCard))
        {
            _tempCardsModel.Remove(tempCard);
        }
        PhotonNetwork.Destroy(tempCard.gameObject);
    }
}