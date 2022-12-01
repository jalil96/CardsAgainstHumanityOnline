using System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class JudgeView : MonoBehaviour
{
    [SerializeField] private JudgeModel _model;
    [SerializeField] private TextMeshProUGUI _text;
    
    private void Start()
    {
        _model.OnNicknameUpdated += UpdateNickname;
    }

    private void UpdateNickname()
    {
        _text.text = _model.Nickname;
    }
}