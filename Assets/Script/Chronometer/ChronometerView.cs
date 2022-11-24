using System;
using TMPro;
using UnityEngine;

public class ChronometerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private ChronometerModel _model;
    
    private void Update()
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0, _model.TimeToElapse));
        _text.text = time.ToString("mm':'ss");
    }
}