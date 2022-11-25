using System;
using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TimerModel _model;
    
    private void Update()
    {
        TimeSpan time = TimeSpan.FromSeconds(_model.TimeElapsed);
        _text.text = time.ToString("mm':'ss");
    }
}
