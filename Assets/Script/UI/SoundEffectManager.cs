using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviourPun
{
    public GameObject airHornSound;
    public float timer = 2f;

    void Awake()
    {
        airHornSound.SetActive(false);
    }

    public void SendPlaySoundHorn()
    {
        photonView.RPC(nameof(PlayTheSoundHorn), RpcTarget.Others);
    }

    private IEnumerator PlayItemParty(GameObject sound, float time)
    {
        sound.SetActive(true);

        yield return new WaitForSeconds(time);

        sound.SetActive(false);
    }

    [PunRPC]
    public void PlayTheSoundHorn()
    {
        StartCoroutine(PlayItemParty(airHornSound, timer));
    }

}
