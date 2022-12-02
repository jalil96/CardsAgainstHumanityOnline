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

            characterModel.SetNickName(player.NickName);
            
            _characters.Add(characterModel);

            photonView.RPC(nameof(ReferenceCharacter), player, characterModel.photonView.ViewID);
            
            MasterManager.Instance.AddCharacterModelReference(characterModel, player);
        }

        _gameManager.SetCharacters(_characters);
        _gameManager.SetNewWhiteCards();
        
        // _characters.ForEach(c => photonView.RPC(nameof(DeleteOtherControllers), MasterManager.Instance.GetPlayerFromCharacter(c), c.photonView.ViewID));
    }

    [PunRPC]
    private void ReferenceCharacter(int photonID)
    {
        var characters = FindObjectsOfType<CharacterModel>().ToList();
        var character = characters.Find(ch => ch.photonView.ViewID == photonID);

        characters.Remove(character);

        if (character != null)
        {
            character.SetIsMine(true);
            character.Hand.SetIsMine(true);
            var handUI = FindObjectOfType<CharacterHandUI>();
            handUI.SetCharacterHand(character.Hand);
        }
    }

    [PunRPC]
    private void DeleteOtherControllers(int photonID)
    {
        if (PhotonNetwork.IsMasterClient) return;
        var characters = FindObjectsOfType<CharacterModel>().ToList();
        var character = characters.Find(ch => ch.photonView.ViewID == photonID);

        characters.Remove(character);
        
        characters.ForEach(c => c.GetComponent<CharacterController>().Destroy());
    }
    
}