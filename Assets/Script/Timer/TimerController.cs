using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TimerController : MonoBehaviourPun
{
    [SerializeField] private TimerModel _model;
    
    private void Start()
    {
        if (!photonView.IsMine) Destroy(this);
        _model.RestartTimer();
    }

    public void StartTimer()
    {
        _model.StartTimer();
    }

    public void StopTimer()
    {
        _model.StopTimer();
    }
}
