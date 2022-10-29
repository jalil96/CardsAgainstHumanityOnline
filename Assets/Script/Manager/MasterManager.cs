using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterManager : MonoBehaviour
{
    private static MasterManager _instance;
    public static MasterManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
    }

    public void AddInstantiatedObject(PUNObject punObject)
    {
        
    }
}
