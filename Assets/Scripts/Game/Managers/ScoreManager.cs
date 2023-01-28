using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class ScoreManager : NetworkBehaviour
{
    public int WinningScore = 3;
    public float ScoreFreezeTime = 3;
    public float WinFreezeTime = 3;

    [SyncVar]
    public int Team1Score = 0;

    [SyncVar]
    public int Team2Score = 0;

    [SyncVar]
    public bool GameOver = false;

    private float scoreTimer = 0;
    private float victoryTimer = 0;
    private float originalFixedDeltaTime;
    TMPro.TextMeshProUGUI scoredText;

    void Start()
    {
        UIAccessor accessor = FindObjectOfType<UIAccessor>();
        scoredText = accessor.ScoredText;
        scoredText.gameObject.SetActive(false);
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        if (scoreTimer > 0)
        {
            scoreTimer -= Time.deltaTime;
            if (scoreTimer <= 0)
            {
                scoreTimer = 0;
                scoredText.gameObject.SetActive(false);
            }
        }
        if (victoryTimer > 0)
        {
            float t = Mathf.Clamp(1f - (victoryTimer / WinFreezeTime), 0, 1);
            Time.timeScale = Mathf.Lerp(0.5f, 0.15f, t);

            // TOOD: Decresing FDT significantly increases the number of server ticks per frame which can cause lag
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            victoryTimer -= Time.unscaledDeltaTime;
            if (victoryTimer <= 0)
            {
                // Go back to main menu
                Time.timeScale = 1;
                Time.fixedDeltaTime = originalFixedDeltaTime;
                CustomRoomManager customRoomManager = FindObjectOfType<CustomRoomManager>();
                customRoomManager.ServerChangeScene(customRoomManager.RoomScene);

                Debug.Log("Go back to lobby");
            }
        }
    }

    [Server]
    public void TeamScored(int teamNumber, Color teamColor, string teamName)
    {
        if (GameOver)
        {
            return;
        }

        // TODO: Lookup team info from TeamManager

        if (teamNumber == 0)
        {
            Team1Score += 1;
        } else if(teamNumber == 1)
        {
            Team2Score += 1;
        }
        else
        {
            Debug.LogError("Invalid team number given: " + teamNumber);
            return;
        }

        if (Team1Score >= WinningScore || Team2Score >= WinningScore)
        {
            GameOver = true;
            RpcTeamWon(teamName, teamColor);
        }
        else
        {
            NetcodeManager.instance.ResetState(ScoreFreezeTime);
            RpcTeamScored(teamName, teamColor);
        }
    }

    public int GetTeamScore(int teamNumber)
    {
        if (teamNumber == 0)
        {
            return Team1Score;
        }
        else if (teamNumber == 1)
        {
            return Team2Score;
        }
        else
        {
            Debug.Log("Invalid team number given: " + teamNumber);
            return 0;
        }
    }


    [ClientRpc]
    public void RpcTeamWon(string teamName, Color teamColor)
    {
        string scoreText = teamName + " Team Victory!";
        Debug.Log(scoreText);
        scoredText.text = scoreText;
        scoredText.color = teamColor;
        scoredText.gameObject.SetActive(true);
        victoryTimer = WinFreezeTime;
    }

    [ClientRpc]
    public void RpcTeamScored(string teamName, Color teamColor)
    {
        string scoreText = teamName + " Team Goal!";
        Debug.Log(scoreText);
        scoredText.text = scoreText;
        scoredText.color = teamColor;
        scoredText.gameObject.SetActive(true);
        scoreTimer = ScoreFreezeTime;
    }
}
