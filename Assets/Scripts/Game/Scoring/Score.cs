using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Score : MonoBehaviour
{
    public List<Team> Teams;
    public int ScoreToWin = 3;

    private static Score instance;

    public delegate void EndGame(int teamNumber);
    public static event EndGame OnEndGame;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Goal.OnBallScored += OnBallScored;

        Teams = new List<Team>();
        Teams.Add(new Team(1, new Color32(25, 10, 218, 255)));
        Teams.Add(new Team(2, new Color32(218, 10, 10, 255)));

    }

    public static Team GetTeam(int teamNumber)
    {
        return instance.Teams.Find(x => x.TeamNumber == teamNumber);
    }

    public static Color GetColor(int teamNumber)
    {
        return GetTeam(teamNumber).TeamColor;
    }

    public static int GetScore(int teamNumber)
    {
        return GetTeam(teamNumber).Score;
    }

    public static string GetScoresTextLong()
    {
        string ret = "";
        for (int i = 0; i < instance.Teams.Count - 1; ++i)
        {
            ret += "Player " + instance.Teams.ElementAt(i).TeamNumber + "  -  " + instance.Teams.ElementAt(i).Score + " goals\n";
        }
        ret += "Player " + instance.Teams.Last().TeamNumber + "  -  " + instance.Teams.Last().Score;
        return ret;
    }

    public static string GetScoresTextShort()
    {
        string ret = "";
        for (int i = 0; i < instance.Teams.Count - 1; ++i)
        {
            ret += instance.Teams.ElementAt(i).Score + " - ";
        }
        ret += instance.Teams.Last().Score;
        return ret;
    }

    public static void ResetScores()
    {
        foreach (Team team in instance.Teams)
        {
            team.Score = 0;
        }
    }

    private Team GetWinner()
    {
        foreach (Team team in Teams)
        {
            if (team.Score >= ScoreToWin)
            {
                return team;
            }
        }
        return null;
    }

    private void OnBallScored(int teamNumber)
    {
        GetTeam(teamNumber).Score += 1;
        if (GetWinner() != null)
        {
            OnEndGame?.Invoke(teamNumber);
        }
    }
}
