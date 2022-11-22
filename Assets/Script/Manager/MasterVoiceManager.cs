using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class MasterVoiceManager : MonoBehaviourPun
{
    private static MasterVoiceManager _instance;
    public static MasterVoiceManager Instance => _instance;

    private Dictionary<Player, VoiceController> _soundReference = new Dictionary<Player, VoiceController>();
    public List<string> playersAdded = new List<string>();

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RPCMaster(string methodName, params object[] p)
    {
        RPC(methodName, PhotonNetwork.MasterClient, p);
    }
    private void RPC(string methodName, Player target, params object[] p)
    {
        photonView.RPC(methodName, target, p);
    }

    #region SoundManagement
    public void AddSoundReference(VoiceController soundRef, Player client)
    {
        _soundReference[client] = soundRef;
        playersAdded.Add(client.NickName);
    }

    public void RemoveSoundReference(VoiceController soundRef, Player client)
    {
        if (_soundReference.ContainsKey(client))
        {
            _soundReference.Remove(client);
            playersAdded.Remove(client.NickName);
        }
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
