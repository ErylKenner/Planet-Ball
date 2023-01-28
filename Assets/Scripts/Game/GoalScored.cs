using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScored : NetworkBehaviour
{
    public string TeamName;
    public int TeamNumber;
    public Color TeamColor;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer)
        {
            return;
        }
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            ScoreManager scoreManager = NetcodeManager.instance.GetComponent<ScoreManager>();
            scoreManager.TeamScored(TeamNumber, TeamColor, TeamName);
        }
    }
}
