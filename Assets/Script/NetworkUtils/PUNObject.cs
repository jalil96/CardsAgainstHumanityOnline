using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public abstract class PUNObject : MonoBehaviourPun
{
    public Player Client { get; set; }

}