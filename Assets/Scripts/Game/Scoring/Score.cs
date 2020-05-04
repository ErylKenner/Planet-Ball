using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{

    public static int scoreTeam1 = 0;
    public static int scoreTeam2 = 0;

    private readonly static int scoreGoal = 3;

    private static Score instance;

    public Color team1Color;
    public Color team2Color;

    public EndScreen endScreen;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void AddToScore(int teamNumber, int amount = 1)
    {
        if (teamNumber == 1)
        {
            scoreTeam1 += amount;
        }
        else if (teamNumber == 2)
        {
            scoreTeam2 += amount;
        }
        else
        {
            Debug.LogError("Invalid team number!");
            return;
        }

        if (scoreTeam1 >= scoreGoal || scoreTeam2 >= scoreGoal)
        {
            GameEnd(teamNumber);
        }

    }

    public static int GetScore(int teamNumber)
    {
        if (teamNumber == 1)
        {
            return scoreTeam1;
        }
        else if (teamNumber == 2)
        {
            return scoreTeam2;
        }
        else
        {
            Debug.LogError("Invalid team number!");
            return -1;
        }
    }

    public static Color GetColor(int teamNumber)
    {
        if (teamNumber == 1)
        {
            return instance.team1Color;
        }
        else if (teamNumber == 2)
        {
            return instance.team2Color;
        }
        else
        {
            Debug.LogError("Invalid team number!");
            return Color.black;
        }
    }

    public static void GameEnd(int teamNumber)
    {
        instance.endScreen.EndGame(teamNumber);
    }

}
