using System;
using UnityEngine;

public class RoundAction : MonoBehaviour, IRoundAction
{
    public Action OnStartRoundAction;
    public Action OnEndRoundAction;

    public virtual void StartRoundAction()
    {
        
    }

    public virtual void EndRoundAction()
    {
        
    }
}