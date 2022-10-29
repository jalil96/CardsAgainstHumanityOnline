
using System;
using UnityEngine;

public class WhiteCardRoundAction : RoundAction
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            EndRoundAction.Invoke();
        }
    }
}