using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class BlackCardView : MonoBehaviourPun
{

    [SerializeField] private BlackCardModel _model;
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _backgroundAnimator;
    [SerializeField] private TextMeshProUGUI _text;
    
    private static readonly int Show = Animator.StringToHash("Show");

    private void Awake()
    {
        _model.OnShowCard += SetShowCard;
        _model.OnSetText += SetText;
    }

    private void SetText(string text)
    {
        photonView.RPC(nameof(UpdateText), RpcTarget.All, text);
    }

    private void SetShowCard(bool show)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _animator.SetBool(Show ,show);
        _backgroundAnimator.SetBool(Show, show);
        // Invoke(nameof(UnsetShowCard), 1f);
    }

    private void UnsetShowCard()
    {
        _animator.SetBool(Show ,false);
        _backgroundAnimator.SetBool(Show, false);
    }

    [PunRPC]
    private void UpdateText(string text)
    {
        _text.text = text;
    }
}
