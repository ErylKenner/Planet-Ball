using UnityEngine;

public class Score : MonoBehaviour
{

    public static int ScoreTeam1 = 0;
    public static int ScoreTeam2 = 0;

    public readonly static int ScoreToWin = 3;

    public static Score Instance;

    public Color team1Color = new Color32(25, 10, 218, 255);
    public Color team2Color = new Color32(218, 10, 10, 255);

    public EndScreen EndScreen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
            ScoreTeam1 += amount;
        }
        else if (teamNumber == 2)
        {
            ScoreTeam2 += amount;
        }
        else
        {
            Debug.LogError("Invalid team number!");
            return;
        }

        if (ScoreTeam1 >= ScoreToWin || ScoreTeam2 >= ScoreToWin)
        {
            GameEnd(teamNumber);
        }

    }

    public static int GetScore(int teamNumber)
    {
        if (teamNumber == 1)
        {
            return ScoreTeam1;
        }
        else if (teamNumber == 2)
        {
            return ScoreTeam2;
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
            return Instance.team1Color;
        }
        else if (teamNumber == 2)
        {
            return Instance.team2Color;
        }
        else
        {
            Debug.LogError("Invalid team number!");
            return Color.black;
        }
    }

    public static void GameEnd(int teamNumber)
    {
        Instance.EndScreen.EndGame(teamNumber);
    }

}
