using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class WinnerWhiteCardView : MonoBehaviourPun
{
    [SerializeField] private WinnerWhiteCardModel _model;
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _backgroundAnimator;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameObject _winnerInfo;
    [SerializeField] private TextMeshProUGUI _winnerNickname;


    private static readonly int Show = Animator.StringToHash("Show");

    private void Awake()
    {
        _model.OnShowCard += SetShowCard;
        _model.OnSetText += SetText;
    }

    private void SetText(string text, string nickname)
    {
        photonView.RPC(nameof(UpdateText), RpcTarget.All, text, nickname);
        
    }

    private void SetShowCard(bool show)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _animator.SetBool(Show ,show);
        _backgroundAnimator.SetBool(Show, show);
        photonView.RPC(nameof(UpdateWinnerInfo), RpcTarget.All, show);
        // Invoke(nameof(UnsetShowCard), 1f);
    }

    private void UnsetShowCard()
    {
        _animator.SetBool(Show ,false);
        _backgroundAnimator.SetBool(Show, false);
    }

    [PunRPC]
    private void UpdateText(string text, string nickname)
    {
        _text.text = text;
        _winnerNickname.text = nickname;
    }

    [PunRPC]
    private void UpdateWinnerInfo(bool show)
    {
        _winnerInfo.SetActive(show);
    }
}
