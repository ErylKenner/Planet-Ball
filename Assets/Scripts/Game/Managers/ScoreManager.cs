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

    public readonly SyncList<int> TeamScores = new SyncList<int>();

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

        if (isServer)
        {
            for (int i = 0; i < ContextManager.instance.TeamManager.NumberOfTeams(); ++i)
            {
                TeamScores.Add(0);
            }
        }
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
    public void TeamScored(int teamNumber)
    {
        if (GameOver)
        {
            return;
        }

        Team team = ContextManager.instance.TeamManager.GetTeam(teamNumber);
        TeamScores[teamNumber] += 1;

        if (TeamScores[teamNumber] >= WinningScore)
        {
            GameOver = true;
            RpcTeamWon(team.TeamName, team.TeamColor);
        }
        else
        {
            NetcodeManager.instance.ResetState(ScoreFreezeTime);
            RpcTeamScored(team.TeamName, team.TeamColor);
        }
    }

    public int GetTeamScore(int teamNumber)
    {
        return TeamScores[teamNumber];
    }


    [ClientRpc]
    public void RpcTeamWon(string teamName, Color teamColor)
    {
        ContextManager.instance.SoundManager.Play("Victory", 0.05f);
        ContextManager.instance.SoundManager.Play("Goal");
        ContextManager.instance.SoundManager.Play("Fireworks", 0.05f);
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
        ContextManager.instance.SoundManager.Play("Goal");
        ContextManager.instance.SoundManager.Play("Fireworks", 0.05f);
        string scoreText = teamName + " Team Goal!";
        Debug.Log(scoreText);
        scoredText.text = scoreText;
        scoredText.color = teamColor;
        scoredText.gameObject.SetActive(true);
        scoreTimer = ScoreFreezeTime;
    }
}
