using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{

    public Image screen;
    public Text text;
    public Text scoreText;

    private bool ended = false;

    public void EndGame(int teamNumber)
    {
        Time.timeScale = 0;
        ended = true;
        screen.gameObject.SetActive(true);
        screen.color = Score.GetColor(teamNumber);
        text.text = "Player " + teamNumber + " wins!";
        int otherTeam = teamNumber == 1 ? 2 : 1;
        scoreText.text = Score.GetScore(teamNumber) + " - " + Score.GetScore(otherTeam);
    }

    private void Update()
    {
        if (ended)
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
                    Score.scoreTeam1 = 0;
                    Score.scoreTeam2 = 0;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
    }
}
