using Photon.Pun;
using UnityEngine;

public class TempCardsController : MonoBehaviour
{
    [SerializeField] private TempCardsModel _model;
    [SerializeField] private Transform _middlePosition;
    
    public void SpawnTempCard(Vector3 position, Quaternion rotation, bool sendItToMiddle)
    {
        var tempCard = PhotonNetwork.Instantiate("TempCard", position, rotation).GetComponent<TempCard>();
        tempCard.SetMiddlePosition(_middlePosition);
        _model.AddTempCard(tempCard);
        if (sendItToMiddle)
        {
            tempCard.TravelToMiddle();
        }
    }

    public void SendCardsToCharacter(Transform character)
    {
        _model.SendCardsToCharacter(character);
    }
}