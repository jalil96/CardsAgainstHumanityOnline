using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistScoreData : MonoBehaviour
{
    public static PersistScoreData Instance;

    public List<PlayerData> playersData;
    public bool win;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    
}
