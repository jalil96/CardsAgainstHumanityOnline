using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Photon.Pun.UtilityScripts;


public class PlayerData
{
    public int score;
    public string nickname;

    public PlayerData(int score, string nickname)
    {
        this.score = score;
        this.nickname = nickname;
    }
}

public class ScoreManager : MonoBehaviour
{
    private const string victoryMessage = "Victory";
    private const string gameOverMessage = "Game Over";

    public PlayerScore playerScorePrefab;
    public GameObject playerListContainer;
    public Button mainMenuButton;

    public Text titleTxt;

    [SerializeField] private List<PlayerScore> _playerScoreList;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(BackToMainMenu);
        playerScorePrefab.gameObject.SetActive(false);

        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["WinCondition"]);
        
        SetTitle(PhotonNetwork.LocalPlayer.GetScore() >= (int)PhotonNetwork.CurrentRoom.CustomProperties["WinCondition"]);
        // SetPlayersPrefab(PersistScoreData.Instance.playersData.Count);
        SetScores();
    }

    public void SetTitle(bool win)
    {
        if (win)
            titleTxt.text = victoryMessage;
        else
            titleTxt.text = gameOverMessage;
    }

    public void SetPlayersPrefab(int quantityPlayer)
    {
        playerScorePrefab.gameObject.SetActive(false);
        for (int i = 0; i < quantityPlayer; i++)
        {
            PlayerScore aux = Instantiate(playerScorePrefab, playerListContainer.transform, false);
            aux.gameObject.SetActive(false);
            _playerScoreList.Add(aux);
        }
    }

    public void SetScores()
    {

        var players = PhotonNetwork.PlayerList;

        List<PlayerData> playerList = new List<PlayerData>();
        
        foreach (var player in players)
        {
            if(player.IsMasterClient) continue;
            playerList.Add(new PlayerData(player.GetScore(), player.NickName));
        }
        
        IEnumerable orderedList =  playerList.OrderBy(o => o.score).Reverse();

        List<PlayerData> auxList =  new List<PlayerData>();

        foreach (PlayerData item in orderedList)
        {
            auxList.Add(item);
        }

        RefreshList(auxList);
    }

    public void RefreshList(List<PlayerData> playerList)
    {
        for (int i = 0; i < _playerScoreList.Count; i++)
        {
            if((playerList.Count() - 1) < i)
            {
                _playerScoreList[i].gameObject.SetActive(false);
                continue;
            }

            _playerScoreList[i].gameObject.SetActive(true);
            _playerScoreList[i].positionTxt.text = $"{i + 1} - ";
            _playerScoreList[i].nameTxt.text = playerList[i].nickname;
            _playerScoreList[i].scoreTxt.text = playerList[i].score.ToString();
        }
    }

    public void BackToMainMenu()
    {
        // PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("MainMenu");
    }
    
}
