using UnityEngine;

public class Team
{
    public int TeamNumber;
    public Color TeamColor;
    public int Score;

    public Team(int teamNumber, Color teamColor)
    {
        TeamNumber = teamNumber;
        TeamColor = teamColor;
        Score = 0;
    }
}