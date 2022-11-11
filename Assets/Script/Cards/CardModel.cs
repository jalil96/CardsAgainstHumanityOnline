using Photon.Pun;
using UnityEngine;

public class CardModel : MonoBehaviourPun
{

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }
}