
using System;
using UnityEngine;

public class BlackCardRoundAction : RoundAction
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            EndRoundAction.Invoke();
        }
    }
}