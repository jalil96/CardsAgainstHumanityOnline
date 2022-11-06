using Photon.Pun;
using UnityEngine;

public class CardModel : MonoBehaviourPun
{
    [PunRPC]
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void Activate()
    {
        gameObject.SetActive(true);
    }
}