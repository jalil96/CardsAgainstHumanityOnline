using System;
using Photon.Pun;
using UnityEngine;


public class ChronometerController : MonoBehaviourPun
{
    public Action OnChronometerTimeElapsed = delegate {};
    
    [SerializeField] private ChronometerModel _model;
    
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) Destroy(this);
        // _model.RestartChronometer(0f);
        _model.OnTimeElapsed += ChronometerTimeElapsed;
    }

    private void ChronometerTimeElapsed()
    {
        OnChronometerTimeElapsed.Invoke();
    }

    public void StartChronometer(float timeToElapse)
    {
        _model.StartChronometer(timeToElapse);
    }

    public void StopChronometer()
    {
        _model.StopChronometer();
    }

    public void AddTime(float time)
    {
        _model.AddTime(time);
    }
    
}