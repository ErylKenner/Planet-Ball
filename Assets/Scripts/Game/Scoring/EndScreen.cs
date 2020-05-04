using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{

    public Image screen;
    public Text text;
    public Text scoreText;

    public bool Ended = false;

    public void EndGame(int teamNumber)
    {
        Time.timeScale = 0;
        Ended = true;
        screen.gameObject.SetActive(true);
        screen.color = Score.GetColor(teamNumber);
        text.text = "Player " + teamNumber + " wins!";
        int otherTeam = teamNumber == 1 ? 2 : 1;
        scoreText.text = Score.GetScore(teamNumber) + " - " + Score.GetScore(otherTeam);
    }

    private void Update()
    {
        if (Ended)
        {
            Player[] players = InputAssign.players;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].ControllerInput == null)
                {
                    continue;
                }

                if (Input.GetButtonDown(players[i].ControllerInput.Button("Start")))
                {
                    Time.timeScale = 1;
                    InputAssign.currentPlayer = 1;
                    Score.ScoreTeam1 = 0;
                    Score.ScoreTeam2 = 0;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
}
