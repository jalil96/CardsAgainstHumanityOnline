using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class CharacterInstantiator : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<CharacterModel> _characters = new List<CharacterModel>();
    [SerializeField] private List<SpawnPoint> _spawnPoints;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Destroy(gameObject);
            return;
        }

        var players = PhotonNetwork.PlayerList;

        foreach (var player in players)
        {
            if (player.IsMasterClient) continue;
            var spawnPoint = _spawnPoints.First(spawnPoint => !spawnPoint.Occupied);

            spawnPoint.SetOccupied(true);
        
            var spawnPointT = spawnPoint.transform;
        
            var character = PhotonNetwork.Instantiate("Character", spawnPointT.position, spawnPointT.localRotation);
            var characterModel = character.GetComponent<CharacterModel>();
            _characters.Add(characterModel);
            
            MasterManager.Instance.AddCharacterModelReference(characterModel, player);
        }
    }
    
}