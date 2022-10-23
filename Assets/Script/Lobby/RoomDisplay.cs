using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomDisplay : MonoBehaviour
{
    public Text nameTxt;
    public Text numberTxt;
    public Button joinButton;
    public RoomInfo RoomInfo { get; set; }
}
