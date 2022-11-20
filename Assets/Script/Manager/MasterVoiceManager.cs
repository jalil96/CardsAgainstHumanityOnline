using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterVoiceManager : MonoBehaviourPun
{
    private static MasterVoiceManager _instance;
    public static MasterVoiceManager Instance => _instance;

    private Dictionary<Player, VoiceController> _soundReference = new Dictionary<Player, VoiceController>();

    private void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
    }

    #region SoundManagment
    public void AddSoundReference(VoiceController soundRef, Player client)
    {
        _soundReference[client] = soundRef;
    }

    public void RemoveSoundReference(VoiceController soundRef, Player client)
    {
        if (_soundReference.ContainsKey(client))
            _soundReference.Remove(client);
    }

    [PunRPC]
    public void RequestUpdateSoundStatus(Player client, bool isSoundOpen)
    {
        if (_soundReference.ContainsKey(client))
        {
            _soundReference[client].EnabelSoundSystem(isSoundOpen);
        }
    }
    #endregion
}
