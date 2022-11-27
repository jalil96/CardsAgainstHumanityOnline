using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PartyEffectManager : MonoBehaviourPun
{
    public List<GameObject> partyEffect;
    public float timer = 2f;

    void Start()
    {
        CommunicationsManager.Instance.inputManager.SetSceneAsGameplay();
        CommunicationsManager.Instance.commandManager.PartyPopper += StartPartyEffect;
        SetEffects(false);
    }

    private void OnDestroy()
    {
        CommunicationsManager.Instance.commandManager.PartyPopper -= StartPartyEffect;
    }

    private void StartPartyEffect()
    {
        StartCoroutine(RunParty(timer));
        photonView.RPC(nameof(SetOffFireworks), RpcTarget.Others);
    }

    private IEnumerator RunParty(float time)
    {
        SetEffects(true);

        yield return new WaitForSeconds(time);

        SetEffects(false);
    }

    private void SetEffects(bool value)
    {
        for (int i = 0; i < partyEffect.Count; i++)
        {
            partyEffect[i].SetActive(value);
        }
    }

    [PunRPC]
    public void SetOffFireworks()
    {
        StartCoroutine(RunParty(timer));
    }
}
