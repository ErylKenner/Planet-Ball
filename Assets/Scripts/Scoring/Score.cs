using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Score {
    public static int scoreTeam1 = 0;
    public static int scoreTeam2 = 0;

    public static void AddToScore(int teamNumber, int amount = 1)
    {
        if(teamNumber == 1)
        {
            scoreTeam1 += amount;
        } else if(teamNumber == 2)
        {
            scoreTeam2 += amount;
        } else
        {
            Debug.LogError("Invalid team number!");
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

}
