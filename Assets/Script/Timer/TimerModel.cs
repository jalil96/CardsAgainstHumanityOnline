using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TimerModel : MonoBehaviourPun
{
    private float _timeElapsed;
    private bool _started;

    public float TimeElapsed => _timeElapsed;
    
    private void Update()
    {
        if (_started) _timeElapsed += Time.deltaTime;
    }

    public void RestartTimer()
    {
        _started = false;
        _timeElapsed = 0;
    }

    public void StartTimer()
    {
        _started = true;
        InvokeRepeating(nameof(BroadcastTime), 0f, 1f);
        photonView.RPC(nameof(UpdateStarted), RpcTarget.Others, _started);
    }

    public void StopTimer()
    {
        _started = false;
        CancelInvoke(nameof(BroadcastTime));
        photonView.RPC(nameof(UpdateStarted), RpcTarget.Others, _started);
    }

    private void BroadcastTime()
    {
        photonView.RPC(nameof(UpdateTimeElapsed), RpcTarget.Others, _timeElapsed);
    }
    
    [PunRPC]
    private void UpdateTimeElapsed(float time)
    {
        _timeElapsed = time;
    }

    [PunRPC]
    private void UpdateStarted(bool started)
    {
        _started = started;
    }
}
