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
    [SerializeField] private GameManager _gameManager;
    
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
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
            
            photonView.RPC(nameof(ReferenceCharacter), player, characterModel.photonView.ViewID);
            
            MasterManager.Instance.AddCharacterModelReference(characterModel, player);
        }

        _gameManager.SetCharacters(_characters);
    }

    [PunRPC]
    private void ReferenceCharacter(int photonID)
    {
        var characters = FindObjectsOfType<CharacterModel>().ToList();
        var character = characters.Find(ch => ch.photonView.ViewID == photonID);

        if (character != null)
        {
            character.SetIsMine(true);
            character.Hand.SetIsMine(true);
            var handUI = FindObjectOfType<CharacterHandUI>();
            handUI.SetCharacterHand(character.Hand);
        }
    }
    
}