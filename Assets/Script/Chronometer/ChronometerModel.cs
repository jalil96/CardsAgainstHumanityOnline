using System;
using Photon.Pun;
using UnityEngine;

public class ChronometerModel : MonoBehaviourPun
{
    public Action OnTimeElapsed = delegate {};
    
    private float _timeToElapse;
    private bool _started;

    public float TimeToElapse => _timeToElapse;
    
    private void Update()
    {
        if (!_started) return;
        _timeToElapse -= Time.deltaTime;
        if (_timeToElapse <= 0)
        {
            OnTimeElapsed.Invoke();
        }
    }

    public void RestartChronometer(float time)
    {
        _started = false;
        _timeToElapse = time;
    }

    public void StartChronometer(float timeToElapse)
    {
        Debug.Log("Started chronometer");
        _started = true;
        _timeToElapse = timeToElapse;
        InvokeRepeating(nameof(BroadcastTime), 0f, 1f);
        photonView.RPC(nameof(UpdateStarted), RpcTarget.Others, _started);
    }

    public void StopChronometer()
    {
        Debug.Log("Stoped chronometer");
        _started = false;
        CancelInvoke(nameof(BroadcastTime));
        photonView.RPC(nameof(UpdateStarted), RpcTarget.Others, _started);
    }

    public void AddTime(float time)
    {
        _timeToElapse += time;
    }
    
    private void BroadcastTime()
    {
        photonView.RPC(nameof(UpdateTimeElapsed), RpcTarget.Others, _timeToElapse);
    }
    
    [PunRPC]
    private void UpdateTimeElapsed(float time)
    {
        _timeToElapse = time;
    }

    [PunRPC]
    private void UpdateStarted(bool started)
    {
        _started = started;
    }
}